import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SortingModel } from "../../../hooks/url/useItemsTableParams";
import {
  ToSeriesSortByDTO,
  ToSortDirectionDTO,
} from "../../../utils/SortingUtils";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useSeriesListSimpleQueryProps {
  page: number;
  pageSize?: number;
  sorting?: SortingModel;
}

export const useSeriesListSimpleQuery = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  sorting,
}: useSeriesListSimpleQueryProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["series", page, pageSize, sorting],
    queryFn: async () => {
      const sortBy = ToSeriesSortByDTO(sorting?.field);
      const sortDir = ToSortDirectionDTO(sorting?.sort);

      const { data } = await clients.series.seriesGet(
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
