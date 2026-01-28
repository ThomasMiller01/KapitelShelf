import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useSettingsList = () => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["settings-list"],
    queryFn: async () => {
      const { data } = await clients.settings.settingsGet();
      return data;
    },
  });
};
