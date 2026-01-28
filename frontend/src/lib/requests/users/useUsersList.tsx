import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useUsersList = () => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["user-profile-list"],
    queryFn: async () => {
      const { data } = await clients.users.usersGet();
      return data;
    },
    refetchOnMount: "always",
  });
};
