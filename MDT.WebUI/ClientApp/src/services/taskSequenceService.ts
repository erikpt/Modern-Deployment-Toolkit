import axios from 'axios';
import { TaskSequence, StepTypeMetadata } from '../models/TaskSequence';

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

  // Save a task sequence
  async saveTaskSequence(taskSequence: TaskSequence): Promise<{ id: string; message: string }> {
    const response = await api.post('/tasksequence/save', taskSequence);
    return response.data;
  },

  // Load a task sequence by ID
  async loadTaskSequence(id: string): Promise<TaskSequence> {
    const response = await api.get<TaskSequence>(`/tasksequence/load/${id}`);
    return response.data;
  },

  // List all saved task sequences
  async listTaskSequences(): Promise<Array<{
    id: string;
    name: string;
    description: string;
    version: string;
    modifiedDate: string;
  }>> {
    const response = await api.get('/tasksequence/list');
    return response.data;
  }
};
