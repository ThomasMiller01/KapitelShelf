import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SECOND_MS } from "../../../utils/TimeUtils";

export const useTasksList = () => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["tasks-list"],
    queryFn: async () => {
      const { data } = await clients.tasks.tasksGet();
      return data;
    },
    refetchInterval: 5 * SECOND_MS,
  });
};
