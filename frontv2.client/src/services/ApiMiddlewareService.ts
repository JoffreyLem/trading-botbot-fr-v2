import { ApiErrorResponseException } from "../exceptions/ApiErrorResponseException.ts";

import { MsalAuthService } from "./MsalAuthService.ts";
import { ApiResponseError } from "../modeles/ApiResponseError.ts";

export class ApiMiddlewareService {
  static async callApi<T>(url: string, options: RequestInit): Promise<T> {
    const response = await ApiMiddlewareService.performRequest(url, options);

    const contentType = response.headers.get("Content-Type");

    if (contentType && contentType.includes("application/json")) {
      return (await response.json()) as T;
    } else if (
      contentType &&
      (contentType.includes("text/plain") || contentType.includes("text/html"))
    ) {
      const textResponse = await response.text();
      return textResponse as unknown as T;
    } else {
      //TODO : Voir comment traiter ici ?
    }
  }

  static async callApiWithoutResponse(
    url: string,
    options: RequestInit,
  ): Promise<void> {
    await ApiMiddlewareService.performRequest(url, options);
  }

  private static async performRequest(
    url: string,
    options: RequestInit,
  ): Promise<Response> {
    try {
      const accessToken = await MsalAuthService.getAuthToken();

      const headers = new Headers(options.headers || {});
      headers.set("Authorization", `Bearer ${accessToken}`);
      if (
        options.method === "POST" &&
        !(options.body instanceof FormData) &&
        !headers.has("Content-Type")
      ) {
        headers.set("Content-Type", "application/json");
      }

      const fetchOptions = { ...options, headers };

      const response = await fetch(url, fetchOptions);

      if (!response.ok) {
        if (
          response.headers.get("Content-Type")?.includes("application/json")
        ) {
          const errorBody: ApiResponseError = await response.json();
          throw new ApiErrorResponseException(
            errorBody.Errors,
            errorBody.Error,
          );
        }
      }

      return response;
    } catch (error) {
      if (error instanceof ApiErrorResponseException) {
        throw error;
      } else {
        console.log(error);
        throw new Error("Une erreur inattendue est survenue.");
      }
    }
  }
}
