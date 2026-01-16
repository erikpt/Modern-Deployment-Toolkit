import axios from 'axios';
import { TaskSequence, StepTypeMetadata, TaskSequenceStatus } from '../models/TaskSequence';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

export const taskSequenceService = {
  // Import a task sequence from a file
  async importTaskSequence(file: File): Promise<TaskSequence> {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await api.post<TaskSequence>('/tasksequence/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data;
  },

  // Export a task sequence to a specific format
  async exportTaskSequence(taskSequence: TaskSequence, format: string = 'yaml'): Promise<Blob> {
    const response = await api.post(`/tasksequence/export?format=${format}`, taskSequence, {
      responseType: 'blob'
    });
    return response.data;
  },

  // Validate a task sequence
  async validateTaskSequence(taskSequence: TaskSequence): Promise<{ valid: boolean; errors?: string[] }> {
    const response = await api.post('/tasksequence/validate', taskSequence);
    return response.data;
  },

  // Get available step types with metadata
  async getStepTypes(): Promise<StepTypeMetadata[]> {
    const response = await api.get<StepTypeMetadata[]>('/tasksequence/step-types');
    return response.data;
  },

  // Save a task sequence with status
  async saveTaskSequence(taskSequence: TaskSequence, status: TaskSequenceStatus = TaskSequenceStatus.Development): Promise<{ id: string; status: string; message: string }> {
    const response = await api.post(`/tasksequence/save?status=${status}`, taskSequence);
    return response.data;
  },

  // Commit a task sequence to a new status (promote)
  async commitTaskSequence(id: string, status: TaskSequenceStatus): Promise<{ id: string; previousStatus: string; newStatus: string; message: string }> {
    const response = await api.post('/tasksequence/commit', { id, status });
    return response.data;
  },

  // Load a task sequence by ID
  async loadTaskSequence(id: string): Promise<{ taskSequence: TaskSequence; status: string }> {
    const response = await api.get<{ taskSequence: TaskSequence; status: string }>(`/tasksequence/load/${id}`);
    return response.data;
  },

  // List all saved task sequences with optional status filter
  async listTaskSequences(status?: TaskSequenceStatus): Promise<Array<{
    id: string;
    name: string;
    description: string;
    version: string;
    status: string;
    baseTaskSequenceId: string;
    versionNumber: number;
    isActive: boolean;
    createdDate: string;
    modifiedDate: string;
  }>> {
    const url = status ? `/tasksequence/list?status=${status}` : '/tasksequence/list';
    const response = await api.get(url);
    return response.data;
  },

  // Create a new version of a task sequence
  async createNewVersion(baseTaskSequenceId: string, newVersion?: string): Promise<{
    id: string;
    version: string;
    versionNumber: number;
    status: string;
    message: string;
  }> {
    const response = await api.post('/tasksequence/create-version', { baseTaskSequenceId, newVersion });
    return response.data;
  },

  // Get all versions of a task sequence
  async getVersions(baseTaskSequenceId: string): Promise<Array<{
    id: string;
    name: string;
    version: string;
    versionNumber: number;
    status: string;
    isActive: boolean;
    description: string;
    createdDate: string;
    modifiedDate: string;
  }>> {
    const response = await api.get(`/tasksequence/versions/${baseTaskSequenceId}`);
    return response.data;
  },

  // Rollback to a previous production version
  async rollbackToVersion(versionId: string): Promise<{
    id: string;
    version: string;
    versionNumber: number;
    message: string;
  }> {
    const response = await api.post('/tasksequence/rollback', { versionId });
    return response.data;
  }
};
