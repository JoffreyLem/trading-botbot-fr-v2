import { IPublicClientApplication } from "@azure/msal-browser";
import { StrategyCreatedResponse } from "../modeles/StrategyCreatedResponse";
import { StrategyFile } from "../modeles/StrategyFile";
import { apiMiddleware } from "./apiMiddleware";

export const strategyGeneratorService = {
  async createNewStrategy(
    msalInstance: IPublicClientApplication,
    file: string,
  ): Promise<StrategyCreatedResponse> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/StrategyGenerator",
      {
        method: "POST",
        body: JSON.stringify({ file }),
      },
    );

    return await response.json();
  },

  async getAllStrategyFiles(
    msalInstance: IPublicClientApplication,
  ): Promise<StrategyFile[]> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/StrategyGenerator/GetAll",
      {
        method: "GET",
      },
    );

    return await response.json();
  },

  async getStrategyFile(
    msalInstance: IPublicClientApplication,
    id: number,
  ): Promise<StrategyFile> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/StrategyGenerator/${id}`,
      {
        method: "GET",
      },
    );

    return await response.json();
  },

  async deleteStrategyFile(
    msalInstance: IPublicClientApplication,
    id: number,
  ): Promise<void> {
    await apiMiddleware(msalInstance, `/api/StrategyGenerator/${id}`, {
      method: "DELETE",
    });
  },

  async updateStrategyFile(
    msalInstance: IPublicClientApplication,
    strategyFile: StrategyFile,
  ): Promise<StrategyFile> {
    const response = await apiMiddleware(
      msalInstance,
      `/api/StrategyGenerator/${strategyFile.id}`,
      {
        method: "PUT",
        body: JSON.stringify(strategyFile),
      },
    );

    return await response.json();
  },
};
