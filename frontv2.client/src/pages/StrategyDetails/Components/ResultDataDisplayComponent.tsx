import React from "react";

import { Result } from "../../../modeles/Result.ts";

const resultDisplayComponent: React.FC<{
  result: Result | undefined;
}> = ({ result }) => {
  if (!result) {
    return <div className="alert alert-warning">Aucune donnée disponible</div>;
  }

  return (
    <div className="container mt-4">
      <form>
        <div className="mb-3">
          <label htmlFor="drawndownMax" className="form-label">
            Drawndown Max
          </label>
          <input
            type="text"
            className="form-control"
            id="drawndownMax"
            value={result.drawndownMax}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="gainMax" className="form-label">
            Gain Max
          </label>
          <input
            type="text"
            className="form-control"
            id="gainMax"
            value={result.gainMax}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="moyenneNegative" className="form-label">
            Moyenne Negative
          </label>
          <input
            type="text"
            className="form-control"
            id="moyenneNegative"
            value={result.moyenneNegative}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="moyennePositive" className="form-label">
            Moyenne Positive
          </label>
          <input
            type="text"
            className="form-control"
            id="moyennePositive"
            value={result.moyennePositive}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="moyenneProfit" className="form-label">
            Moyenne Profit
          </label>
          <input
            type="text"
            className="form-control"
            id="moyenneProfit"
            value={result.moyenneProfit}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="perteMax" className="form-label">
            Perte Max
          </label>
          <input
            type="text"
            className="form-control"
            id="perteMax"
            value={result.perteMax}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="profit" className="form-label">
            Profit
          </label>
          <input
            type="text"
            className="form-control"
            id="profit"
            value={result.profit}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="profitFactor" className="form-label">
            Profit Factor
          </label>
          <input
            type="text"
            className="form-control"
            id="profitFactor"
            value={result.profitFactor}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="profitNegatif" className="form-label">
            Profit Negatif
          </label>
          <input
            type="text"
            className="form-control"
            id="profitNegatif"
            value={result.profitNegatif}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="profitPositif" className="form-label">
            Profit Positif
          </label>
          <input
            type="text"
            className="form-control"
            id="profitPositif"
            value={result.profitPositif}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="ratioMoyennePositifNegatif" className="form-label">
            Ratio Moyenne Positif/Négatif
          </label>
          <input
            type="text"
            className="form-control"
            id="ratioMoyennePositifNegatif"
            value={result.ratioMoyennePositifNegatif}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="tauxReussite" className="form-label">
            Taux de Réussite
          </label>
          <input
            type="text"
            className="form-control"
            id="tauxReussite"
            value={result.tauxReussite}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="totalPositionNegative" className="form-label">
            Total Position Négative
          </label>
          <input
            type="text"
            className="form-control"
            id="totalPositionNegative"
            value={result.totalPositionNegative}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="totalPositionPositive" className="form-label">
            Total Position Positive
          </label>
          <input
            type="text"
            className="form-control"
            id="totalPositionPositive"
            value={result.totalPositionPositive}
            readOnly
          />
        </div>

        <div className="mb-3">
          <label htmlFor="totalPositions" className="form-label">
            Total Positions
          </label>
          <input
            type="text"
            className="form-control"
            id="totalPositions"
            value={result.totalPositions}
            readOnly
          />
        </div>
      </form>
    </div>
  );
};

export default resultDisplayComponent;
