export interface Position {
  id?: string;
  symbol?: string;
  typePosition?: string;
  spread?: number;

  profit?: number;
  openPrice?: number;
  dateOpen: Date;
  closePrice?: number;
  dateClose?: Date;
  reasonClosed?: string;

  stopLoss?: number;
  takeProfit?: number;

  volume?: number;
  pips?: number;
  statusPosition?: string;
  comment: string;

  positionState: string;
}
