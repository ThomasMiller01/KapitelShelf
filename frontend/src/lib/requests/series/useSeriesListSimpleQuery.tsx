import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { MINUTE_MS } from "../../../utils/TimeUtils";

const DEFAULT_PAGE_SIZE = 24;

interface useSeriesListSimpleQueryProps {
  page: number;
  pageSize?: number;
}

export const useSeriesListSimpleQuery = ({
  page,
  pageSize = DEFAULT_PAGE_SIZE,
}: useSeriesListSimpleQueryProps) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["series", page, pageSize],
    queryFn: async () => {
      const { data } = await clients.series.seriesGet(page, pageSize);
      return data;
    },
    placeholderData: (previousData) => previousData,
    staleTime: 5 * MINUTE_MS,
    refetchOnMount: "always",
  });
};
