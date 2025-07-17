import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";
import { type ReactElement, useState } from "react";

import { CloudTypeDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { ConfigureOneDrive } from "./ConfigureOneDrive";

export interface ConfigureCloudDirectoryDialogProps {
  open: boolean;
  onCancel: () => void;
  onConfirm: (directory: string) => void;
  cloudType: CloudTypeDTO | undefined;
  storageId: string | undefined;
}

export const ConfigureCloudDirectoryDialog: React.FC<
  ConfigureCloudDirectoryDialogProps
> = ({ cloudType, storageId, onConfirm, onCancel, ...props }) => {
  const [directory, setDirectory] = useState<string | null>(null);

  const handleCancel = (): void => {
    setDirectory(null);
    onCancel();
  };

  const handleConfirm = (): void => {
    if (directory === null) {
      return;
    }

    onConfirm(directory);
  };

  switch (cloudType) {
    case CloudTypeDTO.NUMBER_0:
      return (
        <ConfigureCloudDirectoryDialogLayout
          directory={directory}
          cloudType={cloudType}
          storageId=""
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          {...props}
        >
          <ConfigureOneDrive
            storageId={storageId}
            onDirectorySelect={setDirectory}
          />
        </ConfigureCloudDirectoryDialogLayout>
      );
  }
};

interface ConfigureCloudDirectoryDialogLayoutProps
  extends ConfigureCloudDirectoryDialogProps {
  directory: string | null;
  onConfirm: () => void;
  children?: ReactElement | ReactElement[];
}

const ConfigureCloudDirectoryDialogLayout: React.FC<
  ConfigureCloudDirectoryDialogLayoutProps
> = ({ directory, open, onCancel, onConfirm, children }) => (
  <Dialog open={open} onClose={onCancel} maxWidth="md" fullWidth>
    <DialogTitle>Configure the Cloud Directory</DialogTitle>
    <DialogContent>{children}</DialogContent>
    <DialogContent sx={{ minHeight: "fit-content", pb: "10px" }}>
      <TextField
        label="Selected"
        variant="filled"
        size="small"
        value={directory}
        fullWidth
        slotProps={{ inputLabel: { shrink: directory !== null } }}
      />
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="primary" variant="contained" onClick={onConfirm}>
        Select
      </Button>
    </DialogActions>
  </Dialog>
);
