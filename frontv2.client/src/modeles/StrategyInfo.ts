import { Tick } from "./Tick.ts";
import { Candle } from "./Candle.ts";
import { Backtest } from "./Backtest.ts";

export interface StrategyInfo {
  id: string;

  symbol: string;
  timeframe: string;
  timeframe2: string;
  strategyName: string;
  canRun: boolean;
  strategyDisabled: boolean;
  secureControlPosition: boolean;
  lastTick: Tick;
  lastCandle: Candle;
  backtest: Backtest;
}
