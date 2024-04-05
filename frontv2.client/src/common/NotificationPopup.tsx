import React, { useEffect } from "react";
import { useNotification } from "../context/NotificationContext.tsx";

const NotificationPopup: React.FC = () => {
  const { notifications, removeNotification } = useNotification();

  useEffect(() => {
    const timers = notifications.map((notification) =>
      setTimeout(() => {
        removeNotification(notification.id);
      }, 10000),
    );

    return () => timers.forEach((timer) => clearTimeout(timer));
  }, [notifications, removeNotification]);

  const notificationContainerStyle: React.CSSProperties = {
    position: "fixed",
    top: "20px",
    right: "20px",
    zIndex: 1050,
    width: "300px",
    maxHeight: "calc(100% - 40px)",
    overflowY: "auto",
  };

  const closeButtonStyle: React.CSSProperties = {
    position: "absolute",
    top: "0.5rem",
    right: "0.5rem",
    border: "none",
    background: "transparent",
    cursor: "pointer",
    color: "#000",
    fontSize: "1.5rem",
  };

  if (notifications.length === 0) return null;

  return (
    <div style={notificationContainerStyle}>
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={`alert alert-${notification.type === "error" ? "danger" : "info"}`}
          role="alert"
          style={{ position: "relative", marginBottom: "1rem" }}
        >
          <strong>{notification.type.toUpperCase()}</strong>:{" "}
          {notification.message}
          {notification.info && <div>Info: {notification.info}</div>}
          <button
            onClick={() => removeNotification(notification.id)}
            style={closeButtonStyle}
            aria-label="Close"
          >
            &times;
          </button>
        </div>
      ))}
    </div>
  );
};

export default NotificationPopup;
