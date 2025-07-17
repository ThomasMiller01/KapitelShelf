import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from "@mui/material";
import { type ReactElement, useState } from "react";

import type { ConfigureCloudDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { CloudTypeDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { CloudTypeToString } from "../../../utils/CloudStorageUtils";
import { ConfigureOneDrive } from "./ConfigureOneDrive";

export interface ConfigureCloudConfigurationDialogProps {
  open: boolean;
  onCancel: () => void;
  onConfirm: (configuration: ConfigureCloudDTO) => void;
  cloudType: CloudTypeDTO;
}

export const ConfigureCloudConfigurationDialog: React.FC<
  ConfigureCloudConfigurationDialogProps
> = ({ cloudType, onConfirm, ...props }) => {
  const [configuration, setConfiguration] = useState<ConfigureCloudDTO>({});

  const handleConfirm = (): void => {
    onConfirm(configuration);
  };

  switch (cloudType) {
    case CloudTypeDTO.NUMBER_0:
      return (
        <ConfigureCloudConfigurationDialogLayout
          cloudType={cloudType}
          onConfirm={handleConfirm}
          {...props}
        >
          <ConfigureOneDrive onConfigurationChange={setConfiguration} />
        </ConfigureCloudConfigurationDialogLayout>
      );
  }
};

interface ConfigureCloudConfigurationDialogLayoutProps
  extends ConfigureCloudConfigurationDialogProps {
  onConfirm: () => void;
  children?: ReactElement | ReactElement[];
}

const ConfigureCloudConfigurationDialogLayout: React.FC<
  ConfigureCloudConfigurationDialogLayoutProps
> = ({ open, onCancel, onConfirm, cloudType, children }) => (
  <Dialog open={open} onClose={onCancel} maxWidth="lg" fullWidth>
    <DialogTitle>
      Configure the {CloudTypeToString(cloudType)} Storage
    </DialogTitle>
    <DialogContent>{children}</DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button color="primary" variant="contained" onClick={onConfirm}>
        Save
      </Button>
    </DialogActions>
  </Dialog>
);
