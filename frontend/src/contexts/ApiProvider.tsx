import { createContext, useContext, useMemo, useState } from "react";

import { SeriesApi, SettingsApi } from "../lib/api/KapitelShelf.Api/api";
import { VersionApi } from "../lib/api/KapitelShelf.Api/api";
import { MetadataApi } from "../lib/api/KapitelShelf.Api/api";
import { UsersApi } from "../lib/api/KapitelShelf.Api/api";
import { TasksApi } from "../lib/api/KapitelShelf.Api/api";
import { CloudStorageApi } from "../lib/api/KapitelShelf.Api/api";
import { OneDriveApi } from "../lib/api/KapitelShelf.Api/api";
import { BooksApi } from "../lib/api/KapitelShelf.Api/api";
import { Configuration } from "../lib/api/KapitelShelf.Api/configuration";
import { GetMobileApiBaseUrl, IsMobileApp } from "../utils/MobileUtils";

interface ApiClients {
  books: BooksApi;
  series: SeriesApi;
  version: VersionApi;
  metadata: MetadataApi;
  users: UsersApi;
  tasks: TasksApi;
  cloudstorages: CloudStorageApi;
  onedrive: OneDriveApi;
  settings: SettingsApi;
}

interface ApiContextData {
  clients: ApiClients;
  basePath: string;
  setBasePath: (newUrl: string) => void;
}

interface ApiProviderProps {
  children: React.ReactNode;
}

const ApiProviderContext = createContext<ApiContextData | undefined>(undefined);

export const ApiProvider: React.FC<ApiProviderProps> = ({ children }) => {
  const InitialBasePath = (): string => {
    // load mobile api base path from localstorage
    if (IsMobileApp()) {
      return GetMobileApiBaseUrl() ?? "";
    }

    // use environment variable for web
    return import.meta.env.VITE_KAPITELSHELF_API ?? "";
  };

  const [basePath, setBasePath] = useState(InitialBasePath());

  // memoize Configuration and all client instances when basePath changes
  const clients: ApiClients = useMemo(() => {
    const config = new Configuration({ basePath });

    return {
      books: new BooksApi(config),
      series: new SeriesApi(config),
      version: new VersionApi(config),
      metadata: new MetadataApi(config),
      users: new UsersApi(config),
      tasks: new TasksApi(config),
      cloudstorages: new CloudStorageApi(config),
      onedrive: new OneDriveApi(config),
      settings: new SettingsApi(config),
    };
  }, [basePath]);

  return (
    <ApiProviderContext.Provider value={{ clients, basePath, setBasePath }}>
      {children}
    </ApiProviderContext.Provider>
  );
};

export function useApi(): ApiContextData {
  const ctx = useContext(ApiProviderContext);
  if (!ctx) {
    throw new Error("useApi must be used within <ApiProvider>");
  }

  return ctx;
}
