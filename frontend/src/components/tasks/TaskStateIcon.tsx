import AutorenewIcon from "@mui/icons-material/Autorenew";
import DoneIcon from "@mui/icons-material/Done";
import ScheduleIcon from "@mui/icons-material/Schedule";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";

import { TaskState } from "../../lib/api/KapitelShelf.Api/api";

interface TaskStateIconProps extends SvgIconOwnProps {
  state: TaskState | undefined;
  animated?: boolean;
}

export const TaskStateIcon: React.FC<TaskStateIconProps> = ({
  state,
  animated = false,
  sx,
  ...props
}) => {
  switch (state) {
    case TaskState.NUMBER_0:
      return (
        <Tooltip title="Scheduled">
          <ScheduleIcon {...props} sx={sx} />
        </Tooltip>
      );
    case TaskState.NUMBER_1:
      return (
        <Tooltip title="Running">
          <AutorenewIcon
            {...props}
            sx={{
              animation: animated ? "spin 1.2s linear infinite" : "unset",
              ...sx,
            }}
          />
        </Tooltip>
      );
    case TaskState.NUMBER_2:
      return (
        <Tooltip title="Finished">
          <DoneIcon {...props} sx={sx} />
        </Tooltip>
      );
    default:
      return <></>;
  }
};
