import "./App.css";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from "@azure/msal-react";
import React, { useEffect, useState } from "react";
import Layout from "./Layout.tsx";

function App() {
  const { instance, accounts } = useMsal();
  const [isInteractionRequired, setIsInteractionRequired] = useState(false);

  useEffect(() => {
    if (accounts.length === 0 && !isInteractionRequired) {
      setIsInteractionRequired(true);
      instance
        .loginPopup()
        .then(() => {
          setIsInteractionRequired(false);
        })
        .catch((e) => {
          console.error(e);
          setIsInteractionRequired(false);
        });
    }
  }, [instance, accounts, isInteractionRequired]);
  return (
    <React.Fragment>
      <AuthenticatedTemplate>
        <Layout />
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <p>Loading...</p>
      </UnauthenticatedTemplate>
    </React.Fragment>
  );
}

export default App;
