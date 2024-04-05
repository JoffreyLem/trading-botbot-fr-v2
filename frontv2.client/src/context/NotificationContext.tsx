import { createContext, ReactNode, useContext, useState } from "react";
import NotificationPopup from "../common/NotificationPopup.tsx";

interface Notification {
  id: string;
  message: string;
  info?: string;
  type: "error" | "info";
}

type NotificationContextType = {
  notifications: Notification[];
  addNotification: (
    message: string,
    type: "error" | "info",
    info?: string,
  ) => void;
  removeNotification: (id: string) => void;
};

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined,
);

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error(
      "useNotification must be used within a NotificationProvider",
    );
  }
  return context;
};

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const addNotification = (
    message: string,
    type: "error" | "info",
    info?: string,
  ) => {
    const id = new Date().getTime().toString();
    setNotifications((prevNotifications) => [
      ...prevNotifications,
      { id, message, type, info },
    ]);
  };

  const removeNotification = (id: string) => {
    setNotifications((prevNotifications) =>
      prevNotifications.filter((notification) => notification.id !== id),
    );
  };

  return (
    <NotificationContext.Provider
      value={{ notifications, addNotification, removeNotification }}
    >
      <NotificationPopup />
      {children}
    </NotificationContext.Provider>
  );
};
