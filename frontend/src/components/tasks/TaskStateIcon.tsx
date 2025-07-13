import AutorenewIcon from "@mui/icons-material/Autorenew";
import DoneIcon from "@mui/icons-material/Done";
import ScheduleIcon from "@mui/icons-material/Schedule";
import type { SvgIconOwnProps } from "@mui/material";
import { GlobalStyles, Tooltip } from "@mui/material";

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
        <>
          <GlobalStyles
            styles={{
              "@keyframes spin": {
                to: { transform: "rotate(360deg)" },
              },
            }}
          />{" "}
          <Tooltip title="Running">
            <AutorenewIcon
              {...props}
              sx={{
                animation: animated ? "spin 3s linear infinite" : "unset",
                ...sx,
              }}
            />
          </Tooltip>
        </>
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
