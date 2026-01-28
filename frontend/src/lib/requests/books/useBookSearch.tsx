import { useInfiniteQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

const DEFAULT_PAGE_SIZE = 24;

export const useBookSearch = (
  searchterm: string,
  pageSize = DEFAULT_PAGE_SIZE
) => {
  const { clients } = useApi();

  return useInfiniteQuery({
    queryKey: ["search-results", searchterm],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await clients.books.booksSearchGet(
        searchterm,
        pageParam,
        pageSize
      );
      return data;
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage, pages) => {
      const total = lastPage?.totalCount ?? 0;
      const loaded = pages.flatMap((p) => p?.items).length;
      return loaded < total ? pages.length + 1 : undefined;
    },
  });
};
