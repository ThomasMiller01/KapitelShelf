import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { UserSettingDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

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
