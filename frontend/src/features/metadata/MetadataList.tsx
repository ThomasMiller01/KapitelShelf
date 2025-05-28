import { Grid } from "@mui/material";
import { type ReactElement, useEffect, useRef, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import MetadataCard from "../../components/MetadataCard";
import { useMetadataSource } from "../../hooks/useMetadataSource";
import type { MetadataDTO } from "../../lib/api/KapitelShelf.Api/api";
import { MetadataSources } from "../../lib/api/KapitelShelf.Api/api";

interface MetadataListProps {
  title: string;
  onClick?: (metadata: MetadataDTO) => void;
}

const MetadataList = ({ title, onClick }: MetadataListProps): ReactElement => {
  const [metadata, setMetadata] = useState<MetadataDTO[]>([]);

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
      setMetadata((m) => [...m, ...openLibraryMetadata]);
      addedSources.current.add(MetadataSources.NUMBER_0);
    }
  }, [openLibraryMetadata, title]);

  if (openLibraryLoading) {
    return <LoadingCard delayed small itemName="Metadata" />;
  }

  if (metadata.length === 0) {
    return <NoItemsFoundCard itemName="Metadata" useLogo />;
  }

  return (
    <Grid container spacing={2}>
      {metadata.map((item, index) => (
        <Grid key={index} size={{ xs: 12, lg: 8, xl: 6 }}>
          <MetadataCard metadata={item} onClick={onClick} />
        </Grid>
      ))}
    </Grid>
  );
};

export default MetadataList;
