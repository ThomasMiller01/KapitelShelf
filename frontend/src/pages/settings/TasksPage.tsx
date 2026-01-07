import { Box, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { HiddenFilter } from "../../components/base/HiddenFilter";
import { TaskStateIcon } from "../../components/tasks/TaskStateIcon";
import { useApi } from "../../contexts/ApiProvider";
import TasksList from "../../features/tasks/TaskList";
import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { SECOND_MS } from "../../utils/TimeUtils";

export const TasksPage = (): ReactElement => {
  const { clients } = useApi();
  const {
    data: tasks,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["tasks-list"],
    queryFn: async () => {
      const { data } = await clients.tasks.tasksGet();
      return data;
    },
    refetchInterval: 5 * SECOND_MS,
  });

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
