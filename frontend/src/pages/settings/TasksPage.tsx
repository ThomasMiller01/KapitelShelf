import { Box } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";

import { tasksApi } from "../../lib/api/KapitelShelf.Api";

export const TasksPage = (): ReactElement => {
  const { data: tasks } = useQuery({
    queryKey: ["tasks-list"],
    queryFn: async () => {
      const { data } = await tasksApi.tasksGet();
      return data;
    },
  });
  console.log(tasks);

  return <Box>Tasks</Box>;
};
