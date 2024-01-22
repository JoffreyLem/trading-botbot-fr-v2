export interface SymbolInfo {
  category: Category;
  symbol?: string;
}

// Category.ts
export enum Category {
  Forex,
  Indices,
  Stock,
  Commodities,
  Unknow,
  Crypto,
  ExchangeTradedFund,
}
