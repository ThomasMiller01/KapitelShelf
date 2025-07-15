import CreateNewFolderIcon from "@mui/icons-material/CreateNewFolder";
import { Box, Button, Divider, Paper, Stack, Typography } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import { CloudStorageCard } from "../../components/cloudstorage/CloudStoragesCard";
import { onedriveApi } from "../../lib/api/KapitelShelf.Api";

export const OneDriveSettings = (): ReactElement => {
  const { data: isConfigured } = useQuery({
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

  if (!isConfigured) {
    return (
      <OneDriveSettingsLayout>
        <Box>TODO Configure OneDrive</Box>
      </OneDriveSettingsLayout>
    );
  }

  return (
    <OneDriveSettingsLayout>
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
    </OneDriveSettingsLayout>
  );
};

interface OneDriveSettingsLayoutProps {
  children: ReactElement | ReactElement[];
}

const OneDriveSettingsLayout: React.FC<OneDriveSettingsLayoutProps> = ({
  children,
}) => (
  <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
    <Typography variant="h6">OneDrive</Typography>
    <Divider sx={{ mb: 2 }} />
    {children}
  </Paper>
);
