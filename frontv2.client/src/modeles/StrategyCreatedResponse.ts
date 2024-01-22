import { StrategyFile } from "./StrategyFile.ts";

export interface StrategyCreatedResponse {
  created: boolean;
  strategyFile: StrategyFile;
  errors?: string[];
}
