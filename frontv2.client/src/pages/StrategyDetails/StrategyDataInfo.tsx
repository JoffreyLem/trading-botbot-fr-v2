import React, { useEffect, useState } from "react";
import { StrategyInfo } from "../../modeles/StrategyInfo.ts";
import { strategyService } from "../../services/StrategyHandlerService.ts";
import { useMsal } from "@azure/msal-react";

interface StrategyFormProps {
  strategyInfo: StrategyInfo;
}

const StrategyForm: React.FC<StrategyFormProps> = ({ strategyInfo }) => {
  const [formData, setFormData] = useState<StrategyInfo>(strategyInfo);
  const { instance } = useMsal();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setFormData(strategyInfo);
  }, [strategyInfo]);

  const handleCanRunChange = () => {
    strategyService
      .setCanRun(instance, strategyInfo.id, !formData.canRun)
      .then(() => {
        setFormData({ ...formData, canRun: !formData.canRun });
      })
      .catch((err) => setError(err.message));
  };

  if (error) {
    return <div>Erreur: {error}</div>;
  }

  return (
    <form>
      <div className="form-group">
        <label htmlFor="symbol">Symbol</label>
        <input
          type="text"
          className="form-control"
          id="symbol"
          value={formData.symbol}
          readOnly
        />
      </div>

      <div className="form-group">
        <label htmlFor="timeframe">Timeframe</label>
        <input
          type="text"
          className="form-control"
          id="timeframe"
          value={formData.timeframe}
          readOnly
        />
      </div>

      <div className="form-group">
        <label htmlFor="timeframe2">Timeframe 2</label>
        <input
          type="text"
          className="form-control"
          id="timeframe2"
          value={formData.timeframe2}
          readOnly
        />
      </div>

      <div className="form-group">
        <label htmlFor="strategyName">Strategy Name</label>
        <input
          type="text"
          className="form-control"
          id="strategyName"
          value={formData.strategyName}
          readOnly
        />
      </div>

      <div className="form-group form-check form-switch">
        <label className="form-check-label" htmlFor="canRun">
          Can Run
        </label>
        <input
          type="checkbox"
          className="form-check-input"
          id="canRun"
          checked={formData.canRun}
          onChange={handleCanRunChange}
        />
      </div>
    </form>
  );
};

export default StrategyForm;
