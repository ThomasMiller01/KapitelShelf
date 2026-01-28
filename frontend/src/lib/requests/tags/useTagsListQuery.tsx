import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SortingModel } from "../../../hooks/url/useItemsTableParams";
import {
  ToSortDirectionDTO,
  ToTagsSortByDTO,
} from "../../../utils/SortingUtils";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useTagsListQueryProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
  filter?: string | null;
}

export const useTagsListQuery = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
  filter,
}: useTagsListQueryProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["tags", page, pageSize, sorting, filter],
    queryFn: async () => {
      const sortBy = ToTagsSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.tags.tagsGet(
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
