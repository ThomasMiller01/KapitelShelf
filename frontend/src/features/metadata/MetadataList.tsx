import { Grid } from "@mui/material";
import { type ReactElement, useEffect, useRef, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import MetadataCard from "../../components/MetadataCard";
import { useMetadataSource } from "../../hooks/useMetadataSource";
import type { MetadataDTO } from "../../lib/api/KapitelShelf.Api/api";
import { MetadataSources } from "../../lib/api/KapitelShelf.Api/api";

interface MetadataListItem extends MetadataDTO {
  source: MetadataSources;
}

interface MetadataListProps {
  title: string;
  onClick?: (metadata: MetadataDTO) => void;
  sources?: number[];
}

const MetadataList = ({
  title,
  onClick,
  sources,
}: MetadataListProps): ReactElement => {
  const [metadata, setMetadata] = useState<MetadataListItem[]>([]);
  const [filteredMetadata, setFilteredMetadata] = useState<MetadataListItem[]>(
    []
  );

  useEffect(() => {
    // filter metadata based on selected sources
    if (sources) {
      setFilteredMetadata(
        metadata.filter((item) => sources.includes(item.source))
      );
    } else {
      setFilteredMetadata(metadata);
    }
  }, [metadata, sources]);

  // keep track of added sources to avoid duplicates
  const addedSources = useRef<Set<number>>(new Set());

  // watch for title change: clear everything
  useEffect(() => {
    setMetadata([]);
    addedSources.current.clear();
  }, [title]);

  // OpenLibrary
  const { data: openLibraryMetadata, isLoading: openLibraryLoading } =
    useMetadataSource({ source: MetadataSources.NUMBER_0, title });
  useEffect(() => {
    if (
      openLibraryMetadata &&
      !addedSources.current.has(MetadataSources.NUMBER_0)
    ) {
      setMetadata((m) => [
        ...m,
        ...openLibraryMetadata.map((item) => ({
          ...item,
          source: MetadataSources.NUMBER_0,
        })),
      ]);
      addedSources.current.add(MetadataSources.NUMBER_0);
    }
  }, [openLibraryMetadata, title]);

  if (openLibraryLoading) {
    return <LoadingCard delayed small itemName="Metadata" />;
  }

  if (filteredMetadata.length === 0) {
    return <NoItemsFoundCard itemName="Metadata" useLogo />;
  }

  return (
    <Grid container spacing={2}>
      {filteredMetadata.map((item, index) => (
        <Grid key={index} size={{ xs: 12, lg: 8, xl: 6 }}>
          <MetadataCard
            metadata={item}
            onClick={onClick}
            source={item.source}
          />
        </Grid>
      ))}
    </Grid>
  );
};

export default MetadataList;
