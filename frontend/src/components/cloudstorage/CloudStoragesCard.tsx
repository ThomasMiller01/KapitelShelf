import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import WarningRoundedIcon from "@mui/icons-material/WarningRounded";
import type { BadgeProps } from "@mui/material";
import {
  Badge,
  Button,
  Grid,
  Stack,
  styled,
  Tooltip,
  Typography,
} from "@mui/material";
import { shouldForwardProp } from "@mui/system";
import { useMutation } from "@tanstack/react-query";
import type { AxiosResponse, RawAxiosRequestConfig } from "axios";
import { type ReactElement, useState } from "react";

import { useMobile } from "../../hooks/useMobile";
import { useNotification } from "../../hooks/useNotification";
import { cloudstorageApi } from "../../lib/api/KapitelShelf.Api";
import type { CloudStorageDTO } from "../../lib/api/KapitelShelf.Api/api";
import { CloudTypeToString } from "../../utils/CloudStorageUtils";
import { IconButtonWithTooltip } from "../base/IconButtonWithTooltip";
import { Property } from "../base/Property";
import { CloudStorageDownloadStatus } from "./CloudStorageDownloadStatus";
import { CloudStorageIcon } from "./CloudStorageIcon";
import { ConfigureCloudDirectoryDialog } from "./ConfigureCloudDirectory/ConfigureCloudDirectoryDialog";

interface DownloadingBadgeProps extends BadgeProps {
  isMobile?: boolean;
}

const DownloadingBadge = styled(Badge, {
  shouldForwardProp: (prop) => prop !== "isMobile" && shouldForwardProp(prop),
})<DownloadingBadgeProps>(({ isMobile }) => ({
  "& .MuiBadge-badge": {
    right: isMobile ? 5 : 15,
    top: 10,
    padding: "0 4px",
  },
}));

interface CloudStorageCardProps {
  cloudstorage: CloudStorageDTO;
  getOAuthUrl: (
    redirectUrl?: string,
    options?: RawAxiosRequestConfig
  ) => Promise<AxiosResponse<string, any>>;
  update: () => void;
}

export const CloudStorageCard = ({
  cloudstorage,
  getOAuthUrl,
  update,
}: CloudStorageCardProps): ReactElement => {
  const { isMobile } = useMobile();
  const { triggerNavigate } = useNotification();

  const [openDirectoryDialog, setOpenDirectoryDialog] = useState(false);

  const { mutate: startOAuthFlow } = useMutation({
    mutationKey: ["cloudstorage-re-oauth-flow", cloudstorage.id],
    mutationFn: async () => {
      const { data } = await getOAuthUrl(window.location.href);
      window.location.href = data;
    },
  });

  const { mutate: configureDirectory } = useMutation({
    mutationKey: ["cloudstorage-configure-directory", cloudstorage.id],
    mutationFn: async (directory: string) => {
      if (cloudstorage.id === undefined) {
        return;
      }

      await cloudstorageApi.cloudstorageStoragesStorageIdConfigureDirectoryPut(
        cloudstorage.id,
        directory
      );

      update();

      triggerNavigate({
        operation: `Downloading from ${CloudTypeToString(cloudstorage.type)}`,
        itemName: directory,
        url: "/settings/tasks",
      });
    },
  });

  const { mutate: deleteStorage } = useMutation({
    mutationKey: ["cloudstorage-delete", cloudstorage.id],
    mutationFn: async () => {
      if (cloudstorage.id === undefined) {
        return;
      }

      await cloudstorageApi.cloudstorageStoragesStorageIdDelete(
        cloudstorage.id
      );

      update();
    },
  });

  const onConfigureDirectory = (directory: string): void => {
    configureDirectory(directory);
    setOpenDirectoryDialog(false);
  };

  return (
    <Grid
      container
      rowSpacing={1.5}
      columnSpacing={isMobile ? 3 : 4}
      alignItems="center"
      sx={{
        minHeight: 56,
        px: 2,
        py: 1,
        borderRadius: 2,
        bgcolor: "background.paper",
        boxShadow: 1,
        minWidth: isMobile ? "100%" : "600px",
        width: "fit-content",
      }}
    >
      {/* Type */}
      <Grid>
        <DownloadingBadgeComponent cloudstorage={cloudstorage}>
          <CloudStorageIcon
            type={cloudstorage.type}
            disabled={cloudstorage.needsReAuthentication}
            fontSize="large"
            sx={{ mx: isMobile ? "5px" : "15px" }}
          />
        </DownloadingBadgeComponent>
      </Grid>

      {/* Re-Authenticate */}
      {cloudstorage.needsReAuthentication && (
        <Grid>
          <Badge badgeContent={<WarningRoundedIcon color="primary" />}>
            <Button
              variant="contained"
              color="error"
              onClick={() => startOAuthFlow()}
            >
              Re-Authenticate
            </Button>
          </Badge>
        </Grid>
      )}

      {/* Directory */}
      <Grid>
        <Property
          label="Directory"
          disabled={cloudstorage.needsReAuthentication}
        >
          <CloudStorageDirectory
            directory={cloudstorage.cloudDirectory}
            needsReAuthentication={cloudstorage.needsReAuthentication}
            onConfigureDirectoryClick={() => setOpenDirectoryDialog(true)}
          />
        </Property>
      </Grid>

      {/* Owner */}
      <Grid>
        <Property label="Owner" disabled={cloudstorage.needsReAuthentication}>
          <Stack spacing={0.8} alignItems="start">
            <Typography variant="body1">
              {cloudstorage.cloudOwnerName}
            </Typography>
            <Typography
              variant="body2"
              color="text.secondary"
              mt="0 !important"
            >
              {cloudstorage.cloudOwnerEmail}
            </Typography>
          </Stack>
        </Property>
      </Grid>

      <Grid sx={{ flexGrow: 1 }}></Grid>

      {/* Operations */}
      <Grid>
        <Stack spacing={0.8} alignItems="start">
          <IconButtonWithTooltip
            tooltip="Delete"
            onClick={() => deleteStorage()}
          >
            <DeleteIcon />
          </IconButtonWithTooltip>
        </Stack>
      </Grid>

      <ConfigureCloudDirectoryDialog
        storageId={cloudstorage.id}
        open={openDirectoryDialog}
        onCancel={() => setOpenDirectoryDialog(false)}
        onConfirm={onConfigureDirectory}
        cloudType={cloudstorage.type}
      />
    </Grid>
  );
};

