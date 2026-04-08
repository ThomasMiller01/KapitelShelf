import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { CloudTypeDTO, ConfigureCloudDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useConfigureStorage = (
  storageType: CloudTypeDTO,
  onSuccess = () => {}
) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (configuration: ConfigureCloudDTO) => {
      await clients.cloudstorages.cloudstorageConfigurePut(
        storageType,
        configuration
      );
    },
    onSuccess,
  });
};
