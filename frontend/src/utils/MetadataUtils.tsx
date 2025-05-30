import { MetadataSources } from "../lib/api/KapitelShelf.Api/api";

interface MetadataSourceToStringResult {
  [key: number]: string;
}

export const MetadataSourceToString: MetadataSourceToStringResult = {
  [MetadataSources.NUMBER_0]: "OpenLibrary",
  [MetadataSources.NUMBER_1]: "GoogleBooks",
  [MetadataSources.NUMBER_2]: "Amazon",
};
