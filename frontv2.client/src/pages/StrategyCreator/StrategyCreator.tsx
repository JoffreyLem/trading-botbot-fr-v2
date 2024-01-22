import React, { useEffect, useState } from "react";
import { StrategyFile } from "../../modeles/StrategyFile.ts";
import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";

import { strategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useNavigate } from "react-router-dom";
import { ApiError } from "../../modeles/ApiError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";

const StrategyCreator: React.FC = () => {
  const [strategyFiles, setStrategyFiles] = useState<StrategyFile[]>([]);
  const navigate = useNavigate();
  const [actionError, setActionError] = useState<ApiError>();
  const [error, setError] = useState<string>("");
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsLoading(true);
    strategyGeneratorService
      .getAllStrategyFiles(instance)
      .then((r) => setStrategyFiles(r))
      .catch((err) => setError(err))
      .finally(() => setIsLoading(false));
  }, [instance]);

  const handleUpdate = (strategyId: number) => {
    navigate(`/strategy-creator/${strategyId}`);
  };

  const handleDelete = (id: number) => {
    strategyGeneratorService
      .deleteStrategyFile(instance, id)
      .then(() =>
        setStrategyFiles(strategyFiles.filter((file) => file.id !== id)),
      )
      .catch((error: ApiError) => setActionError(error));
  };

  const handleCreate = () => {
    navigate("/strategy-creator?isForCreate=true");
  };

  if (error) {
    return <div>Erreur: {error}</div>;
  }
  if (isLoading) {
    return <LoadSpinner />;
  }

  return (
    <div>
      {actionError && (
        <ErrorComponent
          title="Erreur de suppression"
          errors={actionError.errors}
        />
      )}
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
