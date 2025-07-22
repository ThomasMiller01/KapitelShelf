import { CloudTypeDTO } from "../lib/api/KapitelShelf.Api/api";

export const CloudTypeToString = (type: CloudTypeDTO | undefined): string => {
  switch (type) {
    case CloudTypeDTO.NUMBER_0:
      return "OneDrive";

    default:
      return "Unknown";
  }
};
