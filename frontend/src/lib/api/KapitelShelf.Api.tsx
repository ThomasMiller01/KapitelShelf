// src/services/api.ts
import { BooksApi, VersionApi } from "./KapitelShelf.Api/api";
import { Configuration } from "./KapitelShelf.Api/configuration";

const config = new Configuration({
  basePath: "http://localhost:5261", // replace with your actual API base URL
});

export const booksApi = new BooksApi(config);
export const versionApi = new VersionApi(config);
