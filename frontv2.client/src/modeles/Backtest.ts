import { Result } from "./Result.ts";

export interface Backtest {
  isBackTestRunning: boolean;
  lastBackTestExecution: Date;
  resultBacktest: Result;
}
