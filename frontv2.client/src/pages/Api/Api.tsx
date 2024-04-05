import React, { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import LoadSpinner from "../../common/LoadSpinner.tsx";
import { ConnectDto } from "../../modeles/Connect.ts";

import { ApiHandlerService } from "../../services/ApiHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

const Api: React.FC = () => {
  const [isConnected, setIsConected] = useState<boolean>(false);
  const [apiHandlerList, setApihandlerList] = useState<string[]>([]);

  const [defaultApiHandlerSelected, setDefaultApiHandlerselected] =
    useState<string>();
  const [connectDto, setConnectDto] = useState<ConnectDto>({
    user: "",
    pwd: "",
    handlerEnum: "",
  });
  const navigate = useNavigate();
  const handleError = useErrorHandler();

  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsLoading(true);

    const fetchApiHandlerList = ApiHandlerService.getListHandler()
      .then((r) => setApihandlerList(r))
      .catch(handleError);

    const checkIsConnected = ApiHandlerService.isConnected()
      .then((response) => setIsConected(response))
      .catch(handleError);

    const fetchTypeHandler = isConnected
      ? ApiHandlerService.getTypeHandler()
          .then((response) => setDefaultApiHandlerselected(response))
          .catch(handleError)
      : Promise.resolve();

    Promise.all([
      fetchApiHandlerList,
      checkIsConnected,
      fetchTypeHandler,
    ]).finally(() => setIsLoading(false));
  }, [isConnected, setApihandlerList]);

  const handleSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setConnectDto((prevConnectDto) => ({
      ...prevConnectDto,
      handlerEnum: e.target.value,
    }));
    setDefaultApiHandlerselected(e.target.value);
  };

  const handleDisconnect = () => {
    setIsLoading(true);
    ApiHandlerService.disconnect()
      .then(() => setIsConected(false))
      .catch(handleError);
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
    ApiHandlerService.connect(connectDto)
      .then(() => {
        setIsConected(true);
        navigate("/home");
      })
      .catch(handleError)
      .finally(() => {
        {
          setIsLoading(false);
        }
      });
  };

  if (isLoading) {
    return <LoadSpinner />;
  }
  return (
    <div className="row">
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
