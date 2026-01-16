import React from 'react';
import { TaskSequenceStep, StepTypeMetadata, ConditionOperator } from '../../models/TaskSequence';
import './PropertiesPanel.css';

interface PropertiesPanelProps {
  step: TaskSequenceStep | null;
  stepTypes: StepTypeMetadata[];
  onStepUpdate: (step: TaskSequenceStep) => void;
}

const PropertiesPanel: React.FC<PropertiesPanelProps> = ({ step, stepTypes, onStepUpdate }) => {
  if (!step) {
    return (
      <div className="properties-panel">
        <h3>Properties</h3>
        <p className="empty-state">Select a step to edit its properties</p>
      </div>
    );
  }

  const stepTypeMetadata = stepTypes.find((st) => st.type === step.type);

  const handleChange = (field: keyof TaskSequenceStep, value: any) => {
    onStepUpdate({ ...step, [field]: value });
  };

  const handlePropertyChange = (key: string, value: string) => {
    const newProperties = { ...step.properties, [key]: value };
    onStepUpdate({ ...step, properties: newProperties });
  };

  const handleDeleteProperty = (key: string) => {
    const newProperties = { ...step.properties };
    delete newProperties[key];
    onStepUpdate({ ...step, properties: newProperties });
  };

  const handleAddProperty = () => {
    const key = prompt('Enter property name:');
    if (key && key.trim()) {
      handlePropertyChange(key.trim(), '');
    }
  };

  const handleAddCondition = () => {
    const newCondition = {
      variableName: '',
      operator: ConditionOperator.Equals,
      value: ''
    };
    onStepUpdate({
      ...step,
      conditions: [...step.conditions, newCondition]
    });
  };

  const handleConditionChange = (index: number, field: string, value: any) => {
    const newConditions = [...step.conditions];
    newConditions[index] = { ...newConditions[index], [field]: value };
    onStepUpdate({ ...step, conditions: newConditions });
  };

  const handleDeleteCondition = (index: number) => {
    const newConditions = step.conditions.filter((_, i) => i !== index);
    onStepUpdate({ ...step, conditions: newConditions });
  };

  return (
    <div className="properties-panel">
      <h3>Properties: {step.name || '(Unnamed)'}</h3>

      <div className="form-group">
        <label>Name</label>
        <input
          type="text"
          value={step.name}
          onChange={(e) => handleChange('name', e.target.value)}
          placeholder="Step name"
        />
      </div>

      <div className="form-group">
        <label>Description</label>
        <textarea
          value={step.description}
          onChange={(e) => handleChange('description', e.target.value)}
          placeholder="Step description"
          rows={3}
        />
      </div>

      <div className="form-group checkbox-group">
        <label>
          <input
            type="checkbox"
            checked={step.enabled}
            onChange={(e) => handleChange('enabled', e.target.checked)}
          />
          Enabled
        </label>
      </div>

      <div className="form-group checkbox-group">
        <label>
          <input
            type="checkbox"
            checked={step.continueOnError}
            onChange={(e) => handleChange('continueOnError', e.target.checked)}
          />
          Continue on Error
        </label>
      </div>

      <div className="section">
        <h4>Properties</h4>
        {stepTypeMetadata && stepTypeMetadata.properties.length > 0 && (
          <div className="property-suggestions">
            <p className="info">Suggested properties for {stepTypeMetadata.displayName}:</p>
            {stepTypeMetadata.properties.map((prop) => (
              <div key={prop.name} className="property-suggestion">
                <strong>{prop.name}</strong>
                {prop.required && <span className="required">*</span>}
                <span className="prop-desc">: {prop.description}</span>
              </div>
            ))}
          </div>
        )}
        {Object.entries(step.properties).map(([key, value]) => (
          <div key={key} className="property-item">
            <input
              type="text"
              value={key}
              readOnly
              className="property-key"
            />
            <input
              type="text"
              value={value}
              onChange={(e) => handlePropertyChange(key, e.target.value)}
              className="property-value"
              placeholder="Value"
            />
            <button onClick={() => handleDeleteProperty(key)} className="btn-delete-small">
              ✕
            </button>
          </div>
        ))}
        <button onClick={handleAddProperty} className="btn btn-small">
          + Add Property
        </button>
      </div>

      <div className="section">
        <h4>Conditions</h4>
        {step.conditions.map((condition, index) => (
          <div key={index} className="condition-item">
            <input
              type="text"
              value={condition.variableName}
              onChange={(e) => handleConditionChange(index, 'variableName', e.target.value)}
              placeholder="Variable name"
              className="condition-var"
            />
            <select
              value={condition.operator}
              onChange={(e) => handleConditionChange(index, 'operator', e.target.value)}
              className="condition-op"
            >
              {Object.values(ConditionOperator).map((op) => (
                <option key={op} value={op}>
                  {op}
                </option>
              ))}
            </select>
            <input
              type="text"
              value={condition.value}
              onChange={(e) => handleConditionChange(index, 'value', e.target.value)}
              placeholder="Value"
              className="condition-value"
            />
            <button onClick={() => handleDeleteCondition(index)} className="btn-delete-small">
              ✕
            </button>
          </div>
        ))}
        <button onClick={handleAddCondition} className="btn btn-small">
          + Add Condition
        </button>
      </div>
    </div>
  );
};

export default PropertiesPanel;
