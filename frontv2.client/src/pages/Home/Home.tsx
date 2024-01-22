// Home.tsx
import React, { useEffect, useState } from "react";

import { useMsal } from "@azure/msal-react";
import { apiHandlerService } from "../../services/ApiHandlerService.ts";
import StrategyForm from "./StrategyForm.tsx";
import StrategyList from "./StrategyList.tsx";
import { StrategyProvider } from "./StrategyProvider.tsx";
import LoadSpinner from "../../common/LoadSpinner.tsx";

const Home: React.FC = () => {
  const [isConnected, setIsConected] = useState<boolean | null>(null);
  const [error, setError] = useState<string | null>(null);
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  useEffect(() => {
    setIsLoading(true);
    apiHandlerService
      .isConnected(instance)
      .then((response) => setIsConected(response))
      .catch((err) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [instance]);

  if (error) {
    return <div>Erreur: {error}</div>;
  }

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
