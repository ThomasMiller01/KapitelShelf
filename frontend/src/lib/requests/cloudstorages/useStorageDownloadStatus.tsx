import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useStorageDownloadStatus = (
  storageId: string | undefined,
  enabled: boolean = true
) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["cloudstorage-download-status", storageId],
    queryFn: async () => {
      if (storageId === undefined) {
        return null;
      }
      const { data } =
        await clients.cloudstorages.cloudstorageStoragesStorageIdGet(storageId);
      return data.isDownloaded;
    },
    enabled: enabled,
    refetchInterval: 5000,
  });
};
