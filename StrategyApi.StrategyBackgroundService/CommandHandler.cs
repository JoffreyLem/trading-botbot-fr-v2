using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.ApiHandler.Xtb;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;
using RobotAppLibraryV2.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;
using RobotAppLibraryV2.Strategy;
using Serilog;
using StrategyApi.Mail;
using StrategyApi.Strategy.Main;
using StrategyApi.Strategy.StrategySar;
using StrategyApi.Strategy.Test;
using StrategyApi.StrategyBackgroundService.Dto.Command.Api;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Exception;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Services;

namespace StrategyApi.StrategyBackgroundService;

public class CommandHandler
{
    private const string DefaultEmail = "lemery.joffrey@outlook.fr";
    private readonly IHubContext<ApiHandlerHub, IApiHandlerHub> _apiHandlerHub;
    private readonly IEmailService _emailService;

    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IHubContext<StrategyHub, IStrategyHub> _strategyHub;
    private readonly IEventBus _eventBus;

    private IApiHandler? _apiHandlerBase;
    private StrategyBase? _strategyBase;

    public CommandHandler(ILogger logger, IHubContext<ApiHandlerHub, IApiHandlerHub> apiHandlerHub, IMapper mapper,
        IHubContext<StrategyHub, IStrategyHub> strategyHub, IEmailService emailService, IEventBus eventBus)
    {
        _logger = logger.ForContext<CommandHandler>();
        _apiHandlerHub = apiHandlerHub;
        _mapper = mapper;
        _strategyHub = strategyHub;
        _emailService = emailService;
        _eventBus = eventBus;
    }

    public async Task HandleApiCommand(ApiCommandBaseDto command, TaskCompletionSource<CommandResultBase> tcs)
    {
        _logger.Information("Api command received {@Command}", command);
        switch (command.ApiCommandEnum)
        {
            case ApiCommand.InitHandler:
                InitHandler(command, tcs);
                break;
            case ApiCommand.GetTypeHandler:
                GetTypeHandler(tcs);
                break;
            case ApiCommand.GetAllSymbols:
                await GetAllSymbol(tcs);
                break;
            case ApiCommand.IsConnected:
                IsConnected(tcs);
                break;
            case ApiCommand.Connect:
                await Connect(command, tcs);
                break;
            case ApiCommand.Disconnect:
                await Disconnect(tcs);
                break;
            default:
                throw new CommandException($"Commande {command.ApiCommandEnum} non gérer");
        }
    }

