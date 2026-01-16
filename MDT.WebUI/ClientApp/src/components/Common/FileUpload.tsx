import React, { useRef } from 'react';
import './FileUpload.css';

interface FileUploadProps {
  onFileSelect: (file: File) => void;
  accept?: string;
}

const FileUpload: React.FC<FileUploadProps> = ({ onFileSelect, accept = '.xml,.json,.yaml,.yml' }) => {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      onFileSelect(file);
    }
  };

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="file-upload">
      <input
        ref={fileInputRef}
        type="file"
        accept={accept}
        onChange={handleFileChange}
        style={{ display: 'none' }}
      />
      <button onClick={handleClick} className="btn btn-secondary">
        Import File
      </button>
    </div>
  );
};

export default FileUpload;
