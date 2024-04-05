import React, { useEffect, useState } from "react";

import { Position } from "../../modeles/Position";

import LoadSpinner from "../../common/LoadSpinner";

import * as signalR from "@microsoft/signalr";

import PositionComponent from "./Components/PositionComponent.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { MsalAuthService } from "../../services/MsalAuthService.ts";

const PositionOpened: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<Position[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);
    StrategyService.getOpenedPositions(strategyId)
      .then((r) => setPositions(r))
      .catch(handleError)
      .finally(() => {
        setIsLoading(false);
      });
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/infoClient", {
        accessTokenFactory: () => MsalAuthService.getAuthToken(),
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
  }, [strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  return <PositionComponent positions={positions} positionClosed={false} />;
};

export default PositionOpened;
