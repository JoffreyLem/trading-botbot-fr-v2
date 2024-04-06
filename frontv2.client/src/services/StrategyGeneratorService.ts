import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import { StrategyFile } from "../modeles/StrategyFile.ts";
import { StrategyCompilationResponse } from "../modeles/StrategyCompilationResponse.ts";

export class StrategyGeneratorService {
  static async createNewStrategy(
    file: File,
  ): Promise<StrategyCompilationResponse> {
    const formData = new FormData();
    formData.append("file", file);

    return await ApiMiddlewareService.callApi<StrategyCompilationResponse>(
      "/api/StrategyGenerator",
      {
        method: "POST",
        body: formData,
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
    id: number,
    file: File,
  ): Promise<StrategyCompilationResponse> {
    const formData = new FormData();
    formData.append("file", file);
    return await ApiMiddlewareService.callApi<StrategyCompilationResponse>(
      `/api/StrategyGenerator/${id}`,
      {
        method: "PUT",
        body: formData,
      },
    );
  }
}
