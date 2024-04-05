using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Robot.DataBase.Modeles;
using Robot.Mail;
using Robot.Server.Command;
using Robot.Server.Command.Api;
using Robot.Server.Command.Api.Request;
using Robot.Server.Command.Api.Result;
using Robot.Server.Command.Strategy;
using Robot.Server.Command.Strategy.Request;
using Robot.Server.Command.Strategy.Response;
using Robot.Server.Dto.Enum;
using Robot.Server.Dto.Response;
using Robot.Server.Exception;
using Robot.Server.Hubs;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.events;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.StrategyDynamiqCompiler;
using ILogger = Serilog.ILogger;

namespace Robot.Server.BackgroundService;

public class CommandHandler
{
    private readonly IEmailService _emailService;
    private readonly IHubContext<HubInfoClient, IHubInfoClient> _hubContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    private readonly Dictionary<string, StrategyBase> _strategyList = new();
    private readonly Dictionary<string, CustomLoadContext> _strategyListContext = new();

    private IApiHandler? _apiHandlerBase;

    public CommandHandler(ILogger logger, IMapper mapper, IEmailService emailService,
        IHubContext<HubInfoClient, IHubInfoClient> hubContext)
    {
        _logger = logger.ForContext<CommandHandler>();
        _mapper = mapper;
        _emailService = emailService;
        _hubContext = hubContext;
    }

    public async Task HandleApiCommand(ServiceCommandeBaseApiAbstract command)
    {
        switch (command)
        {
            case GetTypeHandlerCommand getTypeHandlerCommand:
                GetTypeHandler(getTypeHandlerCommand);
                _logger.Information("Api command processed {@Command}", getTypeHandlerCommand);
                break;
            case GetAllSymbolCommand getAllSymbolCommand:
                await GetAllSymbol(getAllSymbolCommand);
                _logger.Information("Api command processed {@Command}", nameof(getAllSymbolCommand));
                break;
            case IsConnectedCommand connectedCommand:
                IsConnected(connectedCommand);
                _logger.Information("Api command processed {@Command}", connectedCommand);
                break;
            case ApiConnectCommand apiConnectCommand:
                await Connect(apiConnectCommand);
                _logger.Information("Api command processed {@Command}", apiConnectCommand);
                break;
            case DisconnectCommand disconnectCommand:
                CheckApiHandlerNotNull();
                await Disconnect(disconnectCommand);
                _logger.Information("Api command processed {@Command}", disconnectCommand);
                break;
            default:
                _logger.Error("Trying to use unhandled command {@Command}", command);
                throw new CommandException("Internal error");
        }
    }


