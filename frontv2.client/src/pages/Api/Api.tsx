import React, { useEffect, useState } from "react";

import { useMsal } from "@azure/msal-react";

import { useNavigate } from "react-router-dom";
import { apiHandlerService } from "../../services/ApiHandlerService.ts";
import LoadSpinner from "../../common/LoadSpinner.tsx";
import { ConnectDto } from "../../modeles/Connect.ts";
import { ApiError } from "../../modeles/ApiError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";

const Api: React.FC = () => {
  const [isConnected, setIsConected] = useState<boolean>(false);
  const [apiHandlerList, setApihandlerList] = useState<string[]>([]);
  const [error, setError] = useState<string>("");
  const [connectionError, setConnectionError] = useState<ApiError>();
  const { instance } = useMsal();
  const [defaultApiHandlerSelected, setDefaultApiHandlerselected] =
    useState<string>();
  const [connectDto, setConnectDto] = useState<ConnectDto>({
    user: "",
    pwd: "",
    handlerEnum: "",
  });
  const navigate = useNavigate();

  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsLoading(true);

    const fetchApiHandlerList = apiHandlerService
      .getListHandler(instance)
      .then((r) => setApihandlerList(r))
      .catch((err) => setError(err.message));

    const checkIsConnected = apiHandlerService
      .isConnected(instance)
      .then((response) => setIsConected(response))
      .catch((err) => setError(err.message));

    const fetchTypeHandler = isConnected
      ? apiHandlerService
          .getTypeHandler(instance)
          .then((response) => setDefaultApiHandlerselected(response))
          .catch((err) => setError(err.message))
      : Promise.resolve();

    Promise.all([
      fetchApiHandlerList,
      checkIsConnected,
      fetchTypeHandler,
    ]).finally(() => setIsLoading(false));
  }, [instance, isConnected, setApihandlerList]);

  const handleSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setConnectDto((prevConnectDto) => ({
      ...prevConnectDto,
      handlerEnum: e.target.value,
    }));
    setDefaultApiHandlerselected(e.target.value);
  };

  const handleDisconnect = () => {
    setIsLoading(true);
    apiHandlerService
      .disconnect(instance)
      .then(() => setIsConected(false))
      .catch((err) => setError(err.message));
    setIsLoading(false);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setConnectDto((prevConnectDto) => ({
      ...prevConnectDto,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);
    apiHandlerService
      .connect(instance, connectDto)
      .then(() => {
        setIsConected(true);
        navigate("/home");
      })
      .catch((err: ApiError) => {
        setConnectionError(err);
        setIsConected(false);
      })
      .finally(() => {
        {
          setIsLoading(false);
        }
      });
  };

  if (error) {
    return <div>Erreur: {error}</div>;
  }
  if (isLoading) {
    return <LoadSpinner />;
  }
  return (
    <div className="row">
      {connectionError && (
        <ErrorComponent
          title="Erreur de connexion"
          errors={connectionError.errors}
        />
      )}
      <div className="col-md-4">
        <select
          value={defaultApiHandlerSelected}
          onChange={handleSelect}
          disabled={isConnected}
          className="form-control"
        >
          <option value="">Sélectionnez une option</option>
          {apiHandlerList.map((option, index) => (
            <option key={index} value={option}>
              {option}
            </option>
          ))}
        </select>
      </div>
      {isConnected ? (
        <div className="mt-3">
          <button className="btn btn-primary" onClick={handleDisconnect}>
            Disconnect
          </button>
        </div>
      ) : (
        <div>
          <div className="col-md-4">
            <form onSubmit={handleSubmit} className="mt-3">
              <div className="form-group">
                <label htmlFor="first-name">User</label>
                <input
                  type="text"
                  className="form-control"
                  id="first-name"
                  name="user"
                  value={connectDto?.user}
                  onChange={handleInputChange}
                />
              </div>
              <div className="form-group">
                <label htmlFor="last-name">Password</label>
                <input
                  type="password"
                  className="form-control"
                  id="last-name"
                  name="pwd"
                  value={connectDto?.pwd}
                  onChange={handleInputChange}
                />
              </div>
              <div className="mt-3">
                <button type="submit" className="btn btn-primary">
                  Submit
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Api;
