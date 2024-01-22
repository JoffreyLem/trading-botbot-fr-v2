// apiMiddleware.ts

import { getAuthToken } from "./msalAuthService.ts";
import { IPublicClientApplication } from "@azure/msal-browser";
import { ApiResponseError } from "../modeles/ApiResponseError.ts";
import { ApiError } from "../modeles/ApiError.ts";

export const apiMiddleware = async (
  msalInstance: IPublicClientApplication,
  url: string,
  options: RequestInit,
): Promise<Response> => {
  try {
    const accessToken = await getAuthToken(msalInstance);

    const headers = new Headers(options.headers || {});
    headers.set("Authorization", `Bearer ${accessToken}`);
    if (options.method === "POST" && !headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    const fetchOptions = { ...options, headers };

    const response = await fetch(url, fetchOptions);

    if (!response.ok) {
      let errorMessages = ["Erreur inconnue"];
      if (response.headers.get("Content-Type")?.includes("application/json")) {
        try {
          const errorBody: ApiResponseError = await response.json();
          console.log("Error response :", errorBody);
          errorMessages = errorBody.errors || errorMessages;
        } catch (e) {
          /* empty */
        }
      }
      throw new ApiError("Erreur de l'API", errorMessages, response.status);
    }

    return response;
  } catch (error: unknown) {
    if (error instanceof ApiError) {
      throw error;
    }
    if (error instanceof Error) {
      throw new Error("Une erreur inattendue est survenue: " + error.message);
    } else {
      throw new Error("Une erreur inattendue est survenue.");
    }
  }
};
