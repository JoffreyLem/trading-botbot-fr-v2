import { ConnectDto } from "../modeles/Connect.ts";
import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import { SymbolInfo } from "../modeles/SymbolInfo.ts";
import { ApiResponse } from "../modeles/ApiResponse.ts";

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
    const response = await ApiMiddlewareService.callApi<ApiResponse<boolean>>(
      "/api/ApiHandler/isConnected",
      { method: "GET" },
    );
    return response.data;
  }

  static async getTypeHandler(): Promise<string> {
    const response = await ApiMiddlewareService.callApi<ApiResponse<string>>(
      "/api/ApiHandler/typeHandler",
      { method: "GET" },
    );

    return response.data;
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
