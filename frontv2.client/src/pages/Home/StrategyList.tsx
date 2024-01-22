import React, { useContext, useEffect, useState } from "react";

import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";
import { strategyService } from "../../services/StrategyHandlerService.ts";
import { StrategyInfo } from "../../modeles/StrategyInfo.ts";
import { useNavigate } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";
import "./css/StrategyList.css";
import { StrategyContext } from "./StrategyProvider.tsx";
import { ApiError } from "../../modeles/ApiError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";

const StrategyList: React.FC = () => {
  const [allStrategy, setAllStrategy] = useState<StrategyInfo[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<ApiError>();
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { refreshList } = useContext(StrategyContext);
  const handleRowClick = (strategyId: string) => {
    navigate(`/strategy/${strategyId}`);
  };

  useEffect(() => {
    setIsLoading(true);

    const getAllStrategy = strategyService
      .getAllStrategy(instance)
      .then((response) => setAllStrategy(response))
      .catch((err) => setError(err.message));

    Promise.all([getAllStrategy]).finally(() => {
      setIsLoading(false);
    });
  }, [instance, refreshList]);

  const handleDelete = (
    event: React.MouseEvent<HTMLButtonElement>,
    strategyId: string,
  ) => {
    setIsLoading(true);
    event.stopPropagation();
    strategyService
      .closeStrategy(instance, strategyId)
      .then(() => {
        setAllStrategy((prevStrategies) =>
          prevStrategies.filter((strategy) => strategy.id !== strategyId),
        );
      })
      .catch((err: ApiError) => setActionError(err))
      .finally(() => {
        setIsLoading(false);
      });
  };

  if (error) {
    return <div>Erreur: {error}</div>;
  }

  if (isLoading) {
    return <LoadSpinner />;
  }

  return (
    <div>
      {actionError && (
        <ErrorComponent
          title="Erreur de suppression"
          errors={actionError.errors}
        />
      )}
      <table className="table table-hover">
        <thead>
          <tr>
            <th>Strategy Name</th>
            <th>Symbol</th>
            <th>Timeframe</th>
            <th>Supprimer</th>
          </tr>
        </thead>
        <tbody>
          {allStrategy.map((strategy) => (
            <tr key={strategy.id} onClick={() => handleRowClick(strategy.id)}>
              <td>{strategy.strategyName}</td>
              <td>{strategy.symbol}</td>
              <td>{strategy.timeframe}</td>
              <td>
                <button onClick={(event) => handleDelete(event, strategy.id)}>
                  Supprimer
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default StrategyList;
