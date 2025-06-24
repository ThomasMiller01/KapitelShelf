import { Grid } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";

import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import type { MergeSeriesDialogProps } from "../../components/series/MergeSeriesDialog";
import MergeSeriesDialog from "../../components/series/MergeSeriesDialog";
import SeriesCard from "../../components/SeriesCard";
import { seriesApi } from "../../lib/api/KapitelShelf.Api";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";

// 600ms after user stops typing
const SERIESNAME_REST_MS = 600;

interface MergeSeriesDetailsProps
  extends Omit<MergeSeriesDialogProps, "onConfirm"> {
  seriesName: string | undefined | null;
  onConfirm: (series: SeriesDTO) => void;
}

const MergeSeriesDetails = ({
  seriesName,
  onConfirm,
  ...props
}: MergeSeriesDetailsProps): ReactElement => {
  const { mutateAsync: mutateGetSeriesSuggestions, isSuccess } = useMutation({
    mutationKey: ["merge-series-suggestions", seriesName],
    mutationFn: async (name: string | undefined | null) => {
      if (name === "" || name === undefined || name === null) {
        return [];
      }

      const { data } = await seriesApi.seriesSearchSuggestionsGet(name);
      return data;
    },
  });

  const [suggestions, setSuggestions] = useState<SeriesDTO[]>([]);
  useEffect(() => {
    const handle = setTimeout(
      () =>
        mutateGetSeriesSuggestions(seriesName).then((response) =>
          setSuggestions(response)
        ),
      SERIESNAME_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [seriesName, mutateGetSeriesSuggestions]);

  const [selectedSeries, setSelectedSeries] = useState<SeriesDTO | undefined>(
    undefined
  );
  const handleSelect = (series: SeriesDTO): void => {
    setSelectedSeries(series);
  };

  const handleConfirm = (): void => {
    if (selectedSeries === undefined) {
      return;
    }

    setSelectedSeries(undefined);
    onConfirm(selectedSeries);
  };

  if (seriesName !== "" && suggestions.length === 0 && isSuccess) {
    return (
      <MergeSeriesDialog
        {...props}
        onConfirm={handleConfirm}
        suggestionsContent={<NoItemsFoundCard itemName="Series" small />}
      />
    );
  }

  if (seriesName === "" || suggestions.length === 0) {
    return (
      <MergeSeriesDialog
        {...props}
        onConfirm={handleConfirm}
        suggestionsContent={<NoItemsFoundCard itemName="Series" small />}
      />
    );
  }

  return (
    <MergeSeriesDialog
      {...props}
      onConfirm={handleConfirm}
      suggestionsContent={
        <Grid container spacing={2}>
          {suggestions.map((suggestion) => (
            <Grid
              key={suggestion.id}
              size={{ xs: 12, md: 6 }}
              justifyItems="center"
            >
              <SeriesCard
                series={suggestion}
                itemVariant="detailed"
                enableLink={false}
                onClick={() => handleSelect(suggestion)}
              />
            </Grid>
          ))}
        </Grid>
      }
    />
  );
};

export default MergeSeriesDetails;
