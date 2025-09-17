import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";

export interface DeleteDialogProps {
  open: boolean;
  title?: string;
  description?: string;
  onCancel: () => void;
  onConfirm: () => void;
  confirmText?: string;
}

const DeleteDialog = ({
  open,
  title = "Confirm Delete",
  description = "Are you sure you want to delete this item? This action cannot be undone.",
  onCancel,
  onConfirm,
  confirmText = "Delete",
}: DeleteDialogProps): ReactElement => (
  <Dialog open={open} onClose={onCancel}>
    <DialogTitle textTransform="uppercase">{title}</DialogTitle>
    <DialogContent>
      <Typography>{description}</Typography>
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="error" variant="contained" onClick={onConfirm}>
        {confirmText}
      </Button>
    </DialogActions>
  </Dialog>
);

export default DeleteDialog;
