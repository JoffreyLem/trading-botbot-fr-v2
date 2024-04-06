import React, { useEffect, useState } from "react";
import { StrategyFile } from "../../modeles/StrategyFile.ts";
import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";

import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import CreateUpdateModalStrategy from "./CreateUpdateModalStrategy.tsx";
import ErrorComponent from "../../common/ErrorComponent.tsx";

const StrategyCreator: React.FC = () => {
  const [strategyFiles, setStrategyFiles] = useState<StrategyFile[]>([]);
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  const [modalShow, setModalShow] = useState(false);
  const [compilError, setCompilError] = useState<string[]>();
  const [strategyIdUpdate, setStrategyIdupdate] = useState<number | null>(null);

  useEffect(() => {
    setIsLoading(true);
    StrategyGeneratorService.getAllStrategyFiles()
      .then((r) => setStrategyFiles(r))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [instance]);

  const handleUpdate = (strategyId: number) => {
    setStrategyIdupdate(strategyId);
    setModalShow(true);
  };

  const handleDelete = (id: number) => {
    StrategyGeneratorService.deleteStrategyFile(id)
      .then(() =>
        setStrategyFiles(strategyFiles.filter((file) => file.id !== id)),
      )
      .catch(handleError);
  };

  const handleCreate = () => {
    setModalShow(true);
  };

  const handleSubmitModal = (file: File) => {
    setIsLoading(true);
    if (strategyIdUpdate === null) {
      StrategyGeneratorService.createNewStrategy(file)
        .then((r) => {
          if (r.compiled) {
            addnewStrategy(r.strategyFileDto as StrategyFile);
          } else {
            setCompilError(r.errors as string[]);
          }
        })
        .catch(handleError);
    } else {
      StrategyGeneratorService.updateStrategyFile(strategyIdUpdate, file)
        .then((r) => {
          if (r.compiled) {
            updateStrategyFile(r.strategyFileDto as StrategyFile);
          } else {
            setCompilError(r.errors as string[]);
          }
        })
        .catch(handleError)
        .finally(() => {
          setStrategyIdupdate(null);
        });
    }
    setIsLoading(false);
  };

  const addnewStrategy = (strategy: StrategyFile) => {
    setStrategyFiles((currentStrategyFiles) => [
      ...currentStrategyFiles,
      strategy,
    ]);
  };

  const updateStrategyFile = (strategy: StrategyFile) => {
    setStrategyFiles((currentFiles) =>
      currentFiles.map((file) => {
        if (file.id === strategy.id) {
          return { ...strategy };
        }

        return file;
      }),
    );
  };

  const downloadFile = (file: StrategyFile) => {
    const blob = new Blob([file.data], { type: "text/plain" });

    const fileUrl = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = fileUrl;
    link.download = file.name;

    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);
    URL.revokeObjectURL(fileUrl);
  };

  if (isLoading) {
    return <LoadSpinner />;
  }

  return (
    <div>
      <button className="btn btn-success mb-3" onClick={handleCreate}>
        Créer
      </button>
      <CreateUpdateModalStrategy
        show={modalShow}
        onClose={() => setModalShow(false)}
        handleSubmit={handleSubmitModal}
      />
      {compilError && (
        <ErrorComponent title="Erreur de compilation" errors={compilError} />
      )}
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
                <button
                  className="btn btn-secondary"
                  onClick={() => downloadFile(file)}
                >
                  Télécharger
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
