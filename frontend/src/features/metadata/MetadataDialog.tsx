import CheckIcon from "@mui/icons-material/Check";
import RemoveIcon from "@mui/icons-material/Remove";
import {
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Grid,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useEffect, useState } from "react";

import { useLocalStorage } from "../../hooks/useLocalStorage";
import { useMobile } from "../../hooks/useMobile";
import {
  type MetadataDTO,
  MetadataSources,
} from "../../lib/api/KapitelShelf.Api/api";
import { MetadataSourceToString } from "../../utils/MetadataUtils";
import MetadataList from "./MetadataList";

// 400ms after user stops typing
const TITLE_REST_MS = 600;

const STORAGE_KEY = "metadata.selected.sources";
const STORAGE_DEFAULT_VALUE = [
  MetadataSources.NUMBER_0,
  MetadataSources.NUMBER_1,
  MetadataSources.NUMBER_2,
];

export interface MetadataDialogProps {
  open: boolean;
  title: string;
  onCancel: () => void;
  onConfirm: (metadata: MetadataDTO) => void;
}

const MetadataDialog = ({
  open,
  title: initialTitle,
  onCancel,
  onConfirm,
}: MetadataDialogProps): ReactElement => {
  const { isMobile } = useMobile();
  const [getItem, setItem] = useLocalStorage();

  const [searchTitle, setSearchTitle] = useState(initialTitle);
  const [lockedInTitle, setLockedInTitle] = useState(initialTitle);

  // reset when title changes or dialog opens again
  useEffect(() => {
    if (open) {
      setSearchTitle(initialTitle);
      setLockedInTitle(initialTitle);
    }
  }, [open, initialTitle]);

  // lock in title, when user stops typing
  useEffect(() => {
    const handle = setTimeout(
      () => setLockedInTitle(searchTitle),
      TITLE_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [searchTitle]);

  const [selectedSources, setSelectedSources] = useState<number[]>(() => {
    const stored = getItem(STORAGE_KEY);
    if (!stored) {
      return STORAGE_DEFAULT_VALUE;
    }

    try {
      return JSON.parse(stored) as number[];
    } catch {
      return STORAGE_DEFAULT_VALUE;
    }
  });
  const handleSourceClick = (source: number): void => {
    setSelectedSources((prev) => {
      const updated = prev.includes(source)
        ? prev.filter((s) => s !== source)
        : [...prev, source];
      setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  };

  return (
    <Dialog
      open={open}
      onClose={onCancel}
      fullWidth
      maxWidth="xl"
      fullScreen={isMobile}
    >
      <DialogTitle>
        <Stack direction="row" alignItems="center" spacing={1} mb="10px">
          <Typography sx={{ whiteSpace: "nowrap" }}>Metadata for</Typography>
          <TextField
            variant="outlined"
            size="small"
            value={searchTitle}
            onChange={(e) => setSearchTitle(e.target.value)}
            fullWidth
          />
        </Stack>
        <Grid container spacing={1} mb="10px">
          {/* Amazon */}
          <MetadataSourceItem
            source={MetadataSources.NUMBER_2}
            selected={selectedSources.includes(MetadataSources.NUMBER_2)}
            onClick={() => handleSourceClick(MetadataSources.NUMBER_2)}
          />

          {/* Google */}
          <MetadataSourceItem
            source={MetadataSources.NUMBER_1}
            selected={selectedSources.includes(MetadataSources.NUMBER_1)}
            onClick={() => handleSourceClick(MetadataSources.NUMBER_1)}
          />

          {/* OpenLibrary */}
          <MetadataSourceItem
            source={MetadataSources.NUMBER_0}
            selected={selectedSources.includes(MetadataSources.NUMBER_0)}
            onClick={() => handleSourceClick(MetadataSources.NUMBER_0)}
          />
        </Grid>
        <DialogContentText>
          Click on a book to import its metadata.
        </DialogContentText>
      </DialogTitle>
      <DialogContent
        sx={{ minHeight: "400px", px: isMobile ? "10px" : "24px" }}
      >
        <MetadataList
          title={lockedInTitle}
          onClick={onConfirm}
          sources={selectedSources}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
      </DialogActions>
    </Dialog>
  );
};

interface MetadataSourceItemProps {
  source: number;
  selected: boolean;
  onClick: () => void;
}

const MetadataSourceItem = ({
  source,
  selected,
  onClick,
}: MetadataSourceItemProps): ReactElement => (
  <Chip
    icon={selected ? <CheckIcon /> : <RemoveIcon />}
    label={MetadataSourceToString[source]}
    onClick={onClick}
    variant={selected ? "filled" : "outlined"}
    color="primary"
    size="small"
    sx={{ my: "4px !important" }}
  />
);

export default MetadataDialog;
