using RobotAppLibraryV2.Api.Xtb.Code;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Api.Xtb.Assembler;

public static class ToXtbAssembler
{
    public static long ToPeriodCode(Timeframe timeframe)
    {
        switch (timeframe)
        {
            case Timeframe.OneMinute:
                return 1;
            case Timeframe.FiveMinutes:
                return 5;
            case Timeframe.FifteenMinutes:
                return 15;
            case Timeframe.ThirtyMinutes:
                return 30;
            case Timeframe.OneHour:
                return 60;
            case Timeframe.FourHour:
                return 240;
            case Timeframe.Daily:
                return 1440;
            case Timeframe.Weekly:
                return 10080;
            case Timeframe.Monthly:
                return 43200;
            default:
                throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, null);
        }
    }

    public static long ToTradeOperationCode(TypeOperation typePosition)
    {
        switch (typePosition)
        {
            case TypeOperation.Buy:
                return 0;
            case TypeOperation.Sell:
                return 1;
            case TypeOperation.BuyLimit:
                return 2;
            case TypeOperation.SellLimit:
                return 3;
            case TypeOperation.BuyStop:
                return 4;
            case TypeOperation.SellStop:
                return 5;
            case TypeOperation.Balance:
                return 6;
            case TypeOperation.Credit:
            case TypeOperation.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(typePosition), typePosition, null);
        }
    }

    public static long ToTradeTransactionType(TransactionType transactionType)
    {
        switch (transactionType)
        {
            case TransactionType.Open:
                return 0;
            case TransactionType.Close:
                return 2;
            case TransactionType.Modify:
                return 3;
            case TransactionType.Delete:
                return 4;
            default:
                throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
        }
    }


    public static (PERIOD_CODE periodCode, DateTime dateTime) SetDateTime(Timeframe tf)
    {
        DateTime dateTime;
        PERIOD_CODE periodCodeData;
        switch (tf)
        {
            case Timeframe.OneMinute:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M1;
                return (periodCodeData, dateTime);

            case Timeframe.FiveMinutes:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M5;
                return (periodCodeData, dateTime);

            case Timeframe.FifteenMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M15;
                return (periodCodeData, dateTime);
            case Timeframe.ThirtyMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M30;
                return (periodCodeData, dateTime);
            case Timeframe.OneHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H1;
                return (periodCodeData, dateTime);
            case Timeframe.FourHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H4;
                return (periodCodeData, dateTime);
            case Timeframe.Daily:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_D1;
                return (periodCodeData, dateTime);
            case Timeframe.Weekly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_W1;
                return (periodCodeData, dateTime);
            case Timeframe.Monthly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_MN1;
                return (periodCodeData, dateTime);


            default:
                throw new ArgumentException("Periode code n'existe pas");
        }
    }
}