    public async Task HandleStrategyCommand(ServiceCommandeBaseStrategyAbstract command)
    {
        switch (command)
        {
            case InitStrategyCommand initStrategyCommandDto:
                CheckApiHandlerNotNull();
                InitStrategy(initStrategyCommandDto);
                _logger.Information("Strategy command processed {@Command}", initStrategyCommandDto);
                break;
            case GetStrategyInfoCommand getStrategyInfoCommand:
                GetStrategyInfo(getStrategyInfoCommand, GetStrategyById(getStrategyInfoCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getStrategyInfoCommand);
                break;
            case CloseStrategyCommand closeStrategyCommand:
                await CloseStrategy(closeStrategyCommand, GetStrategyById(closeStrategyCommand.Id));
                _logger.Information("Strategy command processed {@Command}", closeStrategyCommand);
                break;
            case GetStrategyPositionClosedCommand getStrategyPositionClosedCommand:
                GetStrategyPositionClosed(getStrategyPositionClosedCommand,
                    GetStrategyById(getStrategyPositionClosedCommand.Id));
                _logger.Information("Strategy command processed {@Command}", nameof(getStrategyPositionClosedCommand));
                break;
            case GetStrategyResultRequestCommand getStrategyResultRequestCommand:
                GetStrategyResult(getStrategyResultRequestCommand, GetStrategyById(getStrategyResultRequestCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getStrategyResultRequestCommand);
                break;
            case GetOpenedPositionRequestCommand getOpenedPositionRequestCommand:
                GetOpenedPosition(getOpenedPositionRequestCommand, GetStrategyById(getOpenedPositionRequestCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getOpenedPositionRequestCommand);
                break;
            case SetCanRunCommand setCanRunCommand:
                SetCanRun(setCanRunCommand, GetStrategyById(setCanRunCommand.Id));
                _logger.Information("Strategy command processed {@Command}", setCanRunCommand);
                break;
            case GetChartCommandRequest getChartCommandRequest:
                GetChart(getChartCommandRequest, GetStrategyById(getChartCommandRequest.Id));
                _logger.Information("Strategy command processed {@Command}", getChartCommandRequest);
                break;
            case GetAllStrategyCommandRequest getAllStrategyCommandRequest:
                GetAllStrategy(getAllStrategyCommandRequest);
                _logger.Information("Strategy command processed {@Command}", getAllStrategyCommandRequest);
                break;
            case RunStrategyBacktestCommand runStrategyBacktestCommand:
                await RunBackTest(runStrategyBacktestCommand, GetStrategyById(runStrategyBacktestCommand.Id));
                _logger.Information("Api command processed {@Command}", runStrategyBacktestCommand);
                break;
            case GetStrategyResultBacktestCommand strategyResultBacktestCommand:
                await GetStrategyBacktestResult(strategyResultBacktestCommand,
                    GetStrategyById(strategyResultBacktestCommand.Id));
                _logger.Information("Api command processed {@Command}", strategyResultBacktestCommand);
                break;

            default:
                _logger.Error("Trying to use unanhdled command {@Command}", command);
                throw new CommandException("Internal error");
        }
    }


    private void CheckApiHandlerNotNull()
    {
        if (_apiHandlerBase is null) throw new CommandException("The Api handler is not connected");
    }

    private StrategyBase GetStrategyById(string id)
    {
        if (_strategyList.TryGetValue(id, out var strategyBase))
            return strategyBase;
        throw new CommandException($"The strategy {id} is not initialized");
    }

    public async Task Shutdown()
    {
        if (_apiHandlerBase is not null) await _apiHandlerBase.DisconnectAsync();
    }


    #region StrategyCommand

    private Task GetStrategyBacktestResult(GetStrategyResultBacktestCommand strategyResultBacktestCommand,
        StrategyBase strategy)
    {
        if (strategy.BackTest.BacktestRunning)
        {
            strategyResultBacktestCommand.ResponseSource.SetResult(new GetStrategyResultCommandBacktestResponse
            {
                BackTestDto = new BackTestDto
                {
                    IsBackTestRunning = true,
                    LastBackTestExecution = strategy.BackTest.LastBacktestExecution
                }
            });
        }
        else
        {
            var response = _mapper.Map<BackTestDto>(strategy.BackTest);
            strategyResultBacktestCommand.ResponseSource.SetResult(new GetStrategyResultCommandBacktestResponse
            {
                BackTestDto = response
            });
        }

        return Task.CompletedTask;
    }


    private void GetAllStrategy(GetAllStrategyCommandRequest getAllStrategyCommandRequest)
    {
        var response = new GetAllStrategyCommandResponse();
        if (_strategyList is { Count: 0 })
        {
            getAllStrategyCommandRequest.ResponseSource.SetResult(response);
        }
        else
        {
            var listStrategy = new List<StrategyInfoDto>();
            foreach (var (key, value) in _strategyList)
            {
                var strategy = GetStrategyById(key);
                var strategyInfoDto = _mapper.Map<StrategyInfoDto>(strategy);
                listStrategy.Add(strategyInfoDto);
            }

            response.ListStrategyInfoDto = listStrategy;
            getAllStrategyCommandRequest.ResponseSource.SetResult(response);
        }
    }

    private void InitStrategy(InitStrategyCommand initStrategyCommandDto)
    {
        try
        {
            var strategyImplementation = GenerateStrategy(initStrategyCommandDto.StrategyFileDto);
            var istrategySerrvice = new StrategyServiceFactory();
            var strategyBase = new StrategyBase(strategyImplementation.Item1, initStrategyCommandDto.Symbol,
                initStrategyCommandDto.Timeframe, initStrategyCommandDto.timeframe2, _apiHandlerBase, _logger,
                istrategySerrvice);

            strategyBase.TickEvent += StrategyBaseOnTickEvent;
            strategyBase.CandleEvent += StrategyBaseOnCandleEvent;
            strategyBase.PositionOpenedEvent += StrategyBaseOnPositionOpenedEvent;
            strategyBase.PositionUpdatedEvent += StrategyBaseOnPositionUpdatedEvent;
            strategyBase.PositionClosedEvent += StrategyBaseOnPositionClosedEvent;
            strategyBase.PositionRejectedEvent += StrategyBaseOnPositionRejectedEvent;
            strategyBase.StrategyDisabledEvent += StrategyBaseOnStrategyDisabled;
            strategyBase.StrategyEvent += StrategyBaseOnStrategyEvent;

            _strategyList.Add(strategyBase.Id, strategyBase);
            _strategyListContext.Add(strategyBase.Id, strategyImplementation.Item2);

            initStrategyCommandDto.ResponseSource.SetResult(new AcknowledgementResponse());
        }
        catch (System.Exception e) when (e is not StrategyException)
        {
            throw;
        }
    }

    private async Task RunBackTest(RunStrategyBacktestCommand runStrategyBacktestCommand, StrategyBase strategy)
    {
        if (!strategy.BackTest.BacktestRunning)
            await strategy.RunBackTest(runStrategyBacktestCommand.Balance, runStrategyBacktestCommand.MinSpread,
                runStrategyBacktestCommand.MaxSpread);
        var backtestCommandResponse = new BacktestCommandResponse
        {
            BackTestDto = new BackTestDto
            {
                IsBackTestRunning = strategy.BackTest.BacktestRunning,
                LastBackTestExecution = strategy.BackTest.LastBacktestExecution.GetValueOrDefault(),
                ResultBacktest = _mapper.Map<ResultDto>(strategy.BackTest.Result)
            }
        };
        runStrategyBacktestCommand.ResponseSource.SetResult(backtestCommandResponse);
    }


    private void StrategyBaseOnStrategyDisabled(object? sender, RobotEvent<string> e)
    {
        _logger.Warning("{Message}, send email to user", e.EventField);
        _emailService.SendEmail("Strategy disabled", e.EventField).GetAwaiter().GetResult();
        //TODO : Refacto
        //StrategyDisabled?.Invoke(this, new RobotEvent<string>(e.EventField, e.Id));
    }

    private void StrategyBaseOnStrategyEvent(object? sender, RobotEvent<string> e)
    {
        //TODO : Refacto
        //   StrategyEvent?.Invoke(this, new BackGroundServiceEvent<string>(e.EventField, e.Id));
    }


    private void GetChart(GetChartCommandRequest chartCommandRequest, StrategyBase strategy)
    {
        var candles = strategy.History.Select(x => new CandleDto
        {
            Open = (double)x.Open,
            High = (double)x.High,
            Low = (double)x.Low,
            Close = (double)x.Close,
            Date = x.Date,
            Volume = (double)x.Volume
        }).ToList();
        chartCommandRequest.ResponseSource.SetResult(new GetChartCommandResponse
        {
            CandleDtos = candles
        });
    }

    private async void StrategyBaseOnPositionRejectedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Rejected;
        await _hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionClosedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Closed;
        await _hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionUpdatedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Updated;
        await _hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionOpenedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Opened;
        await _hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnCandleEvent(object? sender, RobotEvent<Candle> e)
    {
        var candleDto = _mapper.Map<CandleDto>(e.EventField);
        await _hubContext.Clients.All.ReceiveCandle(candleDto);
    }

    private async void StrategyBaseOnTickEvent(object? sender, RobotEvent<Tick> e)
    {
        var tickDto = _mapper.Map<TickDto>(e.EventField);
        await _hubContext.Clients.All.ReceiveTick(tickDto);
    }


    private void GetStrategyInfo(GetStrategyInfoCommand getStrategyInfoCommand, StrategyBase strategy)
    {
        var strategyInfoDto = _mapper.Map<StrategyInfoDto>(strategy);
        getStrategyInfoCommand.ResponseSource.SetResult(new GetStrategyInfoCommandResponse
        {
            StrategyInfoDto = strategyInfoDto
        });
    }

    private async Task CloseStrategy(CloseStrategyCommand closeStrategyCommand, StrategyBase strategy)
    {
        await strategy.DisableStrategy(StrategyReasonDisabled.User);
        strategy.Dispose();
        _strategyList.Remove(closeStrategyCommand.Id);
        _strategyListContext[closeStrategyCommand.Id].Unload();
        _strategyListContext.Remove(closeStrategyCommand.Id);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        closeStrategyCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void GetStrategyPositionClosed(GetStrategyPositionClosedCommand command, StrategyBase strategy)
    {
        var data = new List<PositionDto>();
        data.AddRange(_mapper.Map<List<PositionDto>>(strategy.PositionsClosed.ToList()));
        command.ResponseSource.SetResult(new GetStrategyPositionClosedCommandResponse
        {
            PositionDtos = data
        });
    }

    private void GetStrategyResult(GetStrategyResultRequestCommand strategyResultRequest, StrategyBase strategy)
    {
        var data = _mapper.Map<ResultDto>(strategy.Results);
        strategyResultRequest.ResponseSource.SetResult(new GetStrategyResultCommandResponse
        {
            ResultDto = data
        });
    }

    private void GetOpenedPosition(GetOpenedPositionRequestCommand command, StrategyBase strategy)
    {
        var listPositionsDto = new List<PositionDto>();

        if (strategy.PositionOpened is not null)
        {
            var position = _mapper.Map<PositionDto>(strategy.PositionOpened);
            listPositionsDto.Add(position);
        }

        command.ResponseSource.SetResult(new GetOpenedPositionResponseCommand
        {
            ListPositionsDto = listPositionsDto
        });
    }

    private void SetCanRun(SetCanRunCommand setCanRunCommand, StrategyBase strategy)
    {
        strategy.CanRun = setCanRunCommand.Bool;
        setCanRunCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }


    private (StrategyImplementationBase, CustomLoadContext) GenerateStrategy(StrategyFile strategyFileDto)
    {
        var sourceCode = Encoding.UTF8.GetString(strategyFileDto.Data);
        var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);

        var context = new CustomLoadContext();
        using var stream = new MemoryStream(compiledCode);
        var assembly = context.LoadFromStream(stream);

        var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);

        var type = assembly.GetType(className);
        var instance = Activator.CreateInstance(type);

        return ((StrategyImplementationBase)instance, context);
    }

    #endregion


    #region ApiCommand

    private async Task Connect(ApiConnectCommand command)
    {
        _logger.Information("Init handler to type {Enum}", command.ConnectDto.HandlerEnum);
        _apiHandlerBase = ApiHandlerFactory.GetApiHandler(command.ConnectDto.HandlerEnum.GetValueOrDefault(), _logger);
        var credentials = new Credentials
        {
            User = command.ConnectDto.User,
            Password = command.ConnectDto.Pwd
        };
        await _apiHandlerBase.ConnectAsync(credentials);
        _apiHandlerBase.Connected += ApiHandlerBaseOnConnected;
        _apiHandlerBase.Disconnected += ApiHandlerBaseOnDisconnected;
        _apiHandlerBase.NewBalanceEvent += ApiHandlerBaseOnNewBalanceEvent;

        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void ApiHandlerBaseOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        // TODO : Mettre l'event
    }

    private async void ApiHandlerBaseOnDisconnected(object? sender, EventArgs e)
    {
        try
        {
            foreach (var keyPairValue in _strategyList)
            {
                var strategy = keyPairValue.Value;
                await strategy.DisableStrategy(StrategyReasonDisabled.Api);
                strategy.Dispose();
            }

            _strategyList.Clear();
            foreach (var customLoadContext in _strategyListContext) customLoadContext.Value.Unload();
            _strategyListContext.Clear();
            _apiHandlerBase?.Dispose();
            _apiHandlerBase = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (System.Exception ex)
        {
            _logger.Fatal(ex, "Can't close strategy after api disconnection");
        }
    }

    private async void ApiHandlerBaseOnConnected(object? sender, EventArgs e)
    {
    }

    private async Task Disconnect(DisconnectCommand command)
    {
        await _apiHandlerBase.DisconnectAsync();
        _apiHandlerBase.Connected -= ApiHandlerBaseOnConnected;
        _apiHandlerBase.Disconnected -= ApiHandlerBaseOnDisconnected;
        _apiHandlerBase.NewBalanceEvent -= ApiHandlerBaseOnNewBalanceEvent;
        _apiHandlerBase.Dispose();
        _apiHandlerBase = null;

        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void GetTypeHandler(GetTypeHandlerCommand getTypeHandlerCommand)
    {
        var data = ResolveHandlerApiType(_apiHandlerBase?.GetType().Name);
        _logger.Information("Api handler type is {Data}", data);
        getTypeHandlerCommand.ResponseSource.SetResult(new GetTypeHandlerResultCommand
        {
            Handler = data
        });
    }


    private async Task GetAllSymbol(GetAllSymbolCommand command)
    {
        CheckApiHandlerNotNull();
        var symbols = await _apiHandlerBase.GetAllSymbolsAsync();

        command.ResponseSource.SetResult(new GetAllSymbolCommandResultCommand
        {
            SymbolInfos = symbols
        });
    }

    private void IsConnected(IsConnectedCommand isConnectedCommand)
    {
        if (_apiHandlerBase is null || !(bool)_apiHandlerBase?.IsConnected())
            isConnectedCommand.ResponseSource.SetResult(new IsConnectedResultCommand
            {
                IsConnected = false
            });
        else
            isConnectedCommand.ResponseSource.SetResult(new IsConnectedResultCommand
            {
                IsConnected = true
            });
    }


    private string ResolveHandlerApiType(string name)
    {
        return name switch
        {
            nameof(XtbApiHandler) => ApiHandlerEnum.Xtb.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }

    #endregion
}