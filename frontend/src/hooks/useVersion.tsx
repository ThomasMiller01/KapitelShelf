import { useQuery } from "@tanstack/react-query";

import { versionApi } from "../lib/api/KapitelShelf.Api";

declare const __APP_VERSION__: string;

interface VersionInfo {
  APP_VERSION: string;
  BACKEND_VERSION: string;
}

export const useVersion = (): VersionInfo => {
  const { data: backendVersion } = useQuery({
    queryKey: ["backend-version"],
    queryFn: async () => {
      const { data } = await versionApi.versionGet();
      return data;
    },
  });

  return {
    APP_VERSION: __APP_VERSION__,
    BACKEND_VERSION: backendVersion ?? "",
  };
};
