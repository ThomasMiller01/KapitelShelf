import AssignmentIcon from "@mui/icons-material/Assignment";
import {
  ListItemIcon,
  ListItemText,
  MenuItem,
  Stack,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";
import { NavLink } from "react-router-dom";

import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { useActiveTasks } from "../../lib/requests/tasks/useActiveTasks";
import { TaskStateIcon } from "./TaskStateIcon";

export const TasksMenuItem = (): ReactElement => {
  const { data: activeTasks } = useActiveTasks();

  return (
    <MenuItem component={NavLink} to="/settings/tasks" sx={{ my: "6px" }}>
      <ListItemIcon>
        <AssignmentIcon fontSize="small" />
      </ListItemIcon>
      <ListItemText>Tasks</ListItemText>
      {(activeTasks?.length ?? 0) > 0 && (
        <Stack
          direction="row"
          spacing={0.6}
          alignItems="end"
          justifyContent="end"
          justifyItems="end"
        >
          <Typography variant="body2" sx={{ color: "text.secondary" }}>
            {activeTasks?.length}
          </Typography>
          <ListItemIcon sx={{ minWidth: "auto !important" }}>
            <TaskStateIcon
              state={TaskState.NUMBER_1}
              animated
              fontSize="small"
              sx={{ color: "text.secondary" }}
            />
          </ListItemIcon>
        </Stack>
      )}
    </MenuItem>
  );
};
