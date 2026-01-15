export enum StepType {
  Group = 'Group',
  InstallOperatingSystem = 'InstallOperatingSystem',
  ApplyWindowsImage = 'ApplyWindowsImage',
  ApplyFFUImage = 'ApplyFFUImage',
  InstallApplication = 'InstallApplication',
  InstallDriver = 'InstallDriver',
  CaptureUserState = 'CaptureUserState',
  RestoreUserState = 'RestoreUserState',
  RunCommandLine = 'RunCommandLine',
  RunPowerShell = 'RunPowerShell',
  SetVariable = 'SetVariable',
  RestartComputer = 'RestartComputer',
  FormatAndPartition = 'FormatAndPartition',
  Custom = 'Custom'
}

export enum ConditionOperator {
  Equals = 'Equals',
  NotEquals = 'NotEquals',
  GreaterThan = 'GreaterThan',
  LessThan = 'LessThan',
  Contains = 'Contains',
  Exists = 'Exists'
}

export interface TaskSequenceCondition {
  variableName: string;
  operator: ConditionOperator;
  value: string;
}

export interface TaskSequenceStep {
  id: string;
  name: string;
  description: string;
  type: StepType;
  enabled: boolean;
  continueOnError: boolean;
  conditions: TaskSequenceCondition[];
  properties: Record<string, string>;
  childSteps: TaskSequenceStep[];
}

export interface TaskSequenceVariable {
  name: string;
  value: string;
  isReadOnly: boolean;
  isSecret: boolean;
}

export interface TaskSequence {
  id: string;
  name: string;
  description: string;
  version: string;
  createdDate: string;
  modifiedDate: string;
  variables: TaskSequenceVariable[];
  steps: TaskSequenceStep[];
}

export interface StepTypeMetadata {
  type: string;
  displayName: string;
  description: string;
  icon: string;
  properties: PropertyDefinition[];
}

export interface PropertyDefinition {
  name: string;
  type: string;
  required: boolean;
  defaultValue: string;
  description: string;
}
