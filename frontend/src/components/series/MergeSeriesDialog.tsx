import AddLinkIcon from "@mui/icons-material/AddLink";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";

export interface MergeSeriesDialogProps {
  open: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

const MergeSeriesDialog = ({
  open,
  onCancel,
  onConfirm,
}: MergeSeriesDialogProps): ReactElement => (
  <Dialog open={open} onClose={onCancel}>
    <DialogTitle>
      <Box display="flex" alignItems="center" gap={1.5}>
        <AddLinkIcon />
        Merge Series
      </Box>
    </DialogTitle>
    <DialogContent>
      <Typography variant="body1" mb={2}>
        Select the target series to merge <b>all books</b> into:
      </Typography>
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="warning" variant="contained" onClick={onConfirm}>
        Merge
      </Button>
    </DialogActions>
  </Dialog>
);

export default MergeSeriesDialog;
