import { Chip, Grid, Typography } from "@mui/material";
import cronstrue from "cronstrue";

import { useLiveTimeUntil } from "../../hooks/useLiveTimeUntil";
import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";
import { FormatTime } from "../../utils/TimeUtils";
import { Property } from "../base/Property";
import { TaskTypeIcon } from "./TaskTypeIcon";

interface TaskEntryProps {
  task: TaskDTO;
}

export const TaskEntry: React.FC<TaskEntryProps> = ({ task }) => {
  const nextExecutionFormatted = useLiveTimeUntil({
    dateStr: task.nextExecution,
    allowPast: false,
  });
  const lastExecutionFormatted = useLiveTimeUntil({
    dateStr: task.lastExecution,
  });

  return (
    <Grid
      container
      alignItems="center"
      spacing={2}
      sx={{
        minHeight: 56,
        px: 2,
        py: 1,
        borderRadius: 2,
        bgcolor: "background.paper",
        boxShadow: 1,
        width: "100%",
      }}
    >
      <Grid>
        <TaskTypeIcon task={task} />
      </Grid>
      <Grid size={{ xs: 10, lg: 2 }}>
        <Typography variant="subtitle1" noWrap>
          {task.name}
        </Typography>
        {task.category && (
          <Chip label={task.category} size="small" sx={{ mt: 0.5 }} />
        )}
      </Grid>
      {task.isCronJob && (
        <Grid size={{ xs: 12, lg: 2 }}>
          <Property label="Execution" tooltip={task.cronExpression}>
            {cronstrue.toString(task.cronExpression ?? "")}
          </Property>
        </Grid>
      )}
      <Grid size={{ xs: 6, lg: 2 }}>
        <Property
          label="Next Execution"
          tooltip={FormatTime(task.nextExecution)}
        >
          {nextExecutionFormatted}
        </Property>
      </Grid>
      <Grid size={{ xs: 6, lg: 2 }}>
        <Property
          label="Last Execution"
          tooltip={FormatTime(task.lastExecution)}
        >
          {lastExecutionFormatted}
        </Property>
      </Grid>
    </Grid>
  );
};
