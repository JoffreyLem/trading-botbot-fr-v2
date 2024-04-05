import { StrategyCreatedResponse } from "../modeles/StrategyCreatedResponse.ts";
import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import { StrategyFile } from "../modeles/StrategyFile.ts";

export class StrategyGeneratorService {
  static async createNewStrategy(
    file: string,
  ): Promise<StrategyCreatedResponse> {
    return await ApiMiddlewareService.callApi<StrategyCreatedResponse>(
      "/api/StrategyGenerator",
      {
        method: "POST",
        body: JSON.stringify({ file }),
      },
    );
  }

  static async getAllStrategyFiles(): Promise<StrategyFile[]> {
    return await ApiMiddlewareService.callApi<StrategyFile[]>(
      "/api/StrategyGenerator/GetAll",
      {
        method: "GET",
      },
    );
  }

  static async getStrategyFile(id: number): Promise<StrategyFile> {
    return await ApiMiddlewareService.callApi<StrategyFile>(
      `/api/StrategyGenerator/${id}`,
      {
        method: "GET",
      },
    );
  }

  static async deleteStrategyFile(id: number): Promise<void> {
    return await ApiMiddlewareService.callApiWithoutResponse(
      `/api/StrategyGenerator/${id}`,
      {
        method: "DELETE",
      },
    );
  }

  static async updateStrategyFile(
    strategyFile: StrategyFile,
  ): Promise<StrategyFile> {
    return await ApiMiddlewareService.callApi<StrategyFile>(
      `/api/StrategyGenerator/${strategyFile.id}`,
      {
        method: "PUT",
        body: JSON.stringify(strategyFile),
      },
    );
  }
}
