using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

public interface StreamingListener
{
    void ReceiveTradeRecord(StreamingTradeRecord tradeRecord);
    void ReceiveTickRecord(StreamingTickRecord tickRecord);
    void ReceiveBalanceRecord(StreamingBalanceRecord balanceRecord);
    void ReceiveTradeStatusRecord(StreamingTradeStatusRecord tradeStatusRecord);
    void ReceiveProfitRecord(StreamingProfitRecord profitRecord);
    void ReceiveNewsRecord(StreamingNewsRecord newsRecord);
    void ReceiveKeepAliveRecord(StreamingKeepAliveRecord keepAliveRecord);
    void ReceiveCandleRecord(StreamingCandleRecord candleRecord);
}