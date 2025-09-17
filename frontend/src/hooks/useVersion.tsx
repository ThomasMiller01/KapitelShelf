import { useQuery } from "@tanstack/react-query";

import { useApi } from "../contexts/ApiProvider";

declare const __APP_VERSION__: string;

interface VersionInfo {
  APP_VERSION: string;
  BACKEND_VERSION: string;
}

export const useVersion = (): VersionInfo => {
  const { clients } = useApi();
  const { data: backendVersion } = useQuery({
    queryKey: ["backend-version"],
    queryFn: async () => {
      const { data } = await clients.version.versionGet();
      return data;
    },
  });

  return {
    APP_VERSION: __APP_VERSION__,
    BACKEND_VERSION: backendVersion ?? "~",
  };
};
