import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { CloudTypeDTO, ConfigureCloudDTO } from "../../api/KapitelShelf.Api";

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
