import CreateNewFolderIcon from "@mui/icons-material/CreateNewFolder";
import TuneIcon from "@mui/icons-material/Tune";
import { Button, Divider, Paper, Stack, Typography } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { CloudStorageIcon } from "../../components/cloudstorage/CloudStorageIcon";
import { CloudStorageCard } from "../../components/cloudstorage/CloudStoragesCard";
import { ConfigureCloudConfigurationDialog } from "../../components/cloudstorage/ConfigureCloudConfiguration/ConfigureCloudConfigurationDialog";
import { onedriveApi } from "../../lib/api/KapitelShelf.Api";
import type { ConfigureCloudDTO } from "../../lib/api/KapitelShelf.Api/api";
import { CloudType } from "../../lib/api/KapitelShelf.Api/api";

export const OneDriveSettings = (): ReactElement => {
  const [configureDialogOpen, setConfigureDialogOpen] = useState(false);

  const {
    data: isConfigured,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["cloudstorage-onedrive-isconfigured"],
    queryFn: async () => {
      const { data } = await onedriveApi.cloudstorageOnedriveIsconfiguredGet();
      return data;
    },
  });

  const { data: cloudstorages } = useQuery({
    queryKey: ["cloudstorage-onedrive-list-cloudstorages"],
    queryFn: async () => {
      const { data } = await onedriveApi.cloudstorageOnedriveGet();
      return data;
    },
  });

  const { mutate: startOAuthFlow } = useMutation({
    mutationKey: ["cloudstorage-onedrive-oauth-flow"],
    mutationFn: async () => {
      const { data } = await onedriveApi.cloudstorageOnedriveOauthGet(
        window.location.href
      );
      window.location.href = data;
    },
  });

  const { mutate: configure } = useMutation({
    mutationKey: ["cloudstorage-onedrive-configure"],
    mutationFn: async (configuration: ConfigureCloudDTO) => {
      await onedriveApi.cloudstorageOnedriveConfigurePut(configuration);
      refetch();
    },
  });

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
          cloudType={CloudType.NUMBER_0}
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
          <CloudStorageCard key={cloudstorage.id} cloudstorage={cloudstorage} />
        ))}
      </Stack>
      <ConfigureCloudConfigurationDialog
        open={configureDialogOpen}
        onCancel={() => setConfigureDialogOpen(false)}
        onConfirm={(configuration: ConfigureCloudDTO) => {
          configure(configuration);
          setConfigureDialogOpen(false);
        }}
        cloudType={CloudType.NUMBER_0}
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
        type={CloudType.NUMBER_0}
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
