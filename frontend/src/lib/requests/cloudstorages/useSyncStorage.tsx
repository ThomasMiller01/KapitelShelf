import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useSyncStorage = (onSuccess = () => {}) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (storageId: string | undefined) => {
      if (storageId === undefined) {
        return;
      }

      await clients.cloudstorages.cloudstorageStoragesStorageIdSyncPut(
        storageId
      );
    },
    onSuccess,
  });
};
