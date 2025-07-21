import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import CancelIcon from "@mui/icons-material/Cancel";
import FiberManualRecordIcon from "@mui/icons-material/FiberManualRecord";
import RepeatIcon from "@mui/icons-material/Repeat";
import type { SvgIconOwnProps } from "@mui/material";
import { Badge, GlobalStyles, Tooltip } from "@mui/material";

import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";

interface TaskTypeIconProps extends SvgIconOwnProps {
  task: TaskDTO;
}

export const TaskTypeIcon: React.FC<TaskTypeIconProps> = ({
  task,
  ...props
}) => {
  if (task.isCancelationRequested) {
    return (
      <Badge
        badgeContent={
          <>
            <GlobalStyles
              styles={{
                "@keyframes pulse-opacity": {
                  "0%, 100%": { opacity: 1 },
                  "50%": { opacity: 0.6 },
                },
              }}
            />
            <Tooltip title="Canceling ...">
              <CancelIcon
                fontSize="small"
                color="error"
                sx={{
                  bgcolor: "background.paper",
                  borderRadius: "15px",
                  animation: "pulse-opacity 2s infinite",
                }}
              />
            </Tooltip>
          </>
        }
        anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
      >
        <TaskTypeIconComponent task={task} {...props} />
      </Badge>
    );
  }

  return <TaskTypeIconComponent task={task} {...props} />;
};

const TaskTypeIconComponent: React.FC<TaskTypeIconProps> = ({
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
