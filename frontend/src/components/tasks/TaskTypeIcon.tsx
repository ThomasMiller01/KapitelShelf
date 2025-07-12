import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import FiberManualRecordIcon from "@mui/icons-material/FiberManualRecord";
import RepeatIcon from "@mui/icons-material/Repeat";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";

import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";

interface TaskTypeIconProps extends SvgIconOwnProps {
  task: TaskDTO;
}

export const TaskTypeIcon: React.FC<TaskTypeIconProps> = ({
  task,
  ...props
}) => {
  // cronjob
  if (task.isCronJob) {
    return (
      <Tooltip title="CronJob">
        <CalendarMonthIcon {...props} />
      </Tooltip>
    );
  }

  // single execution
  if (task.isSingleExecution) {
    return (
      <Tooltip title="One Time">
        <FiberManualRecordIcon {...props} />
      </Tooltip>
    );
  }

  // recuring execution
  return (
    <Tooltip title="Recuring">
      <RepeatIcon {...props} />
    </Tooltip>
  );
};
