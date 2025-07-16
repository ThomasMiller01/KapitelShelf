import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";

export interface ConfigureCloudDirectoryDialogProps {
  open: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export const ConfigureCloudDirectoryDialog = ({
  open,
  onCancel,
  onConfirm,
}: ConfigureCloudDirectoryDialogProps): ReactElement => (
  <Dialog open={open} onClose={onCancel}>
    <DialogTitle textTransform="uppercase">TODO</DialogTitle>
    <DialogContent>
      <Typography>TODO</Typography>
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="primary" variant="contained" onClick={onConfirm}>
        Save
      </Button>
    </DialogActions>
  </Dialog>
);
