import { CloudType } from "../lib/api/KapitelShelf.Api/api";

export const CloudTypeToString = (type: CloudType): string => {
  switch (type) {
    case CloudType.NUMBER_0:
      return "OneDrive";

    default:
      return "Unknown";
  }
};
