import React, { useEffect, useState } from "react";

import { Position } from "../../modeles/Position";
import { useMsal } from "@azure/msal-react";
import { strategyService } from "../../services/StrategyHandlerService";
import LoadSpinner from "../../common/LoadSpinner";

import * as signalR from "@microsoft/signalr";
import { getAuthToken } from "../../services/msalAuthService.ts";
import PositionComponent from "./Components/PositionComponent.tsx";

const PositionOpened: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { instance } = useMsal();

  useEffect(() => {
    setIsLoading(true);
    strategyService
      .getOpenedPositions(instance, strategyId)
      .then((r) => setPositions(r))
      .catch((err) => setError(err.message))
      .finally(() => {
        setIsLoading(false);
      });
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/infoClient", {
        accessTokenFactory: () => getAuthToken(instance),
      })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => {
        console.log("Connected to the hub");

        connection.on("ReceivePosition", (position: Position) => {
          setPositions((prevPositions) => {
            const existingPositionIndex = prevPositions.findIndex(
              (p) => p.id === position.id,
            );
            let newPositions = [...prevPositions];

            switch (position.positionState) {
              case "Opened":
                if (existingPositionIndex === -1) {
                  newPositions.push(position);
                }
                break;
              case "Updated":
                if (existingPositionIndex >= 0) {
                  newPositions[existingPositionIndex] = {
                    ...newPositions[existingPositionIndex],
                    ...position,
                  };
                } else {
                  newPositions.push(position);
                }
                break;
              case "Closed":
              case "Rejected":
                newPositions = newPositions.filter((p) => p.id !== position.id);
                break;
              default:
                break;
            }
            return newPositions;
          });
        });
      })
      .catch((err) => console.error("Connection error: ", err));
  }, [instance, strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (error) {
    return <div>Erreur: {error}</div>;
  }

  return <PositionComponent positions={positions} positionClosed={false} />;
};

export default PositionOpened;
