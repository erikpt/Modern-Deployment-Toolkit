import React from 'react';
import { TaskSequenceVariable } from '../../models/TaskSequence';
import './VariablesEditor.css';

interface VariablesEditorProps {
  variables: TaskSequenceVariable[];
  onVariablesChange: (variables: TaskSequenceVariable[]) => void;
}

const VariablesEditor: React.FC<VariablesEditorProps> = ({ variables, onVariablesChange }) => {
  const handleVariableChange = (index: number, field: keyof TaskSequenceVariable, value: any) => {
    const newVariables = [...variables];
    newVariables[index] = { ...newVariables[index], [field]: value };
    onVariablesChange(newVariables);
  };

  const handleAddVariable = () => {
    const newVariable: TaskSequenceVariable = {
      name: '',
      value: '',
      isReadOnly: false,
      isSecret: false
    };
    onVariablesChange([...variables, newVariable]);
  };

  const handleDeleteVariable = (index: number) => {
    const newVariables = variables.filter((_, i) => i !== index);
    onVariablesChange(newVariables);
  };

  return (
    <div className="variables-editor">
      <h4>Variables</h4>
      <div className="variables-list">
        {variables.map((variable, index) => (
          <div key={index} className="variable-item">
            <input
              type="text"
              value={variable.name}
              onChange={(e) => handleVariableChange(index, 'name', e.target.value)}
              placeholder="Variable name"
              className="var-name"
            />
            <input
              type={variable.isSecret ? 'password' : 'text'}
              value={variable.value}
              onChange={(e) => handleVariableChange(index, 'value', e.target.value)}
              placeholder="Value"
              className="var-value"
            />
            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={variable.isReadOnly}
                onChange={(e) => handleVariableChange(index, 'isReadOnly', e.target.checked)}
              />
              RO
            </label>
            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={variable.isSecret}
                onChange={(e) => handleVariableChange(index, 'isSecret', e.target.checked)}
              />
              Secret
            </label>
            <button onClick={() => handleDeleteVariable(index)} className="btn-delete-small">
              ✕
            </button>
          </div>
        ))}
      </div>
      <button onClick={handleAddVariable} className="btn btn-small">
        + Add Variable
      </button>
    </div>
  );
};

export default VariablesEditor;
