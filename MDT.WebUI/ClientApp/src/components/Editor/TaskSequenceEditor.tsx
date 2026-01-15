import React, { useState, useEffect } from 'react';
import { TaskSequence, TaskSequenceStep, StepType, StepTypeMetadata, TaskSequenceStatus } from '../../models/TaskSequence';
import { taskSequenceService } from '../../services/taskSequenceService';
import Toolbar from './Toolbar';
import StepLibrary from './StepLibrary';
import StepCanvas from './StepCanvas';
import PropertiesPanel from './PropertiesPanel';
import VariablesEditor from './VariablesEditor';
import './TaskSequenceEditor.css';

const TaskSequenceEditor: React.FC = () => {
  const [taskSequence, setTaskSequence] = useState<TaskSequence>({
    id: '',
    name: 'New Task Sequence',
    description: '',
    version: '1.0.0',
    createdDate: new Date().toISOString(),
    modifiedDate: new Date().toISOString(),
    variables: [],
    steps: []
  });

  const [stepTypes, setStepTypes] = useState<StepTypeMetadata[]>([]);
  const [selectedStepId, setSelectedStepId] = useState<string | null>(null);
  const [currentStatus, setCurrentStatus] = useState<TaskSequenceStatus>(TaskSequenceStatus.Development);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  useEffect(() => {
    loadStepTypes();
  }, []);

  const loadStepTypes = async () => {
    try {
      const types = await taskSequenceService.getStepTypes();
      setStepTypes(types);
    } catch (error) {
      showMessage('error', 'Failed to load step types');
    }
  };

  const showMessage = (type: 'success' | 'error', text: string) => {
    setMessage({ type, text });
    setTimeout(() => setMessage(null), 5000);
  };

  const handleNew = () => {
    if (confirm('Create a new task sequence? Unsaved changes will be lost.')) {
      setTaskSequence({
        id: '',
        name: 'New Task Sequence',
        description: '',
        version: '1.0.0',
        createdDate: new Date().toISOString(),
        modifiedDate: new Date().toISOString(),
        variables: [],
        steps: []
      });
      setSelectedStepId(null);
      showMessage('success', 'New task sequence created');
    }
  };

  const handleImport = async (file: File) => {
    try {
      const imported = await taskSequenceService.importTaskSequence(file);
      setTaskSequence(imported);
      setCurrentStatus(TaskSequenceStatus.Development); // Reset to Development on import
      setSelectedStepId(null);
      showMessage('success', `Imported task sequence: ${imported.name}`);
    } catch (error: any) {
      showMessage('error', `Import failed: ${error.response?.data || error.message}`);
    }
  };

  const handleExport = async (format: string) => {
    try {
      const blob = await taskSequenceService.exportTaskSequence(taskSequence, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${taskSequence.name.replace(/\s+/g, '_')}_${taskSequence.id || 'new'}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      showMessage('success', `Exported as ${format.toUpperCase()}`);
    } catch (error: any) {
      showMessage('error', `Export failed: ${error.response?.data || error.message}`);
    }
  };

  const handleSave = async () => {
    try {
      const result = await taskSequenceService.saveTaskSequence(taskSequence, currentStatus);
      setTaskSequence({ ...taskSequence, id: result.id, modifiedDate: new Date().toISOString() });
      setCurrentStatus(result.status as TaskSequenceStatus);
      showMessage('success', result.message);
    } catch (error: any) {
      showMessage('error', `Save failed: ${error.response?.data || error.message}`);
    }
  };

  const handleCommit = async (newStatus: TaskSequenceStatus) => {
    if (!taskSequence.id) {
      showMessage('error', 'Please save the task sequence before committing');
      return;
    }

    try {
      const result = await taskSequenceService.commitTaskSequence(taskSequence.id, newStatus);
      setCurrentStatus(result.newStatus as TaskSequenceStatus);
      setTaskSequence({ ...taskSequence, modifiedDate: new Date().toISOString() });
      showMessage('success', result.message);
    } catch (error: any) {
      showMessage('error', `Commit failed: ${error.response?.data || error.message}`);
    }
  };

  const handleValidate = async () => {
    try {
      const result = await taskSequenceService.validateTaskSequence(taskSequence);
      if (result.valid) {
        showMessage('success', 'Task sequence is valid');
      } else {
        showMessage('error', `Validation errors: ${result.errors?.join(', ')}`);
      }
    } catch (error: any) {
      showMessage('error', `Validation failed: ${error.response?.data || error.message}`);
    }
  };

  const handleStepAdd = (stepType: string) => {
    const newStep: TaskSequenceStep = {
      id: `step-${Date.now()}`,
      name: '',
      description: '',
      type: stepType as StepType,
      enabled: true,
      continueOnError: false,
      conditions: [],
      properties: {},
      childSteps: []
    };

    setTaskSequence({
      ...taskSequence,
      steps: [...taskSequence.steps, newStep]
    });
    setSelectedStepId(newStep.id);
    showMessage('success', 'Step added');
  };

  const findAndUpdateStep = (steps: TaskSequenceStep[], stepId: string, updatedStep: TaskSequenceStep): TaskSequenceStep[] => {
    return steps.map((step) => {
      if (step.id === stepId) {
        return updatedStep;
      }
      if (step.childSteps.length > 0) {
        return {
          ...step,
          childSteps: findAndUpdateStep(step.childSteps, stepId, updatedStep)
        };
      }
      return step;
    });
  };

  const handleStepUpdate = (updatedStep: TaskSequenceStep) => {
    setTaskSequence({
      ...taskSequence,
      steps: findAndUpdateStep(taskSequence.steps, updatedStep.id, updatedStep)
    });
  };

  const findAndDeleteStep = (steps: TaskSequenceStep[], stepId: string): TaskSequenceStep[] => {
    return steps
      .filter((step) => step.id !== stepId)
      .map((step) => ({
        ...step,
        childSteps: findAndDeleteStep(step.childSteps, stepId)
      }));
  };

  const handleStepDelete = (stepId: string) => {
    if (confirm('Delete this step?')) {
      setTaskSequence({
        ...taskSequence,
        steps: findAndDeleteStep(taskSequence.steps, stepId)
      });
      if (selectedStepId === stepId) {
        setSelectedStepId(null);
      }
      showMessage('success', 'Step deleted');
    }
  };

  const handleStepMove = (stepId: string, direction: 'up' | 'down') => {
    const moveInArray = (arr: TaskSequenceStep[]): TaskSequenceStep[] => {
      const index = arr.findIndex((s) => s.id === stepId);
      if (index === -1) {
        return arr.map((s) => ({
          ...s,
          childSteps: moveInArray(s.childSteps)
        }));
      }

      const newIndex = direction === 'up' ? index - 1 : index + 1;
      if (newIndex < 0 || newIndex >= arr.length) {
        return arr;
      }

      const newArr = [...arr];
      [newArr[index], newArr[newIndex]] = [newArr[newIndex], newArr[index]];
      return newArr;
    };

    setTaskSequence({
      ...taskSequence,
      steps: moveInArray(taskSequence.steps)
    });
  };

  const getSelectedStep = (): TaskSequenceStep | null => {
    if (!selectedStepId) return null;

    const findStep = (steps: TaskSequenceStep[]): TaskSequenceStep | null => {
      for (const step of steps) {
        if (step.id === selectedStepId) return step;
        if (step.childSteps.length > 0) {
          const found = findStep(step.childSteps);
          if (found) return found;
        }
      }
      return null;
    };

    return findStep(taskSequence.steps);
  };

  const canSave = taskSequence.name.trim() !== '' && taskSequence.steps.length > 0;

  return (
    <div className="task-sequence-editor">
      <header className="editor-header">
        <h1>Task Sequence Editor</h1>
        <div className="metadata-editor">
          <input
            type="text"
            value={taskSequence.name}
            onChange={(e) => setTaskSequence({ ...taskSequence, name: e.target.value })}
            placeholder="Task Sequence Name"
            className="ts-name-input"
          />
          <input
            type="text"
            value={taskSequence.description}
            onChange={(e) => setTaskSequence({ ...taskSequence, description: e.target.value })}
            placeholder="Description"
            className="ts-desc-input"
          />
          <input
            type="text"
            value={taskSequence.version}
            onChange={(e) => setTaskSequence({ ...taskSequence, version: e.target.value })}
            placeholder="Version"
            className="ts-version-input"
          />
        </div>
        <VariablesEditor
          variables={taskSequence.variables}
          onVariablesChange={(variables) => setTaskSequence({ ...taskSequence, variables })}
        />
      </header>

      {message && (
        <div className={`message message-${message.type}`}>
          {message.text}
        </div>
      )}

      <Toolbar
        onNew={handleNew}
        onImport={handleImport}
        onExport={handleExport}
        onSave={handleSave}
        onValidate={handleValidate}
        onCommit={handleCommit}
        currentStatus={currentStatus}
        canSave={canSave}
        hasId={!!taskSequence.id}
      />

      <div className="editor-content">
        <StepLibrary stepTypes={stepTypes} onStepAdd={handleStepAdd} />
        <StepCanvas
          steps={taskSequence.steps}
          selectedStepId={selectedStepId}
          onStepSelect={setSelectedStepId}
          onStepDelete={handleStepDelete}
          onStepMove={handleStepMove}
        />
        <PropertiesPanel
          step={getSelectedStep()}
          stepTypes={stepTypes}
          onStepUpdate={handleStepUpdate}
        />
      </div>
    </div>
  );
};

export default TaskSequenceEditor;
