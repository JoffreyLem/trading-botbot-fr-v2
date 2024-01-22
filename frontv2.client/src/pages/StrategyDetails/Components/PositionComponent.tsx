import React, { CSSProperties } from "react";
import { FixedSizeList as List } from "react-window";

import "./PositionComponent.css";
import { Position } from "../../../modeles/Position.ts";

const ROW_HEIGHT = 50;
const PositionComponent: React.FC<{
  positions: Position[];
  positionClosed: boolean;
}> = ({ positions, positionClosed }) => {
  interface RowProps {
    index: number;
    style: CSSProperties;
  }

  const Row: React.FC<RowProps> = ({ index, style }) => {
    const position = positions[index];
    return (
      <div style={style} className="table-row">
        <div className="table-cell">{position?.symbol}</div>
        <div className="table-cell">{position?.typePosition}</div>
        <div className="table-cell">{position?.volume}</div>
        <div className="table-cell">{position?.spread}</div>
        <div className="table-cell">{position?.dateOpen.toString()}</div>
        <div className="table-cell">{position?.openPrice}</div>
        <div className="table-cell">{position?.profit}</div>
        <div className="table-cell">{position?.stopLoss}</div>
        <div className="table-cell">{position?.takeProfit}</div>
        {positionClosed && (
          <>
            <div className="table-cell">{position?.dateClose?.toString()}</div>
            <div className="table-cell">{position?.closePrice}</div>
            <div className="table-cell">{position?.reasonClosed}</div>
          </>
        )}
      </div>
    );
  };

  return (
    <div className="virtualized-table">
      <div className="table-header">
        <div className="table-row">
          <div className="table-cell">Symbol</div>
          <div className="table-cell">Type</div>
          <div className="table-cell">volume</div>
          <div className="table-cell">Spread</div>
          <div className="table-cell">Date open</div>
          <div className="table-cell">Open price</div>
          <div className="table-cell">Profit</div>
          <div className="table-cell">Stop loss</div>
          <div className="table-cell">Take profit</div>
          {positionClosed && (
            <>
              <div className="table-cell">Date close</div>
              <div className="table-cell">Close price</div>
              <div className="table-cell">Reason closed</div>
            </>
          )}
        </div>
      </div>
      <List
        height={400}
        itemCount={positions.length}
        itemSize={ROW_HEIGHT}
        width="100%"
      >
        {Row}
      </List>
    </div>
  );
};

export default PositionComponent;