interface CloudStorageDirectoryProps {
  directory: string | undefined | null;
  needsReAuthentication: boolean | undefined;
  onConfigureDirectoryClick: () => void;
}

const CloudStorageDirectory: React.FC<CloudStorageDirectoryProps> = ({
  directory,
  needsReAuthentication,
  onConfigureDirectoryClick,
}) => {
  if (directory === undefined || directory === null) {
    return (
      <Tooltip title="Configure Directory">
        <Badge
          badgeContent={
            <WarningRoundedIcon
              color={needsReAuthentication ? "disabled" : "primary"}
            />
          }
        >
          <Button
            variant="contained"
            color="error"
            size="small"
            onClick={onConfigureDirectoryClick}
            disabled={needsReAuthentication}
          >
            Configure
          </Button>
        </Badge>
      </Tooltip>
    );
  }

  return (
    <Stack direction="row" spacing={1} alignItems="center">
      <Typography sx={{ opacity: needsReAuthentication ? 0.5 : 1 }}>
        {directory}
      </Typography>
      <IconButtonWithTooltip
        tooltip="Change Directory"
        onClick={onConfigureDirectoryClick}
        disabled={needsReAuthentication}
        size="small"
      >
        <EditIcon fontSize="small" />
      </IconButtonWithTooltip>
    </Stack>
  );
};

interface DownloadingBadgeComponentProps {
  cloudstorage: CloudStorageDTO;
  children: ReactElement | ReactElement[];
}

const DownloadingBadgeComponent: React.FC<DownloadingBadgeComponentProps> = ({
  cloudstorage,
  children,
}) => {
  const { isMobile } = useMobile();

  // download could not be started yet, first the cloud directory has to be set
  if (cloudstorage.cloudDirectory === null) {
    return children;
  }

  return (
    <DownloadingBadge
      badgeContent={<CloudStorageDownloadStatus cloudstorage={cloudstorage} />}
      isMobile={isMobile}
      anchorOrigin={{
        vertical: "bottom",
        horizontal: "right",
      }}
    >
      {children}
    </DownloadingBadge>
  );
};
