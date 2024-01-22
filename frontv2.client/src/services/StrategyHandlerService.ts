import { IPublicClientApplication } from "@azure/msal-browser";
import { StrategyInfo } from "../modeles/StrategyInfo";
import { StrategyInit } from "../modeles/StrategyInit";
import { Position } from "../modeles/Position";

import { apiMiddleware } from "./apiMiddleware";
import { Result } from "../modeles/Result.ts";
import { BacktestRequest } from "../modeles/BacktestRequest.ts";
import { Backtest } from "../modeles/Backtest.ts";

export const strategyService = {
  async initStrategy(
    msalInstance: IPublicClientApplication,
    strategyInitDto: StrategyInit,
  ): Promise<void> {
    await apiMiddleware(msalInstance, "/api/Strategy/init", {
      method: "POST",
      body: JSON.stringify(strategyInitDto),
    });
  },

  async getListTimeframes(
    msalInstance: IPublicClientApplication,
  ): Promise<string[]> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/Strategy/timeframes",
      {
        method: "GET",
      },
    );
    return response.json();
  },

  async getAllStrategy(
    msalInstance: IPublicClientApplication,
  ): Promise<StrategyInfo[]> {
    const response = await apiMiddleware(msalInstance, "/api/Strategy/all", {
      method: "GET",
    });
    if (
      response.status === 204 ||
      response.headers.get("Content-Length") === "0"
    ) {
      return [];
    }

    return response.json();
  },

  async closeStrategy(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<void> {
    await apiMiddleware(msalInstance, `/api/Strategy/close/${id}`, {
      method: "POST",
    });
  },

  async getStrategyInfo(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<StrategyInfo> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/${id}/info`,
      {
        method: "GET",
      },
    );
    return response.json();
  },

  async getStrategyPositionClosed(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<Position[]> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/${id}/positions/closed`,
      {
        method: "GET",
      },
    );
    if (
      response.status === 204 ||
      response.headers.get("Content-Length") === "0"
    ) {
      return [];
    }
    return response.json();
  },

  async getResult(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<Result> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/${id}/result`,
      {
        method: "GET",
      },
    );
    return response.json();
  },

  async setCanRun(
    msalInstance: IPublicClientApplication,
    id: string,
    value: boolean,
  ): Promise<void> {
    const url = new URL(`/api/Strategy/${id}/canrun`, window.location.origin);
    url.searchParams.append("value", value.toString());

    await apiMiddleware(msalInstance, url.toString(), {
      method: "POST",
    });
  },

  async getOpenedPositions(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<Position[]> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/${id}/positions/opened`,
      {
        method: "GET",
      },
    );
    if (
      response.status === 204 ||
      response.headers.get("Content-Length") === "0"
    ) {
      return [];
    }
    return response.json();
  },

  async runBackTest(
    msalInstance: IPublicClientApplication,
    id: string,
    backTestDto: BacktestRequest,
  ): Promise<Backtest> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/runbacktest/${id}`,
      {
        method: "POST",
        body: JSON.stringify(backTestDto),
      },
    );
    return response.json();
  },

  async getBacktestResult(
    msalInstance: IPublicClientApplication,
    id: string,
  ): Promise<Backtest> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/Strategy/${id}/resultBacktest`,
      {
        method: "GET",
      },
    );
    return response.json();
  },
};
