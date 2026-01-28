import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { CloudTypeDTO } from "../../api/KapitelShelf.Api";

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
