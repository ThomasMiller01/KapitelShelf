import { Chip, Grid, Stack, Tooltip, Typography } from "@mui/material";
import cronstrue from "cronstrue";

import { useLiveTimeUntil } from "../../hooks/useLiveTimeUntil";
import { FinishedReasonNullable } from "../../lib/api/KapitelShelf.Api/api";
import { type TaskDTO, TaskState } from "../../lib/api/KapitelShelf.Api/api";
import {
  GetTaskCategoryColor,
  GetTaskFinishedReasonString,
} from "../../utils/TaskUtils";
import { FormatTime } from "../../utils/TimeUtils";
import { CircularProgressWithLabel } from "../base/feedback/CircularProgressWithLabel";
import { Property } from "../base/Property";
import { TaskFinishedReasonIcon } from "./TaskFinishedReasonIcon";
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
      <Grid size={{ xs: 10, lg: 3 }}>
        {/* Task Name */}
        <Typography variant="subtitle1" noWrap>
          {task.name}
        </Typography>

        {/* Description */}
        <Typography variant="body2" color="text.secondary" lineHeight={1.2}>
          {task.description}
        </Typography>

        {/* Category */}
        {task.category && (
          <Chip
            label={task.category}
            size="small"
            sx={{
              mt: 0.5,
              bgcolor: GetTaskCategoryColor(task.category),
            }}
          />
        )}
      </Grid>

      {/* Progress */}
      {task.state === TaskState.NUMBER_1 && (
        <Grid size={{ xs: 3, lg: 1 }}>
          <Property label="Progress">
            <CircularProgressWithLabel progress={task.progress} />
          </Property>
        </Grid>
      )}

      {/* Message */}
      {task.state === TaskState.NUMBER_1 && task.message !== null && (
        <Grid size={{ xs: 9, lg: 2 }}>
          <Property label="Message">
            <Tooltip title={task.message}>
              <Typography variant="subtitle1">{task.message}</Typography>
            </Tooltip>
          </Property>
        </Grid>
      )}

      {/* Finished Reason */}
      {task.state === TaskState.NUMBER_2 && (
        <Grid size={{ xs: 6, lg: 2 }}>
          <Property label="Reason">
            <FinishedReasonProperty reason={task.finishedReason} />
          </Property>
        </Grid>
      )}

      {/* CronJob Execution Description */}
      {task.isCronJob && (
        <Grid size={{ xs: task.state === TaskState.NUMBER_1 ? 6 : 12, lg: 2 }}>
          <Property label="Execution" tooltip={task.cronExpression}>
            {cronstrue.toString(task.cronExpression ?? "")}
          </Property>
        </Grid>
      )}

      {/* Next Execution */}
      <Grid size={{ xs: 6, lg: 1.5 }}>
        <Property
          label="Next Execution"
          tooltip={FormatTime(task.nextExecution)}
        >
          {nextExecutionFormatted}
        </Property>
      </Grid>

      {/* Last Execution */}
      <Grid size={{ xs: 6, lg: 1.5 }}>
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

interface FinishedReasonPropertyProps {
  reason: FinishedReasonNullable | undefined | null;
}

const FinishedReasonProperty: React.FC<FinishedReasonPropertyProps> = ({
  reason,
}) => {
  if (reason === undefined || reason === null) {
    return <></>;
  }

  let color = "primary";
  switch (reason) {
    case FinishedReasonNullable.NUMBER_0:
      color = "success";
      break;

    case FinishedReasonNullable.NUMBER_1:
      color = "error";
      break;

    default:
      break;
  }

  return (
    <Stack direction="row" spacing={1} alignItems="center">
      <TaskFinishedReasonIcon reason={reason} fontSize="small" />
      <Typography color={color}>
        {GetTaskFinishedReasonString(reason)}
      </Typography>
    </Stack>
  );
};
