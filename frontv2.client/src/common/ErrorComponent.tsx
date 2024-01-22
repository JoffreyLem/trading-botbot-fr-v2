import React from "react";

interface ErrorComponentProps {
  errors: string | string[];
  title?: string;
}

const ErrorComponent: React.FC<ErrorComponentProps> = ({
  errors,
  title = "Erreur",
}) => {
  const errorMessages = Array.isArray(errors) ? errors : [errors];

  return (
    <div className="alert alert-danger">
      <strong>{title}</strong>
      {errorMessages.map((error, index) => (
        <p key={index}>{error}</p>
      ))}
    </div>
  );
};

export default ErrorComponent;
