import AssignmentIcon from "@mui/icons-material/Assignment";
import {
  ListItemIcon,
  ListItemText,
  MenuItem,
  Stack,
  Typography,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { NavLink } from "react-router-dom";

import { tasksApi } from "../../lib/api/KapitelShelf.Api";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { TaskStateIcon } from "./TaskStateIcon";

export const TasksMenuItem = (): ReactElement => {
  const { data: activeTasks } = useQuery({
    queryKey: ["tasks-list-active"],
    queryFn: async () => {
      const { data } = await tasksApi.tasksGet();
      return data;
      // return data.filter((x) => x.state === TaskState.NUMBER_1);
    },
  });

  return (
    <MenuItem component={NavLink} to="/settings/tasks">
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
