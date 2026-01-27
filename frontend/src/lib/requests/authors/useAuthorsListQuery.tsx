import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SortingModel } from "../../../hooks/url/useItemsTableParams";
import {
  ToAuthorsSortByDTO,
  ToSortDirectionDTO,
} from "../../../utils/SortingUtils";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useAuthorsListQueryProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
  filter?: string | null;
}

export const useAuthorsListQuery = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
  filter,
}: useAuthorsListQueryProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["authors", page, pageSize, sorting, filter],
    queryFn: async () => {
      const sortBy = ToAuthorsSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.authors.authorsGet(
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
