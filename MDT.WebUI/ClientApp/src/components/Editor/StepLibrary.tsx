import React from 'react';
import { StepTypeMetadata } from '../../models/TaskSequence';
import StepIcon from '../Common/StepIcon';
import './StepLibrary.css';

interface StepLibraryProps {
  stepTypes: StepTypeMetadata[];
  onStepAdd: (stepType: string) => void;
}

const StepLibrary: React.FC<StepLibraryProps> = ({ stepTypes, onStepAdd }) => {
  return (
    <div className="step-library">
      <h3>Step Library</h3>
      <div className="step-list">
        {stepTypes.map((stepType) => (
          <div
            key={stepType.type}
            className="step-item"
            onClick={() => onStepAdd(stepType.type)}
            title={stepType.description}
          >
            <StepIcon stepType={stepType.type as any} className="step-icon" />
            <span className="step-name">{stepType.displayName}</span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default StepLibrary;
