import { StrategyInfo } from "../modeles/StrategyInfo";
import { StrategyInit } from "../modeles/StrategyInit";
import { Position } from "../modeles/Position";

import { Result } from "../modeles/Result";
import { BacktestRequest } from "../modeles/BacktestRequest";
import { Backtest } from "../modeles/Backtest";
import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";

export class StrategyService {
  static async initStrategy(strategyInitDto: StrategyInit): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse("/api/Strategy/init", {
      method: "POST",
      body: JSON.stringify(strategyInitDto),
    });
  }

  static async getListTimeframes(): Promise<string[]> {
    return await ApiMiddlewareService.callApi<string[]>(
      "/api/Strategy/timeframes",
      {
        method: "GET",
      },
    );
  }

  static async getAllStrategy(): Promise<StrategyInfo[]> {
    return await ApiMiddlewareService.callApi<StrategyInfo[]>(
      "/api/Strategy/all",
      {
        method: "GET",
      },
    );
  }

  static async closeStrategy(id: string): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(
      `/api/Strategy/close/${id}`,
      {
        method: "POST",
      },
    );
  }

  static async getStrategyInfo(id: string): Promise<StrategyInfo> {
    return await ApiMiddlewareService.callApi<StrategyInfo>(
      `/api/Strategy/${id}/info`,
      {
        method: "GET",
      },
    );
  }

  static async getStrategyPositionClosed(id: string): Promise<Position[]> {
    return await ApiMiddlewareService.callApi<Position[]>(
      `/api/Strategy/${id}/positions/closed`,
      {
        method: "GET",
      },
    );
  }

  static async getResult(id: string): Promise<Result> {
    return await ApiMiddlewareService.callApi<Result>(
      `/api/Strategy/${id}/result`,
      {
        method: "GET",
      },
    );
  }

  static async setCanRun(id: string, value: boolean): Promise<void> {
    const url = new URL(`/api/Strategy/${id}/canrun`, window.location.origin);
    url.searchParams.append("value", value.toString());

    await ApiMiddlewareService.callApiWithoutResponse(url.toString(), {
      method: "POST",
    });
  }

  static async getOpenedPositions(id: string): Promise<Position[]> {
    return await ApiMiddlewareService.callApi<Position[]>(
      `/api/Strategy/${id}/positions/opened`,
      {
        method: "GET",
      },
    );
  }

  static async runBackTest(
    id: string,
    backTestDto: BacktestRequest,
  ): Promise<Backtest> {
    return await ApiMiddlewareService.callApi<Backtest>(
      `/api/Strategy/runbacktest/${id}`,
      {
        method: "POST",
        body: JSON.stringify(backTestDto),
      },
    );
  }

  static async getBacktestResult(id: string): Promise<Backtest> {
    return await ApiMiddlewareService.callApi<Backtest>(
      `/api/Strategy/${id}/resultBacktest`,
      {
        method: "GET",
      },
    );
  }
}
