import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { CloudTypeDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useStorageConfigured = (storageType: CloudTypeDTO) => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["cloudstorage-configured", storageType],
    queryFn: async () => {
      const { data } = await clients.cloudstorages.cloudstorageIsconfiguredGet(
        storageType
      );
      return data;
    },
  });
};
