import { StrategyFile } from "./StrategyFile.ts";

export interface StrategyCompilationResponse {
  compiled: boolean;
  errors?: string[];
  strategyFileDto?: StrategyFile;
}
