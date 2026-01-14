import CreateNewFolderIcon from "@mui/icons-material/CreateNewFolder";
import TuneIcon from "@mui/icons-material/Tune";
import { Button, Divider, Paper, Stack, Typography } from "@mui/material";
import { type ReactElement, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { CloudStorageIcon } from "../../components/cloudstorage/CloudStorageIcon";
import { CloudStorageCard } from "../../components/cloudstorage/CloudStoragesCard";
import { ConfigureCloudConfigurationDialog } from "../../components/cloudstorage/ConfigureCloudConfiguration/ConfigureCloudConfigurationDialog";
import { useApi } from "../../contexts/ApiProvider";
import type { ConfigureCloudDTO } from "../../lib/api/KapitelShelf.Api/api";
import { CloudTypeDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useConfigureStorage } from "../../lib/requests/cloudstorages/useConfigureStorage";
import { useListStorages } from "../../lib/requests/cloudstorages/useListStorages";
import { useOneDriveStartOAuthFlow } from "../../lib/requests/cloudstorages/useOneDriveStartOAuthFlow";
import { useStorageConfigured } from "../../lib/requests/cloudstorages/useSorageConfigured";

export const OneDriveSettings = (): ReactElement => {
  const { clients } = useApi();
  const [configureDialogOpen, setConfigureDialogOpen] = useState(false);

  const {
    data: isConfigured,
    isLoading,
    isError,
    refetch,
  } = useStorageConfigured(CloudTypeDTO.NUMBER_0);

  const { data: cloudstorages, refetch: updateStorages } = useListStorages(
    CloudTypeDTO.NUMBER_0
  );

  const { mutate: startOAuthFlow } = useOneDriveStartOAuthFlow();

  const { mutate: configure } = useConfigureStorage(CloudTypeDTO.NUMBER_0, () =>
    refetch()
  );

  if (isLoading) {
    return (
      <OneDriveSettingsLayout>
        <LoadingCard delayed itemName="OneDrive Storage" small />
      </OneDriveSettingsLayout>
    );
  }

  if (isError) {
    return (
      <OneDriveSettingsLayout>
        <RequestErrorCard itemName="OneDrive storage" onRetry={refetch} small />
      </OneDriveSettingsLayout>
    );
  }

  if (!isConfigured) {
    return (
      <OneDriveSettingsLayout>
        <Button
          variant="contained"
          color="primary"
          startIcon={<TuneIcon />}
          onClick={() => setConfigureDialogOpen(true)}
          sx={{ mb: "15px" }}
        >
          Configure
        </Button>
        <ConfigureCloudConfigurationDialog
          open={configureDialogOpen}
          onCancel={() => setConfigureDialogOpen(false)}
          onConfirm={(configuration: ConfigureCloudDTO) => {
            configure(configuration);
            setConfigureDialogOpen(false);
          }}
          cloudType={CloudTypeDTO.NUMBER_0}
        />
      </OneDriveSettingsLayout>
    );
  }

  return (
    <OneDriveSettingsLayout onConfigure={() => setConfigureDialogOpen(true)}>
      <Button
        variant="contained"
        color="primary"
        startIcon={<CreateNewFolderIcon />}
        onClick={() => startOAuthFlow()}
        sx={{ mb: "15px" }}
      >
        Add Storage
      </Button>
      <Stack spacing={1} mb="5px">
        {cloudstorages?.map((cloudstorage) => (
          <CloudStorageCard
            key={cloudstorage.id}
            cloudstorage={cloudstorage}
            getOAuthUrl={clients.onedrive.cloudstorageOnedriveOauthGet}
            update={() => updateStorages()}
          />
        ))}
      </Stack>
      <ConfigureCloudConfigurationDialog
        open={configureDialogOpen}
        onCancel={() => setConfigureDialogOpen(false)}
        onConfirm={(configuration: ConfigureCloudDTO) => {
          configure(configuration);
          setConfigureDialogOpen(false);
        }}
        cloudType={CloudTypeDTO.NUMBER_0}
      />
    </OneDriveSettingsLayout>
  );
};

interface OneDriveSettingsLayoutProps {
  children: ReactElement | ReactElement[];
  onConfigure?: () => void;
}

const OneDriveSettingsLayout: React.FC<OneDriveSettingsLayoutProps> = ({
  children,
  onConfigure,
}) => (
  <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
    <Stack direction="row" spacing={1} alignItems="center">
      <CloudStorageIcon
        type={CloudTypeDTO.NUMBER_0}
        sx={{ mr: "5px !important" }}
      />
      <Typography variant="h6">OneDrive</Typography>
      {onConfigure && (
        <IconButtonWithTooltip
          tooltip="Configure OneDrive"
          onClick={onConfigure}
        >
          <TuneIcon color="primary" fontSize="small" />
        </IconButtonWithTooltip>
      )}
    </Stack>
    <Divider sx={{ mb: 2 }} />
    {children}
  </Paper>
);
