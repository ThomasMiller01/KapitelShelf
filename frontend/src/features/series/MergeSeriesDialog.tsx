import AddLinkIcon from "@mui/icons-material/AddLink";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useEffect, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import SeriesCard from "../../components/SeriesCard";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useMergeSeriesSuggestions } from "../../lib/requests/series/useMergeSeriesSuggestions";

// 600ms after user stops typing
const SERIESNAME_REST_MS = 600;

interface MergeSeriesListProps {
  seriesId: string | undefined | null;
  seriesName: string | undefined | null;
  selectedSeries: SeriesDTO | undefined;
  onSelectSeries: (series: SeriesDTO) => void;
}

const MergeSeriesList = ({
  seriesId,
  seriesName,
  selectedSeries,
  onSelectSeries,
}: MergeSeriesListProps): ReactElement => {
  const {
    mutateAsync: mutateGetSeriesSuggestions,
    isSuccess,
    isPending,
  } = useMergeSeriesSuggestions();

  const [suggestions, setSuggestions] = useState<SeriesDTO[]>([]);
  useEffect(() => {
    const handle = setTimeout(
      () =>
        mutateGetSeriesSuggestions(seriesName).then((response) =>
          // dont show current series in list of possible series to merge with
          setSuggestions(response.filter((x) => x.id !== seriesId))
        ),
      SERIESNAME_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [seriesId, seriesName, mutateGetSeriesSuggestions]);

  if (isPending) {
    return <LoadingCard delayed itemName="Series" />;
  }

  if (seriesName !== "" && suggestions.length === 0 && isSuccess) {
    return <NoItemsFoundCard itemName="Series" small />;
  }

  if (seriesName === "" || suggestions.length === 0) {
    return <NoItemsFoundCard itemName="Series" small />;
  }

  return (
    <Box>
      <Typography variant="body1" mb={2}>
        Select the target series to merge <b>all books</b> into:
      </Typography>
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
              linkEnabled={false}
              showMetadata
              selected={selectedSeries?.id === suggestion.id}
              onClick={() => onSelectSeries(suggestion)}
            />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

interface MergeSeriesDialogProps {
  series: SeriesDTO | undefined | null;
  open: boolean;
  onCancel: () => void;
  onConfirm: (series: SeriesDTO) => void;
}

const MergeSeriesDialog = ({
  series,
  open,
  onConfirm,
  onCancel,
}: MergeSeriesDialogProps): ReactElement => {
  const [seriesName, setSeriesName] = useState(series?.name);
  const [selectedSeries, setSelectedSeries] = useState<SeriesDTO | undefined>(
    undefined
  );

  const handleConfirm = (): void => {
    if (selectedSeries === undefined) {
      return;
    }

    reset();
    onConfirm(selectedSeries);
  };

  const handleCancel = (): void => {
    reset();
    onCancel();
  };

  const reset = (): void => {
    setSelectedSeries(undefined);
    setSeriesName(series?.name);
  };

  return (
    <Dialog open={open} onClose={handleCancel} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center" gap={1.5}>
          <AddLinkIcon />
          Merge Series
        </Box>
      </DialogTitle>
      <DialogContent sx={{ pt: "10px !important", pb: "0" }}>
        <TextField
          variant="outlined"
          size="small"
          value={seriesName}
          onChange={(e) => setSeriesName(e.target.value)}
          sx={{ mb: "10px" }}
          fullWidth
          label="Series Name"
          error={seriesName === ""}
          helperText={
            seriesName === ""
              ? "Enter the Series Name to start searching"
              : undefined
          }
        />
        <MergeSeriesList
          seriesId={series?.id}
          seriesName={seriesName}
          selectedSeries={selectedSeries}
          onSelectSeries={setSelectedSeries}
        />
      </DialogContent>
      {selectedSeries !== undefined && (
        <DialogContent sx={{ pt: "10px !important", pb: "0" }}>
          <Alert severity="info" sx={{ mt: "10px", width: "fit-content" }}>
            This will merge{" "}
            <Typography
              display="inline-block"
              variant="body2"
              fontWeight="bold"
            >
              all books
            </Typography>{" "}
            from{" "}
            <Typography
              display="inline-block"
              variant="body2"
              fontWeight="bold"
            >
              {series?.name}
            </Typography>{" "}
            into{" "}
            <Typography
              display="inline-block"
              variant="body2"
              fontWeight="bold"
            >
              {selectedSeries?.name}
            </Typography>
            .
          </Alert>
        </DialogContent>
      )}
      <DialogActions>
        <Button onClick={handleCancel}>Cancel</Button>
        <Button
          color="warning"
          variant="contained"
          onClick={handleConfirm}
          disabled={selectedSeries === undefined}
        >
          Merge
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default MergeSeriesDialog;
