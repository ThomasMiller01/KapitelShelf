import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useProxyCover = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (coverUrl: string) =>
      clients.metadata.metadataProxyCoverGet(coverUrl, {
        responseType: "blob",
      }),
  });
};
