import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { Candle } from "../../modeles/Candle.ts";
import { Tick } from "../../modeles/Tick.ts";
import { getAuthToken } from "../../services/msalAuthService.ts";
import { useMsal } from "@azure/msal-react";

const TradingData: React.FC = () => {
  const [currentCandle, setCurrentCandle] = useState<Candle | null>(null);
  const [currentTick, setCurrentTick] = useState<Tick | null>(null);
  const { instance } = useMsal();

  useEffect(() => {
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

        connection.on("ReceiveCandle", (candle: Candle) => {
          setCurrentCandle(candle);
        });

        connection.on("ReceiveTick", (tick: Tick) => {
          setCurrentTick(tick);
        });
      })
      .catch((err) => console.error("Connection error: ", err));

    return () => {
      connection.stop();
    };
  }, [instance]);

  return (
    <div>
      <h2>Current Candle</h2>
      {currentCandle && (
        <div>{`Date: ${currentCandle.date}, Open: ${currentCandle.open}, High: ${currentCandle.high}, Low: ${currentCandle.low}, Close: ${currentCandle.close}`}</div>
      )}
      <h2>Current Tick</h2>
      {currentTick && (
        <div>{`Date: ${currentTick.date}, Ask: ${currentTick.ask}, Bid: ${currentTick.bid}`}</div>
      )}
    </div>
  );
};

export default TradingData;
