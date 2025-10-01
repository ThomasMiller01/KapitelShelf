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

import { useApi } from "../../contexts/ApiProvider";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { SECOND_MS } from "../../utils/TimeUtils";
import { TaskStateIcon } from "./TaskStateIcon";

export const TasksMenuItem = (): ReactElement => {
  const { clients } = useApi();
  const { data: activeTasks } = useQuery({
    queryKey: ["tasks-list-active"],
    queryFn: async () => {
      const { data } = await clients.tasks.tasksGet();
      return data.filter((x) => x.state === TaskState.NUMBER_1);
    },
    refetchInterval: 5 * SECOND_MS,
  });

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
