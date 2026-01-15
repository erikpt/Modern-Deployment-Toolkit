import React from 'react';
import FileUpload from '../Common/FileUpload';
import './Toolbar.css';

interface ToolbarProps {
  onNew: () => void;
  onImport: (file: File) => void;
  onExport: (format: string) => void;
  onSave: () => void;
  onValidate: () => void;
  canSave: boolean;
}

const Toolbar: React.FC<ToolbarProps> = ({
  onNew,
  onImport,
  onExport,
  onSave,
  onValidate,
  canSave
}) => {
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
