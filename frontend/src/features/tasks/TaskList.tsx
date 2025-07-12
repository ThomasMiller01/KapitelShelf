import { Divider, Paper, Stack, Typography } from "@mui/material";
import type { ReactElement } from "react";

import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { TaskEntry } from "../../components/tasks/TaskEntry";
import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";

interface TasksListItemsProps {
  name: string;
  tasks: TaskDTO[];
}

interface TasksListProps extends TasksListItemsProps {
  icon?: ReactElement;
}

const TasksList: React.FC<TasksListProps> = ({ name, icon, tasks }) => (
  <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
    <Stack direction="row" spacing={1} alignItems="center" mb="5px">
      {icon}
      <Typography variant="h6">{name}</Typography>
    </Stack>
    <Divider sx={{ mb: 2 }} />
    <TasksListItems name={name} tasks={tasks} />
  </Paper>
);

const TasksListItems: React.FC<TasksListItemsProps> = ({ name, tasks }) => {
  if (tasks.length === 0) {
    return <NoItemsFoundCard itemName={name} extraSmall wording="normal" />;
  }

  return (
    <Stack spacing={{ xs: 1.5, lg: 1 }}>
      {tasks.map((task) => (
        <TaskEntry
          task={task}
          key={(task?.name ?? "") + (task?.category ?? "")}
        />
      ))}
    </Stack>
  );
};

export default TasksList;
