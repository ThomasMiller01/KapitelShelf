import { Box, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { TaskStateIcon } from "../../components/tasks/TaskStateIcon";
import TasksList from "../../features/tasks/TaskList";
import { tasksApi } from "../../lib/api/KapitelShelf.Api";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { MINUTE_MS } from "../../utils/TimeUtils";

export const TasksPage = (): ReactElement => {
  const {
    data: tasks,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["tasks-list"],
    queryFn: async () => {
      const { data } = await tasksApi.tasksGet();
      return data;
    },
    refetchInterval: MINUTE_MS,
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Tasks" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Tasks" onRetry={refetch} />;
  }

  return (
    <Box padding="20px">
      <Typography variant="h5">Tasks</Typography>
      <TasksList
        name="Active Tasks"
        icon={<TaskStateIcon state={TaskState.NUMBER_1} />}
        tasks={tasks?.filter((x) => x.state === TaskState.NUMBER_1) ?? []}
      />
      <TasksList
        name="Scheduled Tasks"
        icon={<TaskStateIcon state={TaskState.NUMBER_0} />}
        tasks={tasks?.filter((x) => x.state === TaskState.NUMBER_0) ?? []}
      />
      <TasksList
        name="Finished Tasks"
        icon={<TaskStateIcon state={TaskState.NUMBER_2} />}
        tasks={tasks?.filter((x) => x.state === TaskState.NUMBER_2) ?? []}
      />
    </Box>
  );
};
