import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useSeriesById = (seriesId: string | undefined) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["series-by-id", seriesId],
    queryFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      const { data } = await clients.series.seriesSeriesIdGet(seriesId);
      return data;
    },
  });
};
