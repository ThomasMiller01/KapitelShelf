/* eslint-disable no-magic-numbers */
// src/services/api.ts
import type { LocationTypeDTO } from "./KapitelShelf.Api/api";
import { BooksApi, VersionApi } from "./KapitelShelf.Api/api";
import { Configuration } from "./KapitelShelf.Api/configuration";

const config = new Configuration({
  basePath: "http://localhost:5261", // replace with your actual API base URL
});

export const booksApi = new BooksApi(config);
export const versionApi = new VersionApi(config);

export const LocationTypeToString = (
  type: LocationTypeDTO | undefined
): string => {
  switch (type) {
    case 0:
      return "Physical";

    case 1:
      return "KapitelShelf";

    case 2:
      return "Kindle";

    case 3:
      return "Skoobe";

    case 4:
      return "Onleihe";

    case 5:
      return "Library";

    default:
      return "Unavailable";
  }
};
