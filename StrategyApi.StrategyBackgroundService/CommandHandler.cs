using AutoMapper;
using RobotAppLibraryV2.Api.Xtb;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
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
    private const string DefaultEmail = "lemery.joffrey@outlook.fr";

    private readonly IEmailService _emailService;


    private readonly ILogger _logger;
    private readonly IMapper _mapper;


    private IApiHandler? _apiHandlerBase;
    private StrategyBase? _strategyBase;

    public CommandHandler(ILogger logger, IMapper mapper, IEmailService emailService)
    {
        _logger = logger.ForContext<CommandHandler>();

        _mapper = mapper;

        _emailService = emailService;
    }

    public async Task HandleApiCommand(ServiceCommandeBaseApiAbstract command)
    {
        _logger.Verbose("Api command received {@Command}", command);
        switch (command)
        {
            case InitHandlerCommand initHandlerCommand:
                InitHandler(initHandlerCommand);
                break;
            case GetTypeHandlerCommand getTypeHandlerCommand:
                GetTypeHandler(getTypeHandlerCommand);
                break;
            case GetAllSymbolCommand getAllSymbolCommand:
                await GetAllSymbol(getAllSymbolCommand);
                break;
            case IsConnectedCommand connectedCommand:
                IsConnected(connectedCommand);
                break;
            case ApiConnectCommand apiConnectCommand:
                await Connect(apiConnectCommand);
                break;
            case DisconnectCommand disconnectCommand:
                await Disconnect(disconnectCommand);
                break;
            default:
                throw new CommandException($"Commande {command} non gérer");
        }
    }

    public Task HandleStrategyCommand(ServiceCommandeBaseStrategyAbstract command)
    {
        _logger.Verbose("Strategy command received {@Command}", command);
        switch (command)
        {
            case InitStrategyCommand initStrategyCommandDto:
                InitStrategy(initStrategyCommandDto);
                break;
            case IsInitializerCommand isInitializerCommand:
                IsInitialized(isInitializerCommand);
                break;
            case GetStrategyInfoCommand getStrategyInfoCommand:
                GetStrategyInfo(getStrategyInfoCommand);
                break;
            case CloseStrategyCommand closeStrategyCommand:
                CloseStrategy(closeStrategyCommand);
                break;
            case GetStrategyPositionClosedCommand getStrategyPositionClosedCommand:
                GetStrategyPositionClosed(getStrategyPositionClosedCommand);
                break;
            case GetStrategyResultRequestCommand getStrategyResultRequestCommand:
                GetStrategyResult(getStrategyResultRequestCommand);
                break;
            case GetOpenedPositionRequestCommand getOpenedPositionRequestCommand:
                GetOpenedPosition(getOpenedPositionRequestCommand);
                break;
            case SetCanRunCommand setCanRunCommand:
                SetCanRun(setCanRunCommand);
                break;
            case GetChartCommandRequest getChartCommandRequest:
                GetChart(getChartCommandRequest);
                break;
            default:
                throw new CommandException($"Commande {command} non gérer");
        }

        return Task.CompletedTask;
    }


    private void CheckApiHandlerNotNull()
    {
        if (_apiHandlerBase is null) throw new CommandException("The Api handler is not connected");
    }

    private void CheckStrategyNotNull()
    {
        if (_strategyBase is null) throw new CommandException("The strategy is not initialized");
    }

    public async Task Shutdown()
    {
        if (_apiHandlerBase is not null) await _apiHandlerBase.DisconnectAsync();
    }


    public static event EventHandler<ConnexionStateEventArgs>? ConnexionState;
    public static event EventHandler<TickDto>? TickEvent;
    public static event EventHandler<CandleDto>? CandleEvent;
    public static event EventHandler<StrategyEventEvent>? StategyEvent;

    public static event EventHandler<PositionDto>? PositionChangeEvent;

    #region StrategyCommand

    private void InitStrategy(InitStrategyCommand initStrategyCommandDto)
    {
        CheckApiHandlerNotNull();
        try
        {
            var strategyImplementation = GenerateStrategy(initStrategyCommandDto.StrategyType);
            var istrategySerrvice = new StrategyServiceFactory();
            _strategyBase = new StrategyBase(strategyImplementation, initStrategyCommandDto.Symbol,
                initStrategyCommandDto.Timeframe, initStrategyCommandDto.timeframe2, _apiHandlerBase, _logger,
                istrategySerrvice);
            _strategyBase.TickEvent += StrategyBaseOnTickEvent;
            _strategyBase.CandleEvent += StrategyBaseOnCandleEvent;
            _strategyBase.PositionOpenedEvent += StrategyBaseOnPositionOpenedEvent;
            _strategyBase.PositionUpdatedEvent += StrategyBaseOnPositionUpdatedEvent;
            _strategyBase.PositionClosedEvent += StrategyBaseOnPositionClosedEvent;
            _strategyBase.PositionRejectedEvent += StrategyBaseOnPositionRejectedEvent;
            _strategyBase.StrategyClosed += StrategyBaseOnStrategyClosed;
            _strategyBase.TresholdEvent += StrategyBaseOnTresholdEvent;
            _strategyBase.StrategyInfoUpdated += StrategyBaseOnStrategyInfoUpdated;

            var connexionStateEventArgs = new ConnexionStateEventArgs
            {
                Referent = ReferentEnum.Strategy,
                ConnexionState = ConnexionStateEnum.Initialized
            };
            ConnexionState?.Invoke(this, connexionStateEventArgs);
        }
        catch (System.Exception e) when (e is not StrategyException)
        {
            throw;
        }

        initStrategyCommandDto.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void StrategyBaseOnStrategyInfoUpdated(object? sender, EventArgs e)
    {
        var strategyEvent = new StrategyEventEvent
        {
            EventType = EventType.Update,
            Message = ""
        };
        StategyEvent?.Invoke(this, strategyEvent);
    }

    private void GetChart(GetChartCommandRequest chartCommandRequest)
    {
        CheckStrategyNotNull();
        var candles = _strategyBase.History.Select(x => new CandleDto
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


    private async void StrategyBaseOnTresholdEvent(object? sender, EventTreshold e)
    {
        var message = $"Strategy closed cause of treshold : {e.ToString()}";
        await _emailService.SendEmail(DefaultEmail, "Strategy closed", message);
        // TODO : reset threshHold
        var strategyEvent = new StrategyEventEvent
        {
            EventType = EventType.Treshold,
            Message = e.ToString()
        };
        StategyEvent?.Invoke(this, strategyEvent);
    }

    private async void StrategyBaseOnStrategyClosed(object? sender, StrategyReasonClosed e)
    {
        if (e is not StrategyReasonClosed.User)
        {
            _logger.Warning("Strategy closed : {Reason}, send email to user", e.ToString());
            var message = $"Strategy closed cause : {e.ToString()}";
            await _emailService.SendEmail(DefaultEmail, "Strategy closed", message);
            _strategyBase = null;
        }

        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Strategy,
            ConnexionState = ConnexionStateEnum.NotInitialized
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
        var strategyEvent = new StrategyEventEvent
        {
            EventType = EventType.Close,
            Message = "Strategy closing"
        };
        StategyEvent?.Invoke(this, strategyEvent);
    }

    private void StrategyBaseOnPositionRejectedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        posDto.PositionState = PositionStateEnum.Rejected;
        PositionChangeEvent?.Invoke(this, posDto);
    }

    private void StrategyBaseOnPositionClosedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        posDto.PositionState = PositionStateEnum.Closed;
        PositionChangeEvent?.Invoke(this, posDto);
    }

    private void StrategyBaseOnPositionUpdatedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        posDto.PositionState = PositionStateEnum.Updated;
        PositionChangeEvent?.Invoke(this, posDto);
    }

    private async void StrategyBaseOnPositionOpenedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        posDto.PositionState = PositionStateEnum.Opened;
        PositionChangeEvent?.Invoke(this, posDto);
    }

    private void StrategyBaseOnCandleEvent(object? sender, Candle e)
    {
        var candleDto = _mapper.Map<CandleDto>(e);
        //    await _strategyHub.Clients.All.SendCandle(candleDto);
        CandleEvent?.Invoke(this, candleDto);
    }

    private void StrategyBaseOnTickEvent(object? sender, Tick e)
    {
        var tickDto = _mapper.Map<TickDto>(e);

        TickEvent?.Invoke(this, tickDto);
    }

    private void IsInitialized(IsInitializerCommand isInitializerCommand)
    {
        var result = new IsInitializerCommandResponse();
        if (_strategyBase is not null)
            result.IsInitialized = true;
        else
            result.IsInitialized = false;
        isInitializerCommand.ResponseSource.SetResult(result);
    }

    private void GetStrategyInfo(GetStrategyInfoCommand getStrategyInfoCommand)
    {
        CheckStrategyNotNull();
        var strategyInfoDto = _mapper.Map<StrategyInfoDto>(_strategyBase);
        getStrategyInfoCommand.ResponseSource.SetResult(new GetStrategyInfoCommandResponse
        {
            StrategyInfoDto = strategyInfoDto
        });
    }

    private void CloseStrategy(CloseStrategyCommand closeStrategyCommand)
    {
        if (_strategyBase is not null)
        {
            _strategyBase.CloseStrategy(StrategyReasonClosed.User);
            _strategyBase = null;
            var connexionStateEventArgs = new ConnexionStateEventArgs
            {
                Referent = ReferentEnum.Api,
                ConnexionState = ConnexionStateEnum.NotInitialized
            };
            ConnexionState?.Invoke(this, connexionStateEventArgs);
        }

        closeStrategyCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void GetStrategyPositionClosed(GetStrategyPositionClosedCommand command)
    {
        CheckStrategyNotNull();
        var data = new ListPositionsDto
        {
            Positions = _mapper.Map<List<PositionDto>>(_strategyBase.PositionsClosed.ToList())
        };
        command.ResponseSource.SetResult(new GetStrategyPositionClosedCommandResponse
        {
            PositionDtos = data
        });
    }

    private void GetStrategyResult(GetStrategyResultRequestCommand strategyResultRequest)
    {
        CheckStrategyNotNull();
        var data = _mapper.Map<ResultDto>(_strategyBase.Results);
        strategyResultRequest.ResponseSource.SetResult(new GetStrategyResultCommandResponse
        {
            ResultDto = data
        });
    }

    private void GetOpenedPosition(GetOpenedPositionRequestCommand command)
    {
        CheckStrategyNotNull();
        var listPositionsDto = new ListPositionsDto();

        if (_strategyBase.PositionOpened is not null)
        {
            var position = _mapper.Map<PositionDto>(_strategyBase.PositionOpened);
            listPositionsDto.Positions.Add(position);
        }

        command.ResponseSource.SetResult(new GetOpenedPositionResponseCommand
        {
            ListPositionsDto = listPositionsDto
        });
    }

    private void SetCanRun(SetCanRunCommand setCanRunCommand)
    {
        CheckStrategyNotNull();
        _strategyBase.CanRun = setCanRunCommand.Bool;
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
        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Api,
            ConnexionState = ConnexionStateEnum.Initialized
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
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
        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Api,
            ConnexionState = ConnexionStateEnum.Connected
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void ApiHandlerBaseOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        // TODO : Mettre l'event
    }

    private async void ApiHandlerBaseOnDisconnected(object? sender, EventArgs e)
    {
        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Api,
            ConnexionState = ConnexionStateEnum.Disconnected
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
    }

    private async void ApiHandlerBaseOnConnected(object? sender, EventArgs e)
    {
        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Api,
            ConnexionState = ConnexionStateEnum.Connected
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
    }

    private async Task Disconnect(DisconnectCommand command)
    {
        CheckApiHandlerNotNull();
        await _apiHandlerBase.DisconnectAsync();
        _apiHandlerBase = null;
        var connexionStateEventArgs = new ConnexionStateEventArgs
        {
            Referent = ReferentEnum.Api,
            ConnexionState = ConnexionStateEnum.Disconnected
        };
        ConnexionState?.Invoke(this, connexionStateEventArgs);
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
        var symbols = await _apiHandlerBase?.GetAllSymbolsAsync();

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