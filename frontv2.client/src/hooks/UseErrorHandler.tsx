// ErrorHandler.tsx

import { ApiErrorResponseException } from "../exceptions/ApiErrorResponseException.ts";
import { useNotification } from "../context/NotificationContext.tsx";

export const useErrorHandler = () => {
  const { addNotification } = useNotification();

  return (error: Error | ApiErrorResponseException) => {
    console.error(error);
    addNotification(error.message, "error");
  };
};
