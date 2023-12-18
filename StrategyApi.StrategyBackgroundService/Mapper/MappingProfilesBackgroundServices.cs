using AutoMapper;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Strategy;
using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Mapper;

public class MappingProfilesBackgroundServices : Profile
{
    public MappingProfilesBackgroundServices()
    {
        CreateMap<StrategyBase, StrategyInfoDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.StrategyType, opt => opt.MapFrom(src => src.StrategyName))
            .ForMember(x => x.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(x => x.Timeframe, opt => opt.MapFrom(src => src.Timeframe))
            .ForMember(x => x.Timeframe2, opt => opt.MapFrom(src => src.Timeframe2))
            .ForMember(x => x.StrategyName, opt => opt.MapFrom(src => src.StrategyName))
            .ForMember(x => x.CanRun, opt => opt.MapFrom(src => src.CanRun))
            .ForMember(x => x.SecureControlPosition, opt => opt.MapFrom(src => src.SecureControlPosition))
            .ForMember(x => x.LastCandle, opt => opt.MapFrom(src => src.CurrentCandle))
            .ForMember(x => x.LastTick, opt => opt.MapFrom(src => src.LastPrice));
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
    }
}