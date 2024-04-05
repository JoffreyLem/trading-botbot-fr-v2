import React, { useEffect, useState } from "react";
import { StrategyFile } from "../../modeles/StrategyFile.ts";
import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";

import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const StrategyCreator: React.FC = () => {
  const [strategyFiles, setStrategyFiles] = useState<StrategyFile[]>([]);

  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);
    StrategyGeneratorService.getAllStrategyFiles()
      .then((r) => setStrategyFiles(r))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [instance]);

  const handleUpdate = (strategyId: number) => {
    window.open(`vscode://botbot.botbot-ext?id=${strategyId}`);
  };

  const handleDelete = (id: number) => {
    StrategyGeneratorService.deleteStrategyFile(id)
      .then(() =>
        setStrategyFiles(strategyFiles.filter((file) => file.id !== id)),
      )
      .catch(handleError);
  };

  const handleCreate = () => {
    window.open("vscode://botbot.botbot-ext");
  };

  if (isLoading) {
    return <LoadSpinner />;
  }

  return (
    <div>
      <button className="btn btn-success mb-3" onClick={handleCreate}>
        Créer
      </button>
      <table className="table table-striped">
        <thead>
          <tr>
            <th>Name</th>
            <th>Version</th>
            <th>Last Update</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {strategyFiles.map((file) => (
            <tr key={file.id}>
              <td>{file.name}</td>
              <td>{file.version}</td>
              <td>{file.lastDateUpdate?.toString()}</td>
              <td>
                <button
                  className="btn btn-primary me-2"
                  onClick={() => handleUpdate(file.id)}
                >
                  Update
                </button>
                <button
                  className="btn btn-danger"
                  onClick={() => handleDelete(file.id)}
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default StrategyCreator;
