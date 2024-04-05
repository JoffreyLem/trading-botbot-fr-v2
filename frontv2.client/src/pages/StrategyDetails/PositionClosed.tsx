import React, { useEffect, useState } from "react";

import { Position } from "../../modeles/Position";

import LoadSpinner from "../../common/LoadSpinner";

import PositionComponent from "./Components/PositionComponent.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const PositionClosed: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<Position[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  useEffect(() => {
    setIsLoading(true);
    StrategyService.getStrategyPositionClosed(strategyId)
      .then((response) => setPositions(response))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  return <PositionComponent positions={positions} positionClosed={true} />;
};

export default PositionClosed;
