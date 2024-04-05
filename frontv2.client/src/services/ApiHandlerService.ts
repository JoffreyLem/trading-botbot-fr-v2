import { ConnectDto } from "../modeles/Connect.ts";
import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import { SymbolInfo } from "../modeles/SymbolInfo.ts";

export class ApiHandlerService {
  static async connect(connectDto: ConnectDto): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(
      "/api/ApiHandler/connect",
      {
        method: "POST",
        body: JSON.stringify({
          user: connectDto.user,
          pwd: connectDto.pwd,
          handlerEnum: connectDto.handlerEnum,
        }),
      },
    );
  }

  static async disconnect(): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(
      "/api/ApiHandler/disconnect",
      {
        method: "POST",
      },
    );
  }

  static async isConnected(): Promise<boolean> {
    return await ApiMiddlewareService.callApi<boolean>(
      "/api/ApiHandler/isConnected",
      { method: "GET" },
    );
  }

  static async getTypeHandler(): Promise<string> {
    return await ApiMiddlewareService.callApi<string>(
      "/api/ApiHandler/typeHandler",
      { method: "GET" },
    );
  }

  static async getListHandler(): Promise<string[]> {
    return await ApiMiddlewareService.callApi<string[]>(
      "/api/ApiHandler/listHandlers",
      { method: "GET" },
    );
  }

  static async getAllSymbol(): Promise<SymbolInfo[]> {
    return await ApiMiddlewareService.callApi<SymbolInfo[]>(
      "/api/ApiHandler/allSymbols",
      { method: "GET" },
    );
  }
}
