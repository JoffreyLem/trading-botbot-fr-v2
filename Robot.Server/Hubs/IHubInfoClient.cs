using Robot.Server.Dto.Response;

namespace Robot.Server.Hubs;

public interface IHubInfoClient
{
    Task ReceiveCandle(CandleDto candle);
    Task ReceiveTick(TickDto tick);

    Task ReceivePosition(PositionDto positionDto);
}