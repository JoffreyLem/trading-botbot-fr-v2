import React, { useContext, useEffect, useState } from "react";

import LoadSpinner from "../../common/LoadSpinner.tsx";

import { StrategyInfo } from "../../modeles/StrategyInfo.ts";
import { useNavigate } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";
import "./css/StrategyList.css";
import { StrategyContext } from "./StrategyProvider.tsx";
import { ApiErrorResponse } from "../../modeles/ApiResponseError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const StrategyList: React.FC = () => {
  const [allStrategy, setAllStrategy] = useState<StrategyInfo[]>([]);

  const [actionError, setActionError] = useState<ApiErrorResponse>();

  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { refreshList } = useContext(StrategyContext);
  const handleError = useErrorHandler();
  const handleRowClick = (strategyId: string) => {
    navigate(`/strategy/${strategyId}`);
  };

  useEffect(() => {
    setIsLoading(true);

    const getAllStrategy = StrategyService.getAllStrategy()
      .then((response) => setAllStrategy(response))
      .catch(handleError);

    Promise.all([getAllStrategy]).finally(() => {
      setIsLoading(false);
    });
  }, [refreshList]);

  const handleDelete = (
    event: React.MouseEvent<HTMLButtonElement>,
    strategyId: string,
  ) => {
    setIsLoading(true);
    event.stopPropagation();
    StrategyService.closeStrategy(strategyId)
      .then(() => {
        setAllStrategy((prevStrategies) =>
          prevStrategies.filter((strategy) => strategy.id !== strategyId),
        );
      })
      .catch((err: ApiErrorResponse) => setActionError(err))
      .finally(() => {
        setIsLoading(false);
      });
  };

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
