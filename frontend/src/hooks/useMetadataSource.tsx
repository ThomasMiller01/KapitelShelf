import type { UseQueryResult } from "@tanstack/react-query";
import { useQuery } from "@tanstack/react-query";

import { metadataApi } from "../lib/api/KapitelShelf.Api";
import type {
  MetadataDTO,
  MetadataSources,
} from "../lib/api/KapitelShelf.Api/api";

interface useMetadataSourceProps {
  source: MetadataSources;
  title: string;
}

export const useMetadataSource = ({
  source,
  title,
}: useMetadataSourceProps): UseQueryResult<MetadataDTO[]> =>
  useQuery({
    queryKey: ["metadata-by-source", source, title],
    queryFn: async () => {
      const { data } = await metadataApi.metadataSourceGet(source, title);
      return data;
    },
  });
