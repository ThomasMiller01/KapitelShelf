import { Grid } from "@mui/material";
import { type ReactElement, useEffect, useRef, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import MetadataCard from "../../components/MetadataCard";
import { useMetadataSource } from "../../hooks/useMetadataSource";
import type { MetadataDTO } from "../../lib/api/KapitelShelf.Api/api";
import { MetadataSources } from "../../lib/api/KapitelShelf.Api/api";

interface MetadataListItem extends MetadataDTO {
  source: MetadataSources;
  id: string;
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
  const [sortedMetadata, setSortedMetadata] = useState<MetadataListItem[]>([]);

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

  useEffect(() => {
    // sort filteredMetadata
    // 1. by titleMatchScore
    // 2. by completenessScore´
    const sorted = [...filteredMetadata].sort((a, b) => {
      const aTitleScore = a.titleMatchScore ?? Number.NEGATIVE_INFINITY;
      const bTitleScore = b.titleMatchScore ?? Number.NEGATIVE_INFINITY;

      if (aTitleScore !== bTitleScore) {
        return bTitleScore - aTitleScore; // Descending
      }

      const aComp = a.completenessScore ?? Number.NEGATIVE_INFINITY;
      const bComp = b.completenessScore ?? Number.NEGATIVE_INFINITY;

      return bComp - aComp; // Descending
    });

    setSortedMetadata(sorted);
  }, [filteredMetadata]);

  // keep track of added sources to avoid duplicates
  const addedSources = useRef<Set<number>>(new Set());

  // watch for title change: clear everything
  useEffect(() => {
    setMetadata([]);
    addedSources.current.clear();
  }, [title]);

  // OpenLibrary
  const { data: openLibraryMetadata, isLoading: openLibraryLoading } =
    useMetadataSource({
      source: MetadataSources.NUMBER_0,
      title,
      enabled: sources?.includes(MetadataSources.NUMBER_0),
    });
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
          id: uuidv4(),
        })),
      ]);
      addedSources.current.add(MetadataSources.NUMBER_0);
    }
  }, [openLibraryMetadata, title]);

  // Amazon
  const { data: amazonMetadata, isLoading: amazonLoading } = useMetadataSource({
    source: MetadataSources.NUMBER_2,
    title,
    enabled: sources?.includes(MetadataSources.NUMBER_2),
  });
  useEffect(() => {
    if (amazonMetadata && !addedSources.current.has(MetadataSources.NUMBER_2)) {
      setMetadata((m) => [
        ...m,
        ...amazonMetadata.map((item) => ({
          ...item,
          source: MetadataSources.NUMBER_2,
          id: uuidv4(),
        })),
      ]);
      addedSources.current.add(MetadataSources.NUMBER_2);
    }
  }, [amazonMetadata, title]);

  // Google
  const { data: googleMetadata, isLoading: googleLoading } = useMetadataSource({
    source: MetadataSources.NUMBER_1,
    title,
    enabled: sources?.includes(MetadataSources.NUMBER_1),
  });
  useEffect(() => {
    if (googleMetadata && !addedSources.current.has(MetadataSources.NUMBER_1)) {
      setMetadata((m) => [
        ...m,
        ...googleMetadata.map((item) => ({
          ...item,
          source: MetadataSources.NUMBER_1,
          id: uuidv4(),
        })),
      ]);
      addedSources.current.add(MetadataSources.NUMBER_1);
    }
  }, [googleMetadata, title]);

  if (
    sortedMetadata.length === 0 &&
    (openLibraryLoading || amazonLoading || googleLoading)
  ) {
    return <LoadingCard delayed small itemName="Metadata" />;
  }

  if (
    sortedMetadata.length === 0 &&
    !openLibraryLoading &&
    !amazonLoading &&
    !googleLoading
  ) {
    return <NoItemsFoundCard itemName="Metadata" useLogo />;
  }

  return (
    <Grid container spacing={2}>
      {sortedMetadata.map((item) => (
        <Grid key={item.id} size={{ xs: 12, lg: 8, xl: 6 }}>
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
