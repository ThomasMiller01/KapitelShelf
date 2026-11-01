import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

const DEFAULT_COUNT = 24;

export const useLastReadBooks = (count = DEFAULT_COUNT) => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  return useQuery({
    queryKey: ["last-read-books", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.users.usersUserIdLastreadbooksGet(
        profile?.id,
        1,
        count,
      );
      return data;
    },
  });
};
