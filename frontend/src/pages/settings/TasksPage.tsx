import FilterListIcon from "@mui/icons-material/FilterList";
import { Box, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { Selector } from "../../components/base/Selector";
import { TaskStateIcon } from "../../components/tasks/TaskStateIcon";
import TasksList from "../../features/tasks/TaskList";
import { useSetting } from "../../hooks/useSetting";
import { tasksApi } from "../../lib/api/KapitelShelf.Api";
import type { TaskDTO } from "../../lib/api/KapitelShelf.Api/api";
import { TaskState } from "../../lib/api/KapitelShelf.Api/api";
import { SECOND_MS } from "../../utils/TimeUtils";

const HIDDEN_CATEGORIES_KEY = "tasks.hide.categories";

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
    refetchInterval: 5 * SECOND_MS,
  });

  const [filteredTasks, setFilteredTasks] = useState<TaskDTO[]>(tasks ?? []);

  const [hiddenCategories, setHiddenCategories] = useSetting<string[]>(
    HIDDEN_CATEGORIES_KEY,
    ["Maintenance"]
  );

  // extract categories from tasks
  const [categories, setCategories] = useState<string[]>([]);
  useEffect(() => {
    if (tasks === undefined) {
      return;
    }

    // extract categories from tasks
    const allCategories = tasks.map((x) => x.category);

    // only allow strings
    const filteredCategories = allCategories.filter((x): x is string =>
      Boolean(x)
    );

    // get the unique categories and sort
    const uniqueCategories = Array.from(new Set(filteredCategories));
    const sortedCategories = uniqueCategories.sort();

    setCategories(sortedCategories);
  }, [tasks]);

  useEffect(() => {
    if (tasks === undefined) {
      return;
    }

    setFilteredTasks(
      tasks.filter((x) => !hiddenCategories.includes(x.category ?? ""))
    );
  }, [tasks, hiddenCategories]);

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
        <Selector
          icon={<FilterListIcon />}
          tooltip="Hide tasks by category"
          options={categories}
          selected={categories.filter((x) => !hiddenCategories.includes(x))}
          onUnselect={(value: string) =>
            setHiddenCategories(
              Array.from(new Set([...hiddenCategories, value]))
            )
          }
          onSelect={(value: string) =>
            setHiddenCategories(hiddenCategories.filter((x) => x !== value))
          }
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
