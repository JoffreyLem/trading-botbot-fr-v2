import React, { useContext, useEffect, useState } from "react";
import { StrategyInit } from "../../modeles/StrategyInit.ts";
import { StrategyFile } from "../../modeles/StrategyFile.ts";

import styles from "./css/Form.module.css";

import AutocompleteInput from "../../common/AutoCompleteInputProps.tsx";
import { SymbolInfo } from "../../modeles/SymbolInfo.ts";

import LoadSpinner from "../../common/LoadSpinner.tsx";
import { StrategyContext } from "./StrategyProvider.tsx";

import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { ApiHandlerService } from "../../services/ApiHandlerService.ts";
import { StrategyService } from "../../services/StrategyHandlerService.ts";

const StrategyForm: React.FC = () => {
  const [strategyInitDto, setStrategyInitDto] = useState<StrategyInit>({
    strategyFileId: "",
    symbol: "",
    timeframe: "",
    timeframe2: "",
  });

  const [allStrategy, setAllStrategy] = useState<StrategyFile[]>([]);
  const [allSymbol, setAllSymbol] = useState<SymbolInfo[]>([]);
  const [timeframes, setAllTimeframes] = useState<string[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const { handleRefresh } = useContext(StrategyContext);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);

    Promise.all([
      StrategyGeneratorService.getAllStrategyFiles()
        .then((response) => setAllStrategy(response))
        .catch(handleError),
      ApiHandlerService.getAllSymbol()
        .then((response) => setAllSymbol(response))
        .catch(handleError),
      StrategyService.getListTimeframes()
        .then((response) => setAllTimeframes(response))
        .catch(handleError),
    ]).finally(() => {
      setIsLoading(false);
    });
  }, []);

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    StrategyService.initStrategy(strategyInitDto)
      .catch(handleError)
      .finally(() => {
        setIsLoading(false);
        handleRefresh();
      });
  };

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = event.target;

    setStrategyInitDto((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  };

  const handleValueChange = (value: string) => {
    setStrategyInitDto((prevState) => ({
      ...prevState,
      symbol: value,
    }));
  };

  const symbols: string[] = allSymbol
    .map((symbolInfo) => symbolInfo.symbol)
    .filter((symbol) => symbol !== undefined) as string[];

  if (isLoading) {
    return <LoadSpinner />;
  }

  return (
    <div style={{ display: "flex" }}>
      <div style={{ flex: 1, paddingRight: "10px" }}>
        <form onSubmit={handleSubmit}>
          <div className={styles.formGroup}>
            <label htmlFor="strategyName">Strategy</label>
            <select
              id="strategyFileId"
              name="strategyFileId"
              onChange={handleChange}
              value={strategyInitDto.strategyFileId}
              className={styles.formControl}
              aria-label="Select Strategy"
            >
              <option value="">Sélectionnez une option</option>
              {allStrategy.map((option, index) => (
                <option key={index} value={option.id}>
                  {option.name}
                </option>
              ))}
            </select>
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="symbol">Symbol:</label>
            <AutocompleteInput
              suggestions={symbols}
              onValueChange={handleValueChange}
            />
          </div>
          <div className={styles.formGroup}>
            <label htmlFor="timeframe">Timeframe:</label>
            <select
              id="timeframe"
              name="timeframe"
              value={strategyInitDto.timeframe}
              onChange={handleChange}
              className={styles.formControl}
            >
              {timeframes.map((timeframe, index) => (
                <option key={index} value={timeframe}>
                  {timeframe}
                </option>
              ))}
            </select>
          </div>
          <div className={styles.formGroup}>
            <label htmlFor="timeframe2">Timeframe 2:</label>
            <select
              id="timeframe2"
              name="timeframe2"
              value={strategyInitDto.timeframe2}
              onChange={handleChange}
              className={styles.formControl}
            >
              {timeframes.map((timeframe, index) => (
                <option key={index} value={timeframe}>
                  {timeframe}
                </option>
              ))}
            </select>
          </div>
          <div className={styles.formGroup}>
            <button type="submit" className={styles.submitButton}>
              Submit
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
export default StrategyForm;
