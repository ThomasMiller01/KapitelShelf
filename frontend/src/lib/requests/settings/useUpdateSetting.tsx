import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { UserSettingDTO } from "../../api/KapitelShelf.Api";

export const useUpdateSetting = (setting: UserSettingDTO | undefined) => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (value: any) => {
      if (setting === undefined || setting.id === undefined) {
        return;
      }

      await clients.settings.settingsSettingIdPut(setting.id, value);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["settings-list"] });
    },
  });
};
