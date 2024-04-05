import React, { useEffect, useState } from "react";

import LoadSpinner from "../../common/LoadSpinner.tsx";

import { BacktestRequest } from "../../modeles/BacktestRequest.ts";
import { ApiErrorResponse } from "../../modeles/ApiResponseError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";

import ResultDataDisplayComponent from "./Components/ResultDataDisplayComponent.tsx";
import { Backtest } from "../../modeles/Backtest.ts";
import { Result } from "../../modeles/Result.ts";
import { StrategyService } from "../../services/StrategyHandlerService.ts";

const BackTestForm: React.FC<{ strategyId: string }> = ({ strategyId }) => {
  const [formData, setFormData] = useState<BacktestRequest>({
    balance: 1000,
    minSpread: 1,
    maxSpread: 1,
  });
  const initialBacktestState: Backtest = {
    isBackTestRunning: false,
    lastBackTestExecution: new Date(),
    resultBacktest: {} as Result,
  };
  const [isLoading, setIsLoading] = useState(false);

  const [actionError, setActionError] = useState<ApiErrorResponse>();
  const [backtest, setBacktest] = useState<Backtest>(initialBacktestState);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setIsLoading(true);

    StrategyService.getBacktestResult(strategyId)
      .then((data) => setBacktest(data))
      .catch((err) => setError(err.message))
      .finally(() => {
        if (!backtest.isBackTestRunning) {
          setIsLoading(false);
        }
      });
  }, [backtest?.isBackTestRunning, strategyId]);
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: parseFloat(e.target.value) });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    StrategyService.runBackTest(strategyId, formData)
      .then((r) => {
        setBacktest(r);
        setIsLoading(false);
      })
      .catch((err: ApiErrorResponse) => {
        setIsLoading(false);
        setActionError(err);
      });
  };

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (error) {
    return <div className="alert alert-danger">Erreur: {error}</div>;
  }

  return (
    <div>
      {actionError && (
        <ErrorComponent
          title="Erreur d'execution du backtest"
          errors={actionError.errors}
        />
      )}
      {backtest?.isBackTestRunning ? (
        <div>Backtest currently running</div>
      ) : (
        <div>
          <div className="backtest-form">
            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label htmlFor="balance" className="form-label">
                  Balance:
                </label>
                <input
                  type="number"
                  id="balance"
                  name="balance"
                  value={formData.balance}
                  onChange={handleChange}
                  className="form-control"
                />
              </div>
              <div className="mb-3">
                <label htmlFor="minSpread" className="form-label">
                  Min Spread:
                </label>
                <input
                  type="number"
                  id="minSpread"
                  name="minSpread"
                  value={formData.minSpread}
                  onChange={handleChange}
                  className="form-control"
                />
              </div>
              <div className="mb-3">
                <label htmlFor="maxSpread" className="form-label">
                  Max Spread:
                </label>
                <input
                  type="number"
                  id="maxSpread"
                  name="maxSpread"
                  value={formData.maxSpread}
                  onChange={handleChange}
                  className="form-control"
                />
              </div>
              <button type="submit" className="btn btn-primary">
                Run
              </button>
            </form>
          </div>
          <div>
            <label htmlFor="lsatExecution" className="form-label">
              Last execution date:
            </label>
            <input
              type="text"
              id="lsatExecution"
              name="lsatExecution"
              value={backtest?.lastBackTestExecution?.toString()}
              onChange={handleChange}
              className="form-control"
            />
            <ResultDataDisplayComponent result={backtest?.resultBacktest} />
          </div>
        </div>
      )}
    </div>
  );
};

export default BackTestForm;
