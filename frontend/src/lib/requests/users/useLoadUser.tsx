import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useLoadUser = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (userId: string) => {
      const { data } = await clients.users.usersUserIdGet(userId);
      return data;
    },
  });
};
