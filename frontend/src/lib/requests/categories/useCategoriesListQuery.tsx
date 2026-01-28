import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SortingModel } from "../../../hooks/url/useItemsTableParams";
import {
  ToCategoriesSortByDTO,
  ToSortDirectionDTO,
} from "../../../utils/SortingUtils";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useCategoriesListQueryProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
  filter?: string | null;
}

export const useCategoriesListQuery = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
  filter,
}: useCategoriesListQueryProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["categories", page, pageSize, sorting, filter],
    queryFn: async () => {
      const sortBy = ToCategoriesSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.categories.categoriesGet(
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
