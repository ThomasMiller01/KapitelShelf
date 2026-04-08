import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { CloudTypeDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useListStorages = (storageType: CloudTypeDTO) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["cloudstorage-list-cloudstorages", storageType],
    queryFn: async () => {
      const { data } = await clients.cloudstorages.cloudstorageStoragesGet(
        storageType
      );
      return data;
    },
  });
};
