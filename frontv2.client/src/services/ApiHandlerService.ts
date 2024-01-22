// apiService.tsx
import { IPublicClientApplication } from "@azure/msal-browser";
import { SymbolInfo } from "../modeles/SymbolInfo.ts";
import { ConnectDto } from "../modeles/Connect.ts";
import { apiMiddleware } from "./apiMiddleware.ts";

export const apiHandlerService = {
  async connect(
    msalInstance: IPublicClientApplication,
    connectDto: ConnectDto,
  ): Promise<void> {
    await apiMiddleware(msalInstance, "/api/ApiHandler/connect", {
      method: "POST",
      body: JSON.stringify({
        user: connectDto.user,
        pwd: connectDto.pwd,
        handlerEnum: connectDto.handlerEnum,
      }),
    });
  },

  async disconnect(msalInstance: IPublicClientApplication): Promise<void> {
    await apiMiddleware(msalInstance, "/api/ApiHandler/disconnect", {
      method: "POST",
    });
  },

  async isConnected(msalInstance: IPublicClientApplication): Promise<boolean> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/ApiHandler/isConnected",
      { method: "GET" },
    );
    return await response.json();
  },

  async getTypeHandler(
    msalInstance: IPublicClientApplication,
  ): Promise<string> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/ApiHandler/typeHandler",
      { method: "GET" },
    );
    return response.text();
  },

  async getListHandler(
    msalInstance: IPublicClientApplication,
  ): Promise<string[]> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/ApiHandler/listHandlers",
      {
        method: "GET",
      },
    );
    return await response.json();
  },

  async getAllSymbol(
    msalInstance: IPublicClientApplication,
  ): Promise<SymbolInfo[]> {
    const response = await apiMiddleware(
      msalInstance,
      "/api/ApiHandler/allSymbols",
      { method: "GET" },
    );
    return await response.json();
  },
};
