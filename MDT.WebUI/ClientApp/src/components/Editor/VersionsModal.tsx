import React from 'react';
import './VersionsModal.css';

interface Version {
  id: string;
  name: string;
  version: string;
  versionNumber: number;
  status: string;
  isActive: boolean;
  description: string;
  createdDate: string;
  modifiedDate: string;
}

interface VersionsModalProps {
  versions: Version[];
  onClose: () => void;
  onLoadVersion: (versionId: string) => void;
  onRollback: (versionId: string) => void;
}

const VersionsModal: React.FC<VersionsModalProps> = ({
  versions,
  onClose,
  onLoadVersion,
  onRollback
}) => {
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Version History</h2>
          <button onClick={onClose} className="btn-close">✕</button>
        </div>
        
        <div className="modal-body">
          {versions.length === 0 ? (
            <p>No versions found.</p>
          ) : (
            <table className="versions-table">
              <thead>
                <tr>
                  <th>Version</th>
                  <th>Status</th>
                  <th>Active</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {versions.map((version) => (
                  <tr key={version.id} className={version.isActive ? 'active-version' : ''}>
                    <td>
                      <strong>v{version.version}</strong>
                      <br />
                      <small>#{version.versionNumber}</small>
                    </td>
                    <td>
                      <span className={`status-badge status-${version.status.toLowerCase()}`}>
                        {version.status}
                      </span>
                    </td>
                    <td>{version.isActive ? '✓' : ''}</td>
                    <td>{new Date(version.createdDate).toLocaleDateString()}</td>
                    <td>
                      <button
                        onClick={() => onLoadVersion(version.id)}
                        className="btn btn-small btn-secondary"
                      >
                        Load
                      </button>
                      {version.status === 'Production' && !version.isActive && (
                        <button
                          onClick={() => onRollback(version.id)}
                          className="btn btn-small btn-warning"
                          style={{ marginLeft: '5px' }}
                        >
                          Rollback
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
        
        <div className="modal-footer">
          <button onClick={onClose} className="btn btn-secondary">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default VersionsModal;
