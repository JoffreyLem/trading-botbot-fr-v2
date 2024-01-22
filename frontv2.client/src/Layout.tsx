import { Navigate, NavLink, Route, Routes } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";
import { useMsal } from "@azure/msal-react";

import Home from "./pages/Home/Home.tsx";
import StrategyDetails from "./pages/StrategyDetails/StrategyDetails.tsx";
import Api from "./pages/Api/Api.tsx";

import StrategyCreator from "./pages/StrategyCreator/StrategyCreator.tsx";
import CodeEditor from "./pages/StrategyCreator/CodeEditor.tsx";

const NotFound = () => {
  return (
    <div>
      <h2>Page non trouvée</h2>
      <p>Désolé, la page que vous cherchez n'existe pas.</p>
    </div>
  );
};
const Layout = () => {
  const { instance } = useMsal();

  const handleLogout = () => {
    instance.logout();
  };

  return (
    <div className="d-flex flex-column vh-100">
      <nav className="navbar navbar-expand-lg navbar-light bg-light">
        <div className="container-fluid">
          <span className="navbar-brand mb-0 h1">BOT Bot</span>
          <div className="ms-auto">
            <button onClick={handleLogout} className="btn btn-outline-danger">
              Logout
            </button>
          </div>
        </div>
      </nav>

      <div className="d-flex flex-grow-1">
        <div
          className="d-flex flex-column flex-shrink-0 p-3 bg-light"
          style={{ width: "200px" }}
        >
          <nav className="nav flex-column">
            <NavLink
              to="/home"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Home
            </NavLink>
            <NavLink
              to="/api"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              API
            </NavLink>
            <NavLink
              to="/strategy-list"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Strategy Creator
            </NavLink>
          </nav>
        </div>

        <div className="main-content p-4 flex-grow-1">
          <Routes>
            <Route path="/" element={<Navigate replace to="/home" />} />
            <Route path="/home" element={<Home />} />
            <Route path="/api" element={<Api />} />
            <Route path="/strategy-list" element={<StrategyCreator />} />
            <Route
              path="/strategy-creator/:strategyId"
              element={<CodeEditor />}
            />
            <Route path="/strategy-creator" element={<CodeEditor />} />
            <Route path="/strategy/:strategyId" element={<StrategyDetails />} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
      </div>
    </div>
  );
};

export default Layout;
