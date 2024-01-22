import React, { useEffect, useState } from "react";

import { Position } from "../../modeles/Position";
import { useMsal } from "@azure/msal-react";
import { strategyService } from "../../services/StrategyHandlerService";
import LoadSpinner from "../../common/LoadSpinner";

import PositionComponent from "./Components/PositionComponent.tsx";

const PositionClosed: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { instance } = useMsal();

  useEffect(() => {
    setIsLoading(true);
    strategyService
      .getStrategyPositionClosed(instance, strategyId)
      .then((response) => setPositions(response))
      .catch((err) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [instance, strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (error) {
    return <div>Erreur: {error}</div>;
  }

  return <PositionComponent positions={positions} positionClosed={true} />;
};

export default PositionClosed;
