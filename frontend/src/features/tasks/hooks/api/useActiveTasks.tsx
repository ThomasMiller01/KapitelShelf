import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { SECOND_MS } from "../../../../shared/utils/TimeUtils";
import { TaskState } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useActiveTasks = () => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["tasks-list-active"],
    queryFn: async () => {
      const { data } = await clients.tasks.tasksGet();
      return data.filter((x) => x.state === TaskState.NUMBER_1);
    },
    refetchInterval: 5 * SECOND_MS,
  });
};
