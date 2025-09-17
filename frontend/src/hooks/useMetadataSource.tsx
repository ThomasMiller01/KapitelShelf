import type { UseQueryResult } from "@tanstack/react-query";
import { useQuery } from "@tanstack/react-query";

import { useApi } from "../contexts/ApiProvider";
import type {
  MetadataDTO,
  MetadataSources,
} from "../lib/api/KapitelShelf.Api/api";
import { MetadataSourceToString } from "../utils/MetadataUtils";

interface useMetadataSourceProps {
  source: MetadataSources;
  title: string;
  enabled?: boolean;
}

export const useMetadataSource = ({
  source,
  title,
  enabled = true,
}: useMetadataSourceProps): UseQueryResult<MetadataDTO[]> => {
  const { clients } = useApi();
  return useQuery({
    enabled,
    queryKey: ["metadata-by-source", source, title],
    queryFn: async () => {
      const { data } = await clients.metadata.metadataSourceGet(source, title);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: `Fetching metadata from ${MetadataSourceToString[source]}`,
        showLoading: false,
        showSuccess: false,
        showError: true,
      },
    },
  });
};