    public Task HandleStrategyCommand(StrategyCommandBaseDto command, TaskCompletionSource<CommandResultBase> tcs)
    {
        _logger.Information("Strategy command received {@Command}", command);
        switch (command.StrategyCommand)
        {
            case StrategyCommand.InitStrategy:
                InitStrategy(command, tcs);
                break;
            case StrategyCommand.IsInitialized:
                IsInitialized(tcs);
                break;
            case StrategyCommand.GetStrategyInfo:
                GetStrategyInfo(tcs);
                break;
            case StrategyCommand.CloseStrategy:
                CloseStrategy(tcs);
                break;
            case StrategyCommand.GetStrategyPosition:
                GetStrategyPosition(tcs);
                break;
            case StrategyCommand.GetResults:
                GetStrategyResult(tcs);
                break;
            case StrategyCommand.GetOpenedPosition:
                GetOpenedPosition(tcs);
                break;
            case StrategyCommand.SetCanRun:
                SetCanRun(tcs, command);
                break;
            case StrategyCommand.SetSecureControlPosition:
                SetSecureControlPosition(tcs, command);
                break;
            case StrategyCommand.GetChart:
                GetChart(tcs);
                break;
            default:
                throw new CommandException($"Commande {command.StrategyCommand} non gérer");
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

    #region StrategyCommand

    private void InitStrategy(StrategyCommandBaseDto command, TaskCompletionSource<CommandResultBase> tcs)
    {
        if (_strategyBase is not null) throw new CommandException("The strategy is not null, close it");

        if (command is InitStrategyCommandDto initStrategyCommandDto)
        {
            CheckApiHandlerNotNull();
            try
            {
                var strategyImplementation = GenerateStrategy(initStrategyCommandDto.StrategyType);
                _strategyBase = new StrategyBase(strategyImplementation, initStrategyCommandDto.Symbol,
                    initStrategyCommandDto.Timeframe, initStrategyCommandDto.timeframe2, _apiHandlerBase, _logger);
                _strategyBase.TickEvent += StrategyBaseOnTickEvent;
                _strategyBase.CandleEvent += StrategyBaseOnCandleEvent;
                _strategyBase.PositionOpenedEvent += StrategyBaseOnPositionOpenedEvent;
                _strategyBase.PositionUpdatedEvent += StrategyBaseOnPositionUpdatedEvent;
                _strategyBase.PositionClosedEvent += StrategyBaseOnPositionClosedEvent;
                _strategyBase.PositionRejectedEvent += StrategyBaseOnPositionRejectedEvent;
                _strategyBase.StrategyClosed += StrategyBaseOnStrategyClosed;
                _strategyBase.TresholdEvent += StrategyBaseOnTresholdEvent;
                _eventBus.PublishAsync(ReferentEnum.Strategy,ConnexionStateEnum.Initialized);
            }
            catch (System.Exception e)
            {
                throw new CommandExceptionInternalError("Can't initialize strategy");
            }

            tcs.SetResult(new CommandExecutedResult());
        }
        else
        {
            throw new CommandException($"Bad command for {nameof(InitStrategy)}");
        }
    }

    private  void GetChart(TaskCompletionSource<CommandResultBase> tcs)
    {
        CheckStrategyNotNull();
        List<CandleDto> candles = _strategyBase.History.Select( x=>new CandleDto()
        {
            Open =(double) x.Open,
            High =(double) x.High,
            Low =(double) x.Low,
            Close =(double) x.Close,
            Date = x.Date,
            Volume =(double) x.Volume
        }).ToList();
        tcs.SetResult(new CommandExecutedTypedResult<List<CandleDto>>(candles));
    }



    private async void StrategyBaseOnTresholdEvent(object? sender, MoneyManagementTresholdType e)
    {
        var message = $"Strategy closed cause of treshold : {e.ToString()}";
        await _emailService.SendEmail(DefaultEmail, "Strategy closed", message);
        // TODO : reset threshHold
        await _strategyHub.Clients.All.SendEvent(EventType.Treshold, e.ToString());
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
        await _eventBus.PublishAsync(ReferentEnum.Strategy,ConnexionStateEnum.NotInitialized);
        await _strategyHub.Clients.All.SendEvent(EventType.Close, "Strategy closing");
    }

    private async void StrategyBaseOnPositionRejectedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        await _strategyHub.Clients.All.SendPositionState(posDto, PositionStateEnum.Rejected);
        await _eventBus.PublishAsync(posDto,PositionStateEnum.Rejected);
    }

    private async void StrategyBaseOnPositionClosedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        await _strategyHub.Clients.All.SendPositionState(posDto, PositionStateEnum.Closed);
        await _eventBus.PublishAsync(posDto,PositionStateEnum.Closed);
    }

    private async void StrategyBaseOnPositionUpdatedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        await _strategyHub.Clients.All.SendPositionState(posDto, PositionStateEnum.Updated);
        await _eventBus.PublishAsync(posDto,PositionStateEnum.Updated);
    }

    private async void StrategyBaseOnPositionOpenedEvent(object? sender, Position e)
    {
        var posDto = _mapper.Map<PositionDto>(e);
        await _strategyHub.Clients.All.SendPositionState(posDto, PositionStateEnum.Opened);
        await _eventBus.PublishAsync(posDto,PositionStateEnum.Opened);
    }

    private async void StrategyBaseOnCandleEvent(object? sender, Candle e)
    {
        var candleDto = _mapper.Map<CandleDto>(e);
        await _strategyHub.Clients.All.SendCandle(candleDto);
        await _eventBus.PublishAsync(candleDto);
    }

    private async void StrategyBaseOnTickEvent(object? sender, Tick e)
    {
        var tickDto = _mapper.Map<TickDto>(e);
        await _strategyHub.Clients.All.SendTick(tickDto);
        await _eventBus.PublishAsync(tickDto);
    }

    private void IsInitialized(TaskCompletionSource<CommandResultBase> tcs)
    {
        var result = new IsInitializedDto();
        if (_strategyBase is not null)
            result.Initialized = true;
        else
            result.Initialized = false;
        tcs.SetResult(new CommandExecutedTypedResult<IsInitializedDto>(result));
    }

    private void GetStrategyInfo(TaskCompletionSource<CommandResultBase> tcs)
    {
        CheckStrategyNotNull();
        var strategyInfoDto = _mapper.Map<StrategyInfoDto>(_strategyBase);
        tcs.SetResult(new CommandExecutedTypedResult<StrategyInfoDto>(strategyInfoDto));
    }

    private async void CloseStrategy(TaskCompletionSource<CommandResultBase> tcs)
    {
        if (_strategyBase is not null)
        {
            _strategyBase.CloseStrategy(StrategyReasonClosed.User);
            _strategyBase = null;
            await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.NotInitialized);
        }

        tcs.SetResult(new CommandExecutedResult());
    }

    private void GetStrategyPosition(TaskCompletionSource<CommandResultBase> tcs)
    {
        CheckStrategyNotNull();
        var data = new ListPositionsDto
        {
            Positions = _mapper.Map<List<PositionDto>>(_strategyBase.Positions.ToList())
        };
        tcs.SetResult(new CommandExecutedTypedResult<ListPositionsDto>(data));
    }

    private void GetStrategyResult(TaskCompletionSource<CommandResultBase> tcs)
    {
        CheckStrategyNotNull();
        var data = _mapper.Map<ResultDto>(_strategyBase.Results);
        tcs.SetResult(new CommandExecutedTypedResult<ResultDto>(data));
    }

    private void GetOpenedPosition(TaskCompletionSource<CommandResultBase> tcs)
    {
        CheckStrategyNotNull();
        var listPositionsDto = new ListPositionsDto();

        if (_strategyBase.PositionOpened is not null)
        {
            var position = _mapper.Map<PositionDto>(_strategyBase.PositionOpened);
            listPositionsDto.Positions.Add(position);
        }

        tcs.SetResult(new CommandExecutedTypedResult<ListPositionsDto>(listPositionsDto));
    }

    private void SetCanRun(TaskCompletionSource<CommandResultBase> tcs, StrategyCommandBaseDto strategyCommandBaseDto)
    {
        if (strategyCommandBaseDto is StrategyBoolCommand strategyBoolCommand)
        {
            CheckStrategyNotNull();
            _strategyBase.CanRun = strategyBoolCommand.Bool;
            tcs.SetResult(new CommandExecutedResult());
        }
        else
        {
            throw new CommandException($"Bad command for {nameof(SetCanRun)}");
        }
    }

    private void SetSecureControlPosition(TaskCompletionSource<CommandResultBase> tcs,
        StrategyCommandBaseDto strategyCommandBaseDto)
    {
        if (strategyCommandBaseDto is StrategyBoolCommand strategyBoolCommand)
        {
            CheckStrategyNotNull();
            _strategyBase.SecureControlPosition = strategyBoolCommand.Bool;
            tcs.SetResult(new CommandExecutedResult());
        }
        else
        {
            throw new CommandException($"Bad command for {nameof(SetSecureControlPosition)}");
        }
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

    private async void InitHandler(ApiCommandBaseDto command, TaskCompletionSource<CommandResultBase> taskCompletionSource)
    {
        if (command is InitHandlerCommandDto initHandlerCommandDto)
        {
            if (_apiHandlerBase is not null && _apiHandlerBase.IsConnected())
                throw new CommandException("Api handler already connected, disconnect first");

            _logger.Information("Init handler to type {Enum}", initHandlerCommandDto.ApiHandlerEnum);
            _apiHandlerBase = GetApiByType(initHandlerCommandDto.ApiHandlerEnum.GetValueOrDefault());
            await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.Initialized);
            taskCompletionSource.SetResult(new CommandExecutedResult());
        }
        else
        {
            throw new CommandException($"Bad command for {nameof(InitHandler)}");
        }
    }

    // TODO : a faire evol lors du multi API
    private async Task Connect(ApiCommandBaseDto command, TaskCompletionSource<CommandResultBase> taskCompletionSource)
    {
        CheckApiHandlerNotNull();
        if (command is ApiConnectCommandDto apiConnectCommand)
        {
            await _apiHandlerBase.ConnectAsync(apiConnectCommand.User, apiConnectCommand.Password);
            _apiHandlerBase.Connected += ApiHandlerBaseOnConnected;
            _apiHandlerBase.Disconnected += ApiHandlerBaseOnDisconnected;
            _apiHandlerBase.NewBalanceEvent += ApiHandlerBaseOnNewBalanceEvent;
            await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.Connected);
            taskCompletionSource.SetResult(new CommandExecutedResult());
        }
        else
        {
            throw new CommandException($"Bad command for {nameof(Connect)}");
        }
    }

    private void ApiHandlerBaseOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        _apiHandlerHub.Clients.All.SendBalanceState(_mapper.Map<AccountBalanceDto>(e));
    }

    private async void ApiHandlerBaseOnDisconnected(object? sender, EventArgs e)
    {
        await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.Disconnected);
    }

    private async void ApiHandlerBaseOnConnected(object? sender, EventArgs e)
    {
        await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.Connected);
    }

    private async Task Disconnect(TaskCompletionSource<CommandResultBase> taskCompletionSource)
    {
        CheckApiHandlerNotNull();
        await _apiHandlerBase.DisconnectAsync();
        _apiHandlerBase = null;
        await _eventBus.PublishAsync(ReferentEnum.Api,ConnexionStateEnum.Disconnected);
        taskCompletionSource.SetResult(new CommandExecutedResult());
    }

    private void GetTypeHandler(TaskCompletionSource<CommandResultBase> tcs)
    {
        var data = ResolveHandlerApiType(_apiHandlerBase?.GetType().Name);
        _logger.Information("Api handler type is {Data}", data);
        tcs.SetResult(new CommandExecutedTypedResult<string>(data));
    }

    private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    // TODO : Changer pour tout retourner
    private async Task GetAllSymbol(TaskCompletionSource<CommandResultBase> tcs)
    {
      
        CheckApiHandlerNotNull();
        var symbols = await _apiHandlerBase?.GetAllSymbolsAsync();

        tcs.SetResult(new CommandExecutedTypedResult<List<string>>(symbols));
 
    }

    private void IsConnected(TaskCompletionSource<CommandResultBase> tcs)
    {
        if (_apiHandlerBase is null)
            tcs.SetResult(new CommandExecutedTypedResult<ConnexionStateEnum>(ConnexionStateEnum.NotInitialized));
        else if ((bool)_apiHandlerBase?.IsConnected())
            tcs.SetResult(new CommandExecutedTypedResult<ConnexionStateEnum>(ConnexionStateEnum.Connected));
        else
            tcs.SetResult(new CommandExecutedTypedResult<ConnexionStateEnum>(ConnexionStateEnum.Disconnected));
    }

    private IApiHandler GetApiByType(ApiHandlerEnum api)
    {
        return api switch
        {
            ApiHandlerEnum.Xtb => new XtbApi(new APICommandFactory(),
                new SyncAPIConnector(Servers.DEMO)
                , _logger),
            _ => throw new ArgumentException($"{api.ToString()} not handled")
        };
    }


    private string ResolveHandlerApiType(string name)
    {
        return name switch
        {
            nameof(XtbApi) => ApiHandlerEnum.Xtb.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }

    #endregion

    public event EventHandler<StrategyReasonClosed>? StrategyClosed;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Candle>? CandleEvent;
    public event EventHandler<MoneyManagementTresholdType>? TresholdEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;
}