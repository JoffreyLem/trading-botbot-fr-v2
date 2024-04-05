import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { useMsal } from "@azure/msal-react";
import { StrategyInfo } from "../../modeles/StrategyInfo.ts";

import LoadSpinner from "../../common/LoadSpinner.tsx";
import StrategyDataInfo from "./StrategyDataInfo.tsx";
import GraphComponent from "./GraphComponent.tsx";
import DynamicTabs from "../../common/DynamicTabs.tsx";

import Backtest from "./Backtest.tsx";
import PositionOpened from "./PositionOpened.tsx";
import PositionClosed from "./PositionClosed.tsx";
import ResultDisplay from "./ResultDisplay.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const StrategyDetails: React.FC = () => {
  const { strategyId } = useParams();
  const navigate = useNavigate();
  const [strategyInfo, setStrategyInfo] = useState<StrategyInfo>();

  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);
    if (strategyId != null) {
      StrategyService.getStrategyInfo(strategyId)
        .then((rsp) => setStrategyInfo(rsp))
        .catch(handleError)
        .finally(() => {
          setIsLoading(false);
        });
    }
  }, [instance, strategyId]);

  const deleteStrategy = () => {
    setIsLoading(true);
    if (strategyId != null) {
      StrategyService.closeStrategy(strategyId)
        .then(() => navigate("/Home"))
        .catch(handleError)
        .finally(() => {
          setIsLoading(false);
        });
    }
  };

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (!strategyInfo || !strategyId) {
    return <div>Aucune information de stratégie disponible.</div>;
  }

  return (
    <div>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <button
          className="btn btn-secondary"
          onClick={() => navigate("/Home")}
          style={{ alignSelf: "start" }}
        >
          Retour
        </button>
        <button
          className="btn btn-danger"
          onClick={deleteStrategy}
          style={{ alignSelf: "start" }}
        >
          Supprimer
        </button>
      </div>
      <div>
        <StrategyDataInfo strategyInfo={strategyInfo} />
      </div>
      <div>
        <GraphComponent />
      </div>
      <div>
        <DynamicTabs>
          <DynamicTabs.TabPanel title="Positions ouvertes">
            <PositionOpened strategyId={strategyId} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Positions closes">
            <PositionClosed strategyId={strategyId} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Result">
            <ResultDisplay strategyId={strategyId} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Backtest">
            <Backtest strategyId={strategyId} />
          </DynamicTabs.TabPanel>
        </DynamicTabs>
      </div>
    </div>
  );
};

export default StrategyDetails;
