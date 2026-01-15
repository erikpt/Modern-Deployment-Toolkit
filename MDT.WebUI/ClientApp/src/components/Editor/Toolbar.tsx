import React from 'react';
import FileUpload from '../Common/FileUpload';
import { TaskSequenceStatus } from '../../models/TaskSequence';
import './Toolbar.css';

interface ToolbarProps {
  onNew: () => void;
  onImport: (file: File) => void;
  onExport: (format: string) => void;
  onSave: () => void;
  onValidate: () => void;
  onCommit: (status: TaskSequenceStatus) => void;
  onCreateVersion: () => void;
  onViewVersions: () => void;
  currentStatus: TaskSequenceStatus;
  canSave: boolean;
  hasId: boolean;
}

const STATUS_ORDER = [
  TaskSequenceStatus.Development,
  TaskSequenceStatus.Testing,
  TaskSequenceStatus.Production
];

const Toolbar: React.FC<ToolbarProps> = ({
  onNew,
  onImport,
  onExport,
  onSave,
  onValidate,
  onCommit,
  onCreateVersion,
  onViewVersions,
  currentStatus,
  canSave,
  hasId
}) => {
  const getStatusColor = (status: TaskSequenceStatus) => {
    switch (status) {
      case TaskSequenceStatus.Development:
        return '#6c757d';
      case TaskSequenceStatus.Testing:
        return '#ffc107';
      case TaskSequenceStatus.Production:
        return '#28a745';
      default:
        return '#6c757d';
    }
  };

  const canPromoteTo = (status: TaskSequenceStatus) => {
    if (!hasId) return false;
    
    const currentIndex = STATUS_ORDER.indexOf(currentStatus);
    const targetIndex = STATUS_ORDER.indexOf(status);
    
    return targetIndex > currentIndex;
  };

  return (
    <div className="toolbar">
      <div className="toolbar-group">
        <button onClick={onNew} className="btn btn-primary">
          New
        </button>
        <FileUpload onFileSelect={onImport} />
        <div className="export-group">
          <button onClick={() => onExport('yaml')} className="btn btn-secondary">
            Export YAML
          </button>
          <button onClick={() => onExport('json')} className="btn btn-secondary">
            Export JSON
          </button>
          <button onClick={() => onExport('xml')} className="btn btn-secondary">
            Export XML
          </button>
        </div>
      </div>
      
      <div className="toolbar-group">
        <div className="status-indicator" style={{ backgroundColor: getStatusColor(currentStatus) }}>
          Status: {currentStatus}
        </div>
        
        {hasId && (
          <>
            <button 
              onClick={onCreateVersion} 
              className="btn btn-secondary"
              title="Create new version"
            >
              📋 New Version
            </button>
            <button 
              onClick={onViewVersions} 
              className="btn btn-secondary"
              title="View all versions"
            >
              📚 Versions
            </button>
          </>
        )}
        
        {canPromoteTo(TaskSequenceStatus.Testing) && (
          <button 
            onClick={() => onCommit(TaskSequenceStatus.Testing)} 
            className="btn btn-warning"
            title="Commit to Testing"
          >
            → Testing
          </button>
        )}
        
        {canPromoteTo(TaskSequenceStatus.Production) && (
          <button 
            onClick={() => onCommit(TaskSequenceStatus.Production)} 
            className="btn btn-success"
            title="Commit to Production"
          >
            → Production
          </button>
        )}
      </div>

      <div className="toolbar-group">
        <button onClick={onValidate} className="btn btn-secondary">
          Validate
        </button>
        <button onClick={onSave} className="btn btn-success" disabled={!canSave}>
          Save
        </button>
      </div>
    </div>
  );
};

export default Toolbar;
