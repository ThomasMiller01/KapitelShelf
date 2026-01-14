import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useMergeSeriesSuggestions = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (name: string | undefined | null) => {
      if (name === "" || name === undefined || name === null) {
        return [];
      }

      const { data } = await clients.series.seriesSearchSuggestionsGet(name);
      return data;
    },
  });
};
