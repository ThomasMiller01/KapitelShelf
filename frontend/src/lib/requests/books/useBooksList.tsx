import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SortingModel } from "../../../hooks/url/useItemsTableParams";
import {
  ToBookSortByDTO,
  ToSortDirectionDTO,
} from "../../../utils/SortingUtils";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useBooksListProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
}

export const useBooksList = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
}: useBooksListProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["books", page, pageSize, sorting],
    queryFn: async () => {
      const sortBy = ToBookSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.books.booksPaginatedGet(
        page,
        pageSize,
        sortBy,
        sortDir,
      );
      return data;
    },
    placeholderData: (previousData) => previousData,
    staleTime: 5 * MINUTE_MS,
    refetchOnMount: "always",
  });
};
