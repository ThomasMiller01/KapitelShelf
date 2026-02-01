import { useMemo } from "react";
import { useSettingsList } from "../lib/requests/settings/useSettingsList";

export const useAiEnabled = () => {
  const { data: settings } = useSettingsList();

  const aiProviderEnabled = useMemo(() => {
    const providerConfigured = settings?.find(
      (x) => x.key === "ai.provider.configured",
    );
    return providerConfigured?.value;
  }, [settings]);

  return aiProviderEnabled;
};
