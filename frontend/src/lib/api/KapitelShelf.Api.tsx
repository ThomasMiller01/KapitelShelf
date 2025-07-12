import {
  BooksApi,
  MetadataApi,
  SeriesApi,
  TasksApi,
  UsersApi,
  VersionApi,
} from "./KapitelShelf.Api/api";
import { Configuration } from "./KapitelShelf.Api/configuration";

const config = new Configuration({
  basePath: import.meta.env.VITE_KAPITELSHELF_API,
});

export const booksApi = new BooksApi(config);
export const seriesApi = new SeriesApi(config);
export const versionApi = new VersionApi(config);
export const metadataApi = new MetadataApi(config);
export const usersApi = new UsersApi(config);
export const tasksApi = new TasksApi(config);
