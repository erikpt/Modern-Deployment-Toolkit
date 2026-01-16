import React from 'react';
import TaskSequenceEditor from './components/Editor/TaskSequenceEditor';
import './App.css';

const App: React.FC = () => {
  return (
    <div className="app">
      <TaskSequenceEditor />
    </div>
  );
};

export default App;
