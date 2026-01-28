import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteStorage = (onSuccess = () => {}) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (storageId: string | undefined) => {
      if (storageId === undefined) {
        return;
      }

      await clients.cloudstorages.cloudstorageStoragesStorageIdDelete(
        storageId
      );
    },
    onSuccess,
  });
};
