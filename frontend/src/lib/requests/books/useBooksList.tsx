import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useBooksListProps {
  page: number;
  pageSize?: number;
}

export const useBooksList = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
}: useBooksListProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["books", page, pageSize],
    queryFn: async () => {
      const { data } = await clients.books.booksPaginatedGet(page, pageSize);
      return data;
    },
    placeholderData: (previousData) => previousData,
    staleTime: 5 * MINUTE_MS,
    refetchOnMount: "always",
  });
};
