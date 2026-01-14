import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useListCloudDirectory = (storageId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (path: string) => {
      if (storageId === undefined) {
        return;
      }

      const { data } =
        await clients.cloudstorages.cloudstorageStoragesStorageIdListDirectoriesGet(
          storageId,
          path
        );
      return data;
    },
  });
};
