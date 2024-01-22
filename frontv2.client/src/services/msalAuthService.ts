import {
  InteractionRequiredAuthError,
  IPublicClientApplication,
} from "@azure/msal-browser";

export const getAuthToken = async (
  msalInstance: IPublicClientApplication,
): Promise<string> => {
  const request = {
    scopes: ["api://21543424-93d7-4cf1-a776-383de1100a79/access_as_user"],
    account: msalInstance.getAllAccounts()[0],
  };
  try {
    const response = await msalInstance.acquireTokenSilent(request);
    return response.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      // Fallback to interactive method if silent fails
      const interactiveResponse = await msalInstance.acquireTokenPopup(request);
      return interactiveResponse.accessToken;
    } else {
      // Handle other errors
      throw error;
    }
  }
};
