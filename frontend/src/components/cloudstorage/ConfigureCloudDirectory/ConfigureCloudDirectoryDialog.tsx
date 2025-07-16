import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from "@mui/material";
import { type ReactElement, useState } from "react";

import { CloudType } from "../../../lib/api/KapitelShelf.Api/api";
import { ConfigureOneDrive } from "./ConfigureOneDrive";

export interface ConfigureCloudDirectoryDialogProps {
  open: boolean;
  onCancel: () => void;
  onConfirm: (directory: string) => void;
  cloudType: CloudType | undefined;
}

export const ConfigureCloudDirectoryDialog: React.FC<
  ConfigureCloudDirectoryDialogProps
> = ({ cloudType, onConfirm, ...props }) => {
  const [directory, setDirectory] = useState<string | null>(null);

  const handleConfirm = (): void => {
    if (directory === null) {
      return;
    }

    onConfirm(directory);
  };

  switch (cloudType) {
    case CloudType.NUMBER_0:
      return (
        <ConfigureCloudDirectoryDialogLayout
          cloudType={cloudType}
          onConfirm={handleConfirm}
          {...props}
        >
          <ConfigureOneDrive onDirectorySelect={setDirectory} />
        </ConfigureCloudDirectoryDialogLayout>
      );
  }
};

interface ConfigureCloudDirectoryDialogLayoutProps
  extends ConfigureCloudDirectoryDialogProps {
  onConfirm: () => void;
  children?: ReactElement | ReactElement[];
}

const ConfigureCloudDirectoryDialogLayout: React.FC<
  ConfigureCloudDirectoryDialogLayoutProps
> = ({ open, onCancel, onConfirm, children }) => (
  <Dialog open={open} onClose={onCancel} maxWidth="md" fullWidth>
    <DialogTitle>Configure the Cloud Directory</DialogTitle>
    <DialogContent>{children}</DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="primary" variant="contained" onClick={onConfirm}>
        Save
      </Button>
    </DialogActions>
  </Dialog>
);
