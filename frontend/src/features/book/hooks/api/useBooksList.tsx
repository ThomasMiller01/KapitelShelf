import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { SortingModel } from "../../../../shared/hooks/useItemsTableParams";
import {
  ToBookSortByDTO,
  ToSortDirectionDTO,
} from "../../../../shared/utils/SortingUtils";
import { MINUTE_MS } from "../../../../shared/utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useBooksListProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
  filter?: string | null;
}

export const useBooksList = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
  filter,
}: useBooksListProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["books", page, pageSize, sorting, filter],
    queryFn: async () => {
      const sortBy = ToBookSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.books.booksPaginatedGet(
        page,
        pageSize,
        sortBy,
        sortDir,
        filter ?? undefined,
      );
      return data;
    },
    placeholderData: (previousData) => previousData,
    staleTime: 5 * MINUTE_MS,
    refetchOnMount: "always",
  });
};
