import React from 'react';
import { TaskSequenceStep } from '../../models/TaskSequence';
import StepIcon from '../Common/StepIcon';
import './StepCanvas.css';

interface StepCanvasProps {
  steps: TaskSequenceStep[];
  selectedStepId: string | null;
  onStepSelect: (stepId: string) => void;
  onStepDelete: (stepId: string) => void;
  onStepMove: (stepId: string, direction: 'up' | 'down') => void;
}

const StepCanvas: React.FC<StepCanvasProps> = ({
  steps,
  selectedStepId,
  onStepSelect,
  onStepDelete,
  onStepMove
}) => {
  const renderStep = (step: TaskSequenceStep, depth: number = 0) => {
    const isSelected = step.id === selectedStepId;
    const hasConditions = step.conditions.length > 0;
    
    return (
      <div key={step.id} style={{ marginLeft: `${depth * 20}px` }}>
        <div
          className={`step-node ${isSelected ? 'selected' : ''} ${!step.enabled ? 'disabled' : ''}`}
          onClick={(e) => {
            e.stopPropagation();
            onStepSelect(step.id);
          }}
        >
          <div className="step-info">
            <StepIcon stepType={step.type} className="step-icon-small" />
            <span className="step-name">{step.name || '(Unnamed Step)'}</span>
            {hasConditions && <span className="condition-indicator" title="Has conditions">⚠️</span>}
            {!step.enabled && <span className="disabled-indicator" title="Disabled">🚫</span>}
          </div>
          <div className="step-actions">
            <button
              onClick={(e) => {
                e.stopPropagation();
                onStepMove(step.id, 'up');
              }}
              title="Move up"
            >
              ▲
            </button>
            <button
              onClick={(e) => {
                e.stopPropagation();
                onStepMove(step.id, 'down');
              }}
              title="Move down"
            >
              ▼
            </button>
            <button
              onClick={(e) => {
                e.stopPropagation();
                onStepDelete(step.id);
              }}
              className="btn-delete"
              title="Delete"
            >
              ✕
            </button>
          </div>
        </div>
        {step.childSteps && step.childSteps.length > 0 && (
          <div className="child-steps">
            {step.childSteps.map((child) => renderStep(child, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="step-canvas">
      <h3>Task Sequence Steps</h3>
      <div className="steps-tree">
        {steps.length === 0 ? (
          <div className="empty-state">
            <p>No steps yet. Add steps from the library on the left.</p>
          </div>
        ) : (
          steps.map((step) => renderStep(step))
        )}
      </div>
    </div>
  );
};

export default StepCanvas;
