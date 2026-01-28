import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

const DEFAULT_COUNT = 24;

export const useLastVisitedBooks = (count = DEFAULT_COUNT) => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  return useQuery({
    queryKey: ["last-visited-books", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.users.usersUserIdLastvisitedbooksGet(
        profile?.id,
        1,
        count
      );
      return data;
    },
  });
};
