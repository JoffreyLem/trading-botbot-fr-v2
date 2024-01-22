using AutoMapper;
using Robot.DataBase.Modeles;
using Robot.Server.Dto;
using Robot.Server.Dto.Response;
using RobotAppLibraryV2.BackTest;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;

namespace Robot.Server.Mapper;

public class MappingProfilesBackgroundServices : Profile
{
    public MappingProfilesBackgroundServices()
    {
        CreateMap<StrategyBase, StrategyInfoDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(x => x.Timeframe, opt => opt.MapFrom(src => src.Timeframe))
            .ForMember(x => x.Timeframe2, opt => opt.MapFrom(src => src.Timeframe2))
            .ForMember(x => x.StrategyName, opt => opt.MapFrom(src => src.StrategyName))
            .ForMember(x => x.CanRun, opt => opt.MapFrom(src => src.CanRun))
            .ForMember(x => x.StrategyDisabled, opt => opt.MapFrom(src => src.StrategyDisabled))
            .ForMember(x => x.SecureControlPosition, opt => opt.MapFrom(src => src.SecureControlPosition))
            .ForMember(x => x.LastCandle, opt => opt.MapFrom(src => src.CurrentCandle))
            .ForMember(x => x.LastTick, opt => opt.MapFrom(src => src.LastPrice));

        CreateMap<BackTest, BackTestDto>()
            .ForMember(dest => dest.IsBackTestRunning, opt => opt.MapFrom(src => src.BacktestRunning))
            .ForMember(dest => dest.LastBackTestExecution, opt => opt.MapFrom(src => src.LastBacktestExecution))
            .ForMember(dest => dest.ResultBacktest, opt => opt.MapFrom(src => src.Result));

        CreateMap<Result, ResultDto>();
        CreateMap<AccountBalance, AccountBalanceDto>();
        CreateMap<Candle, CandleDto>();
        CreateMap<Tick, TickDto>();


        // Mapper position
        CreateMap<ReasonClosed, string>().ConvertUsing(src => src.ToString());
        CreateMap<StatusPosition, string>().ConvertUsing(src => src.ToString());
        CreateMap<TypeOperation, string>().ConvertUsing(src => src.ToString());

        CreateMap<Timeframe, string>().ConvertUsing(src => src.ToString());
        CreateMap<Position, PositionDto>();

        CreateMap<StrategyFile, StrategyFileDto>().ReverseMap();
    }
}