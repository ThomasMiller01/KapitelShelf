import { Box, Stack, Typography } from "@mui/material";
import { type ReactElement, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { HiddenFilter } from "../../components/base/HiddenFilter";
import { TaskStateIcon } from "../../components/tasks/TaskStateIcon";
import TasksList from "../../features/tasks/TaskList";
import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { useTasksList } from "../../lib/requests/tasks/useTasksList";

export const TasksPage = (): ReactElement => {
  const { data: tasks, isLoading, isError, refetch } = useTasksList();

  const [filteredTasks, setFilteredTasks] = useState<TaskDTO[]>(tasks ?? []);

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Tasks" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Tasks" onRetry={refetch} />;
  }

  return (
    <Box padding="20px">
      <Typography variant="h5" gutterBottom>
        Tasks
      </Typography>
      <Stack direction="row" justifyContent="end">
        <HiddenFilter
          items={tasks ?? []}
          settingsKey="tasks.hide.categories"
          defaultHiddenOptions={["Maintenance"]}
          tooltip="Hide tasks by category"
          extractValue={(x) => x.category}
          onFilteredChange={(next) => setFilteredTasks(next)}
        />
      </Stack>
      <TasksList
        name="Active Tasks"
        icon={
          <TaskStateIcon
            state={TaskState.NUMBER_1}
            animated={
              (
                filteredTasks?.filter((x) => x.state === TaskState.NUMBER_1) ??
                []
              ).length > 0
            }
          />
        }
        tasks={
          filteredTasks?.filter((x) => x.state === TaskState.NUMBER_1) ?? []
        }
      />
      <TasksList
        name="Scheduled Tasks"
        icon={<TaskStateIcon state={TaskState.NUMBER_0} />}
        tasks={
          filteredTasks?.filter((x) => x.state === TaskState.NUMBER_0) ?? []
        }
      />
      <TasksList
        name="Finished Tasks"
        icon={<TaskStateIcon state={TaskState.NUMBER_2} />}
        tasks={
          filteredTasks?.filter((x) => x.state === TaskState.NUMBER_2) ?? []
        }
      />
    </Box>
  );
};
