using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Exposition;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.PositionHandler;
using RobotAppLibraryV2.Results;
using Serilog;

namespace RobotAppLibraryV2.Factory;

public interface IStrategyServiceFactory
{
    IStrategyResult GetStrategyResultService(IApiHandler apiHandler, string positionRefenrece);
    ILotValueCalculator GetLotValueCalculator(IApiHandler apiHandler, ILogger logger, string symbol);
    IMoneyManagement GetMoneyManagement(IApiHandler apiHandler, ILogger logger, string symbol, string positionReferene);
    IPositionHandler GetPositionHandler(ILogger logger, IApiHandler handler, string symbol, string positionReferene);
    ICandleList GetHistory(ILogger logger, IApiHandler apiHandler, string symbol, Timeframe timeframe);
}

public class StrategyServiceFactory : IStrategyServiceFactory
{
    public IStrategyResult GetStrategyResultService(IApiHandler apiHandler, string positionRefenrece)
    {
        return new StrategyResult(apiHandler, positionRefenrece);
    }

    public ILotValueCalculator GetLotValueCalculator(IApiHandler apiHandler, ILogger logger, string symbol)
    {
        return new LotValueCalculator(apiHandler, logger, symbol);
    }

    public IMoneyManagement GetMoneyManagement(IApiHandler apiHandler, ILogger logger, string symbol,
        string positionReferene)
    {
        var lotValue = GetLotValueCalculator(apiHandler, logger, symbol);
        return new MoneyManagement.MoneyManagement(apiHandler, symbol, logger, lotValue, positionReferene);
    }

    public IPositionHandler GetPositionHandler(ILogger logger, IApiHandler handler, string symbol,
        string positionReferene)
    {
        return new PositionHandler.PositionHandler(logger, handler, symbol, positionReferene);
    }

    public ICandleList GetHistory(ILogger logger, IApiHandler apiHandler, string symbol, Timeframe timeframe)
    {
        return new CandleList.CandleList(apiHandler, logger, timeframe, symbol);
    }
}