import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useEffect, useState } from "react";

import { useMobile } from "../../hooks/useMobile";
import type { MetadataDTO } from "../../lib/api/KapitelShelf.Api/api";
import MetadataList from "./MetadataList";

// 400ms after user stops typing
const TITLE_REST_MS = 600;

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
        <DialogContentText>
          Click on a book to import its metadata.
        </DialogContentText>
      </DialogTitle>
      <DialogContent
        sx={{ minHeight: "400px", px: isMobile ? "10px" : "24px" }}
      >
        <MetadataList title={lockedInTitle} onClick={onConfirm} />
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
      </DialogActions>
    </Dialog>
  );
};

export default MetadataDialog;
