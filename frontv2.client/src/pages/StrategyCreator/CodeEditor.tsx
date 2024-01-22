import React, { useEffect, useRef, useState } from "react";
import ace from "ace-builds";

// Importez les ressources nécessaires pour Ace
import "ace-builds/src-noconflict/mode-csharp";
import "ace-builds/src-noconflict/theme-monokai";
import "ace-builds/src-noconflict/ext-language_tools";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { StrategyFile } from "../../modeles/StrategyFile.ts";
import { strategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useMsal } from "@azure/msal-react";
import LoadSpinner from "../../common/LoadSpinner.tsx";
import { ApiError } from "../../modeles/ApiError.ts";
import ErrorComponent from "../../common/ErrorComponent.tsx";

const CodeEditor: React.FC = () => {
  const editorRef = useRef<HTMLDivElement>(null);
  const [strategyFile, setStrategyFile] = useState<StrategyFile>();
  const [editor, setEditor] = useState<ace.Ace.Editor>();
  const { strategyId } = useParams();
  const navigate = useNavigate();
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const [searchParams] = useSearchParams();
  const isForCreate = searchParams.get("isForCreate");
  const [actionError, setActionError] = useState<ApiError>();

  useEffect(() => {
    if (strategyId) {
      setIsLoading(true);
      strategyGeneratorService
        .getStrategyFile(instance, Number(strategyId))
        .then((r) => {
          setStrategyFile(r);
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
  }, [instance, strategyId]);

  useEffect(() => {
    if (editorRef.current) {
      // Initialiser Ace Editor
      const aceEditor = ace.edit(editorRef.current);
      aceEditor.setTheme("ace/theme/monokai");
      aceEditor.session.setMode("ace/mode/csharp");
      aceEditor.setOptions({
        enableBasicAutocompletion: true,
        enableSnippets: true,
        enableLiveAutocompletion: true,
      });

      setEditor(aceEditor);
    }
  }, []);

  useEffect(() => {
    if (editor && strategyFile && strategyFile.data) {
      const langTools = ace.require("ace/ext/language_tools");
      const customCompleter = {
        getCompletions: function (
          _editor: never,
          _session: never,
          _pos: never,
          _prefix: never,
          callback: (
            arg0: null,
            arg1: { caption: string; value: string; meta: string }[],
          ) => void,
        ) {
          const customCompletions = [
            {
              caption: "Console.WriteLine",
              value: "Console.WriteLine()",
              meta: "method",
            },
            {
              caption: "Console.ReadLine",
              value: "Console.ReadLine()",
              meta: "method",
            },
          ];
          callback(null, customCompletions);
        },
      };
      langTools.addCompleter(customCompleter);

      const decodedData = atob(strategyFile.data);
      editor.setValue(decodedData);
    }
  }, [editor, strategyFile]);

  const handleSave = () => {
    if (editor) {
      const base64Data = btoa(editor.getValue());
      if (isForCreate === "false") {
        setIsLoading(true);

        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-expect-error
        const updatedStrategyFile: StrategyFile = {
          ...strategyFile,
          data: base64Data,
        };
        strategyGeneratorService
          .updateStrategyFile(instance, updatedStrategyFile)
          .then(() => navigate(`/strategy-list`))
          .catch((err: ApiError) => setActionError(err))
          .finally(() => setIsLoading(false));
      } else {
        setIsLoading(true);
        strategyGeneratorService
          .createNewStrategy(instance, base64Data)
          .then(() => navigate(`/strategy-list`))
          .catch((err: ApiError) => setActionError(err))
          .finally(() => setIsLoading(false));
      }
    }
  };

  return (
    <div>
      {isLoading && <LoadSpinner />}

      <div className="d-flex justify-content-between mb-3">
        <button
          className="btn btn-secondary"
          onClick={() => navigate("/strategy-list")}
        >
          Retour
        </button>
        <button className="btn btn-primary" onClick={handleSave}>
          Sauvegarder
        </button>
      </div>

      <div ref={editorRef} style={{ height: "1000px", width: "100%" }} />
      <div>
        {actionError && (
          <ErrorComponent
            title="Erreur de sauvegarde"
            errors={actionError.errors}
          />
        )}
      </div>
    </div>
  );
};

export default CodeEditor;
