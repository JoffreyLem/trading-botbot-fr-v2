import React, { useState } from "react";
import StrategyCreator from "./StrategyCreator.tsx";
import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";

const CreateModalStrategy: React.FC<{ show: boolean; onClose: () => void }> = ({
  show,
  onClose,
}) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files[0]) {
      setSelectedFile(event.target.files[0]);
    } else {
      setSelectedFile(null);
    }
  };

  const handleFileDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    if (event.dataTransfer.files && event.dataTransfer.files[0]) {
      setSelectedFile(event.dataTransfer.files[0]);
    } else {
      setSelectedFile(null);
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    await StrategyGeneratorService.createNewStrategy(selectedFile);

    onClose();
  };

  if (!show) {
    return null;
  }

  return (
    <div className="modal show" style={{ display: "block" }} role="dialog">
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Upload File</h5>
            <button type="button" className="close" onClick={onClose}>
              <span>&times;</span>
            </button>
          </div>
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div
                onDragOver={(e) => e.preventDefault()}
                onDrop={handleFileDrop}
                className="dropzone text-center p-3 my-2 border-dashed border-2"
              >
                Drag and drop a file here, or click to select a file.
                <input
                  type="file"
                  className="form-control-file mt-3"
                  onChange={handleFileChange}
                  style={{ display: "none" }}
                  id="fileUpload"
                />
                <label htmlFor="fileUpload" className="btn btn-primary mt-2">
                  Select File
                </label>
              </div>
              {selectedFile && <p>Selected file: {selectedFile.name}</p>}
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
              >
                Close
              </button>
              <button type="submit" className="btn btn-primary">
                Upload
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default CreateModalStrategy;
