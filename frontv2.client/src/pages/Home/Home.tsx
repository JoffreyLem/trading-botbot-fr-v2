// Home.tsx
import React, { useEffect, useState } from "react";

import StrategyForm from "./StrategyForm.tsx";
import StrategyList from "./StrategyList.tsx";
import { StrategyProvider } from "./StrategyProvider.tsx";
import LoadSpinner from "../../common/LoadSpinner.tsx";
import { ApiHandlerService } from "../../services/ApiHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const Home: React.FC = () => {
  const [isConnected, setIsConected] = useState<boolean | null>(null);

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  useEffect(() => {
    setIsLoading(true);
    ApiHandlerService.isConnected()
      .then((response) => setIsConected(response))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, []);

  if (isLoading) {
    return <LoadSpinner />;
  }

  if (!isConnected) {
    return <div>Connexion à une API nécessaire</div>;
  } else {
    return (
      <StrategyProvider>
        <div>
          <div>
            <StrategyForm />
          </div>
          <div>
            <StrategyList />
          </div>
        </div>
      </StrategyProvider>
    );
  }
};

export default Home;
