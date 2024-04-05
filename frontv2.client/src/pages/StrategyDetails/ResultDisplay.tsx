import React, { useEffect, useState } from "react";
import { Result } from "../../modeles/Result.ts";

import LoadSpinner from "../../common/LoadSpinner.tsx";
import ResultDataDisplayComponent from "./Components/ResultDataDisplayComponent.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const ResultDisplay: React.FC<{ strategyId: string }> = ({ strategyId }) => {
  const [result, setResult] = useState<Result>();

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);

    StrategyService.getResult(strategyId)
      .then((data) => setResult(data))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  return <ResultDataDisplayComponent result={result} />;
};

export default ResultDisplay;
