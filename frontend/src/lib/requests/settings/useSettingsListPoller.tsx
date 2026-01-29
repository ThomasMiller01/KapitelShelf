import { Query, useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { ObjectSettingsDTO } from "../../api/KapitelShelf.Api";

const refetchWhen = (
  query: Query<ObjectSettingsDTO[], Error, ObjectSettingsDTO[], string[]>,
) => {
  const data = query.state.data;

  if (!data) {
    return 5000;
  }

  // refetch while ai provider is not configured
  const provider = data.find((x) => x.key === "ai.provider");
  const providerConfigured = data.find(
    (x) => x.key === "ai.provider.configured",
  );

  const shouldRefetch =
    provider?.value !== "None" && !providerConfigured?.value;

  return shouldRefetch ? 5000 : false;
};

export const useSettingsListPoller = () => {
  const { clients } = useApi();

  return useQuery({
    queryKey: ["settings-list"],
    queryFn: async () => {
      const { data } = await clients.settings.settingsGet();
      return data;
    },
    refetchInterval: refetchWhen,
    staleTime: Infinity,
  });
};
