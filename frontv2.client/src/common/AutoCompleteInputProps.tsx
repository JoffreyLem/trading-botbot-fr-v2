import React, { useState } from "react";
import "./css/AutoCompleteInput.css";

interface AutocompleteInputProps {
  suggestions: string[];
  onValueChange?: (value: string) => void;
}

const AutocompleteInput: React.FC<AutocompleteInputProps> = ({
  suggestions,
  onValueChange,
}) => {
  const [inputValue, setInputValue] = useState<string>("");
  const [filteredSuggestions, setFilteredSuggestions] = useState<string[]>([]);
  const [showSuggestions, setShowSuggestions] = useState<boolean>(false);

  const updateSuggestions = (value: string) => {
    setInputValue(value);
    onValueChange && onValueChange(value);
    if (!value) {
      setFilteredSuggestions([]);
      setShowSuggestions(false);
      return;
    }
    const filtered = suggestions.filter((suggestion) =>
      suggestion.toLowerCase().includes(value.toLowerCase()),
    );
    setFilteredSuggestions(filtered);
    setShowSuggestions(true);
  };

  const selectSuggestion = (suggestion: string) => {
    setInputValue(suggestion);
    onValueChange && onValueChange(suggestion);
    setFilteredSuggestions([]);
    setShowSuggestions(false);
  };

  return (
    <div className="autocomplete-container">
      <input
        className="autocomplete-input"
        type="text"
        value={inputValue}
        onChange={(e) => updateSuggestions(e.target.value)}
      />
      {showSuggestions && (
        <ul className="autocomplete-suggestions">
          {filteredSuggestions.map((suggestion, index) => (
            <li key={index} onClick={() => selectSuggestion(suggestion)}>
              {suggestion}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default AutocompleteInput;
