import { strategyService } from "../../services/StrategyHandlerService.ts";
import React, { useEffect, useState } from "react";
import { Result } from "../../modeles/Result.ts";
import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";
import ResultDataDisplayComponent from "./Components/ResultDataDisplayComponent.tsx";

const ResultDisplay: React.FC<{ strategyId: string }> = ({ strategyId }) => {
  const [result, setResult] = useState<Result>();
  const [error, setError] = useState<string | null>(null);
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsLoading(true);

    strategyService
      .getResult(instance, strategyId)
      .then((data) => setResult(data))
      .catch((err) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [instance, strategyId]);

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (error) {
    return <div className="alert alert-danger">Erreur: {error}</div>;
  }

  return <ResultDataDisplayComponent result={result} />;
};

export default ResultDisplay;
