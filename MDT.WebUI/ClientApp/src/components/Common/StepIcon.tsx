import React from 'react';
import { FaFolder, FaDesktop, FaImage, FaBolt, FaBox, FaMicrochip, FaDownload, FaUpload, FaTerminal, FaCode, FaCog, FaSync, FaHdd, FaWrench } from 'react-icons/fa';
import { StepType } from '../../models/TaskSequence';

interface StepIconProps {
  stepType: StepType;
  className?: string;
}

const StepIcon: React.FC<StepIconProps> = ({ stepType, className = '' }) => {
  const iconMap: Record<StepType, React.ReactElement> = {
    [StepType.Group]: <FaFolder className={className} />,
    [StepType.InstallOperatingSystem]: <FaDesktop className={className} />,
    [StepType.ApplyWindowsImage]: <FaImage className={className} />,
    [StepType.ApplyFFUImage]: <FaBolt className={className} />,
    [StepType.InstallApplication]: <FaBox className={className} />,
    [StepType.InstallDriver]: <FaMicrochip className={className} />,
    [StepType.CaptureUserState]: <FaDownload className={className} />,
    [StepType.RestoreUserState]: <FaUpload className={className} />,
    [StepType.RunCommandLine]: <FaTerminal className={className} />,
    [StepType.RunPowerShell]: <FaCode className={className} />,
    [StepType.SetVariable]: <FaCog className={className} />,
    [StepType.RestartComputer]: <FaSync className={className} />,
    [StepType.FormatAndPartition]: <FaHdd className={className} />,
    [StepType.Custom]: <FaWrench className={className} />
  };

  return iconMap[stepType] || <FaWrench className={className} />;
};

export default StepIcon;
