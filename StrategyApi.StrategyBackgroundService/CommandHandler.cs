using AutoMapper;
using RobotAppLibraryV2.Api.Xtb;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.events;
using RobotAppLibraryV2.Strategy;
using Serilog;
using StrategyApi.Mail;
using StrategyApi.Strategy.Main;
using StrategyApi.Strategy.StrategySar;
using StrategyApi.Strategy.Test;
using StrategyApi.StrategyBackgroundService.Command;
using StrategyApi.StrategyBackgroundService.Command.Api;
using StrategyApi.StrategyBackgroundService.Command.Api.Request;
using StrategyApi.StrategyBackgroundService.Command.Api.Result;
using StrategyApi.StrategyBackgroundService.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Command.Strategy.Request;
using StrategyApi.StrategyBackgroundService.Command.Strategy.Response;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Exception;

namespace StrategyApi.StrategyBackgroundService;

public class CommandHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private IApiHandler? _apiHandlerBase;

    private readonly Dictionary<string, StrategyBase> _strategyList = new();

    public CommandHandler(ILogger logger, IMapper mapper, IEmailService emailService)
    {
        _logger = logger.ForContext<CommandHandler>();
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task HandleApiCommand(ServiceCommandeBaseApiAbstract command)
    {
        _logger.Information("Api command received {Command}", command);
        switch (command)
        {
            case InitHandlerCommand initHandlerCommand:
                InitHandler(initHandlerCommand);
                _logger.Information("Api command processed {@Command}", initHandlerCommand);
                break;
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
                await Disconnect(disconnectCommand);
                _logger.Information("Api command processed {@Command}", disconnectCommand);
                break;
            default:
                _logger.Error("Trying to use unhandled command {@Command}", command);
                throw new CommandException($"Commande {command} non gérer");
        }
    }

    public async Task HandleStrategyCommand(ServiceCommandeBaseStrategyAbstract command)
    {
        _logger.Information("Strategy command received {Command}", command);
        switch (command)
        {
            case InitStrategyCommand initStrategyCommandDto:
                InitStrategy(initStrategyCommandDto);
                _logger.Information("Strategy command processed {@Command}", initStrategyCommandDto);
                break;
            case GetStrategyInfoCommand getStrategyInfoCommand:
                GetStrategyInfo(getStrategyInfoCommand);
                _logger.Information("Strategy command processed {@Command}", getStrategyInfoCommand);
                break;
            case CloseStrategyCommand closeStrategyCommand:
                await CloseStrategy(closeStrategyCommand);
                _logger.Information("Strategy command processed {@Command}", closeStrategyCommand);
                break;
            case GetStrategyPositionClosedCommand getStrategyPositionClosedCommand:
                GetStrategyPositionClosed(getStrategyPositionClosedCommand);
                _logger.Information("Strategy command processed {@Command}", nameof(getStrategyPositionClosedCommand));
                break;
            case GetStrategyResultRequestCommand getStrategyResultRequestCommand:
                GetStrategyResult(getStrategyResultRequestCommand);
                _logger.Information("Strategy command processed {@Command}", getStrategyResultRequestCommand);
                break;
            case GetOpenedPositionRequestCommand getOpenedPositionRequestCommand:
                GetOpenedPosition(getOpenedPositionRequestCommand);
                _logger.Information("Strategy command processed {@Command}", getOpenedPositionRequestCommand);
                break;
            case SetCanRunCommand setCanRunCommand:
                SetCanRun(setCanRunCommand);
                _logger.Information("Strategy command processed {@Command}", setCanRunCommand);
                break;
            case GetChartCommandRequest getChartCommandRequest:
                GetChart(getChartCommandRequest);
                _logger.Information("Strategy command processed {@Command}", getChartCommandRequest);
                break;
            case GetAllStrategyCommandRequest getAllStrategyCommandRequest:
                GetAllStrategy(getAllStrategyCommandRequest);
                _logger.Information("Strategy command processed {@Command}", getAllStrategyCommandRequest);
                break;
            default:
                _logger.Error("Trying to use unanhdled command {@Command}", command);
                throw new CommandException($"Commande {command} non gérer");
        }
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

    public static event EventHandler<BackGroundServiceEvent<TickDto>>? TickEvent;
    public static event EventHandler<BackGroundServiceEvent<CandleDto>>? CandleEvent;


    public static event EventHandler<BackGroundServiceEvent<PositionDto>>? PositionChangeEvent;

    #region StrategyCommand

    private void InitStrategy(InitStrategyCommand initStrategyCommandDto)
    {
        CheckApiHandlerNotNull();
        try
        {
            var strategyImplementation = GenerateStrategy(initStrategyCommandDto.StrategyType);
            var istrategySerrvice = new StrategyServiceFactory();
            var strategyBase = new StrategyBase(strategyImplementation, initStrategyCommandDto.Symbol,
                initStrategyCommandDto.Timeframe, initStrategyCommandDto.timeframe2, _apiHandlerBase, _logger,
                istrategySerrvice);

            strategyBase.TickEvent += StrategyBaseOnTickEvent;
            strategyBase.CandleEvent += StrategyBaseOnCandleEvent;
            strategyBase.PositionOpenedEvent += StrategyBaseOnPositionOpenedEvent;
            strategyBase.PositionUpdatedEvent += StrategyBaseOnPositionUpdatedEvent;
            strategyBase.PositionClosedEvent += StrategyBaseOnPositionClosedEvent;
            strategyBase.PositionRejectedEvent += StrategyBaseOnPositionRejectedEvent;
            strategyBase.StrategyClosed += StrategyBaseOnStrategyClosed;
            strategyBase.TresholdEvent += StrategyBaseOnTresholdEvent;
            strategyBase.StrategyInfoUpdated += StrategyBaseOnStrategyInfoUpdated;

            _strategyList.Add(strategyBase.Id, strategyBase);


            initStrategyCommandDto.ResponseSource.SetResult(new AcknowledgementResponse());
        }
        catch (System.Exception e) when (e is not StrategyException)
        {
            throw;
        }
    }

    private void StrategyBaseOnStrategyInfoUpdated(object? sender, RobotEvent e)
    {
    }

    private void GetChart(GetChartCommandRequest chartCommandRequest)
    {
        var strategy = GetStrategyById(chartCommandRequest.Id);
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


    private async void StrategyBaseOnTresholdEvent(object? sender, RobotEvent<EventTreshold> e)
    {
        var message = $"Strategy closed cause of treshold : {e}";
        await _emailService.SendEmail("Strategy closed", message);
    }

    private async void StrategyBaseOnStrategyClosed(object? sender, RobotEvent<StrategyReasonClosed> e)
    {
        if (e is not { EventField: StrategyReasonClosed.User })
        {
            _logger.Warning("Strategy closed : {Reason}, send email to user", e.EventField.ToString());
            var message = $"Strategy closed cause : {e.EventField.ToString()}";
            await _emailService.SendEmail("Strategy closed", message);
        }

        _strategyList.Remove(e.Id);
    }

    private void StrategyBaseOnPositionRejectedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Rejected;
        PositionChangeEvent?.Invoke(this, new BackGroundServiceEvent<PositionDto>(posDto, e.Id));
    }

    private void StrategyBaseOnPositionClosedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Closed;
        PositionChangeEvent?.Invoke(this, new BackGroundServiceEvent<PositionDto>(posDto, e.Id));
    }

    private void StrategyBaseOnPositionUpdatedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Updated;
        PositionChangeEvent?.Invoke(this, new BackGroundServiceEvent<PositionDto>(posDto, e.Id));
    }

    private async void StrategyBaseOnPositionOpenedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Opened;
        PositionChangeEvent?.Invoke(this, new BackGroundServiceEvent<PositionDto>(posDto, e.Id));
    }

    private void StrategyBaseOnCandleEvent(object? sender, RobotEvent<Candle> e)
    {
        var candleDto = _mapper.Map<CandleDto>(e.EventField);
        //    await _strategyHub.Clients.All.SendCandle(candleDto);
        CandleEvent?.Invoke(this, new BackGroundServiceEvent<CandleDto>(candleDto, e.Id));
    }

    private void StrategyBaseOnTickEvent(object? sender, RobotEvent<Tick> e)
    {
        var tickDto = _mapper.Map<TickDto>(e.EventField);

        TickEvent?.Invoke(this, new BackGroundServiceEvent<TickDto>(tickDto, e.Id));
    }


    private void GetStrategyInfo(GetStrategyInfoCommand getStrategyInfoCommand)
    {
        var strategy = GetStrategyById(getStrategyInfoCommand.Id);
        var strategyInfoDto = _mapper.Map<StrategyInfoDto>(strategy);
        getStrategyInfoCommand.ResponseSource.SetResult(new GetStrategyInfoCommandResponse
        {
            StrategyInfoDto = strategyInfoDto
        });
    }

    private async Task CloseStrategy(CloseStrategyCommand closeStrategyCommand)
    {
        var strategy = GetStrategyById(closeStrategyCommand.Id);
        await strategy.CloseStrategy(StrategyReasonClosed.User);
        _strategyList.Remove(closeStrategyCommand.Id);


        closeStrategyCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void GetStrategyPositionClosed(GetStrategyPositionClosedCommand command)
    {
        var strategy = GetStrategyById(command.Id);
        var data = new ListPositionsDto
        {
            Positions = _mapper.Map<List<PositionDto>>(strategy.PositionsClosed.ToList())
        };
        command.ResponseSource.SetResult(new GetStrategyPositionClosedCommandResponse
        {
            PositionDtos = data
        });
    }

    private void GetStrategyResult(GetStrategyResultRequestCommand strategyResultRequest)
    {
        var strategy = GetStrategyById(strategyResultRequest.Id);
        var data = _mapper.Map<ResultDto>(strategy.Results);
        strategyResultRequest.ResponseSource.SetResult(new GetStrategyResultCommandResponse
        {
            ResultDto = data
        });
    }

    private void GetOpenedPosition(GetOpenedPositionRequestCommand command)
    {
        var strategy = GetStrategyById(command.Id);
        var listPositionsDto = new ListPositionsDto();

        if (strategy.PositionOpened is not null)
        {
            var position = _mapper.Map<PositionDto>(strategy.PositionOpened);
            listPositionsDto.Positions.Add(position);
        }

        command.ResponseSource.SetResult(new GetOpenedPositionResponseCommand
        {
            ListPositionsDto = listPositionsDto
        });
    }

    private void SetCanRun(SetCanRunCommand setCanRunCommand)
    {
        var stategy = GetStrategyById(setCanRunCommand.Id);
        stategy.CanRun = setCanRunCommand.Bool;
        setCanRunCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }


    private StrategyImplementationBase GenerateStrategy(StrategyTypeEnum? type)
    {
        return type switch
        {
            StrategyTypeEnum.Test => new TestStrategy(),
            StrategyTypeEnum.Main => new MainStrategy(),
            StrategyTypeEnum.Sar => new StrategySar(),
            _ => throw new CommandException($"Strategy {type} non gérer")
        };
    }

    #endregion


    #region ApiCommand

    private void InitHandler(InitHandlerCommand command)
    {
        if (_apiHandlerBase is not null && _apiHandlerBase.IsConnected())
            throw new CommandException("Api handler already connected, disconnect first");

        _logger.Information("Init handler to type {Enum}", command.ApiHandlerEnum);
        _apiHandlerBase = ApiHandlerFactory.GetApiHandler(command.ApiHandlerEnum, XtbServer.DEMO, _logger);

        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    // TODO : a faire evol lors du multi API
    private async Task Connect(ApiConnectCommand command)
    {
        CheckApiHandlerNotNull();

        await _apiHandlerBase.ConnectAsync(command.Credentials);
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
    }

    private async void ApiHandlerBaseOnConnected(object? sender, EventArgs e)
    {
    }

    private async Task Disconnect(DisconnectCommand command)
    {
        CheckApiHandlerNotNull();
        await _apiHandlerBase.DisconnectAsync();
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
        if (_apiHandlerBase is null)
            isConnectedCommand.ResponseSource.SetResult(new IsConnectedResultCommand
            {
                ConnexionStateEnum = ConnexionStateEnum.NotInitialized
            });
        else if ((bool)_apiHandlerBase?.IsConnected())
            isConnectedCommand.ResponseSource.SetResult(new IsConnectedResultCommand
            {
                ConnexionStateEnum = ConnexionStateEnum.Connected
            });
        else
            isConnectedCommand.ResponseSource.SetResult(new IsConnectedResultCommand
            {
                ConnexionStateEnum = ConnexionStateEnum.Disconnected
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