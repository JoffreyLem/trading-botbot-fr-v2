using RobotAppLibraryV2.ApiHandler.Exception;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.BackTest;

public class BacktestApiHandler : ApiHandlerBase
{
    public BacktestApiHandler(BackTestApiExecutor commandExecutor, ILogger logger) : base(commandExecutor, logger)
    {
    }

    public async Task StartBacktest()
    {
        await (CommandExecutor as BackTestApiExecutor)?.StartBackTest()!;
    }

    public override async Task<Position> OpenPositionAsync(Position position, decimal price)
    {
        try
        {
            var pos = await CommandExecutor.ExecuteOpenTradeCommand(position, price);
            position.Order = pos.Order;
            CachePosition.Add(pos);
            OnPositionOpenedEvent(pos);
            return pos;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(OpenPositionAsync)}");
            throw new ApiHandlerException($"Error on  {nameof(OpenPositionAsync)}");
        }
    }

    public override async Task UpdatePositionAsync(decimal price, Position position)
    {
        try
        {
            await CommandExecutor.ExecuteUpdateTradeCommand(position, price);
            OnPositionUpdatedEvent(position);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(UpdatePositionAsync)}");
            throw new ApiHandlerException($"Error on  {nameof(UpdatePositionAsync)}");
        }
    }

    public override async Task ClosePositionAsync(decimal price, Position position)
    {
        try
        {
            var pos = await CommandExecutor.ExecuteCloseTradeCommand(position, price);
            OnPositionClosedEvent(pos);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(ClosePositionAsync)}");
            throw new ApiHandlerException($"Error on  {nameof(ClosePositionAsync)}");
        }
    }
}