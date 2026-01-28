import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { CloudStorageDTO } from "../../api/KapitelShelf.Api";

export const useConfigureCloudDirectory = (cloudstorage: CloudStorageDTO) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (directory: string) => {
      if (cloudstorage.id === undefined) {
        return;
      }

      await clients.cloudstorages.cloudstorageStoragesStorageIdConfigureDirectoryPut(
        cloudstorage.id,
        directory
      );
    },
  });
};
