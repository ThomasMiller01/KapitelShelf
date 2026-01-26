import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import SearchIcon from "@mui/icons-material/Search";
import SyncIcon from "@mui/icons-material/Sync";
import WarningRoundedIcon from "@mui/icons-material/WarningRounded";
import type { BadgeProps } from "@mui/material";
import {
  Badge,
  Button,
  Grid,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
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
import type { CloudStorageDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useConfigureCloudDirectory } from "../../lib/requests/cloudstorages/useConfigureCloudDirectory";
import { useDeleteStorage } from "../../lib/requests/cloudstorages/useDeleteStorage";
import { useScanStorage } from "../../lib/requests/cloudstorages/useScanStorage";
import { useSyncStorage } from "../../lib/requests/cloudstorages/useSyncStorage";
import { CloudTypeToString } from "../../utils/CloudStorageUtils";
import ConfirmDialog from "../base/feedback/ConfirmDialog";
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
    options?: RawAxiosRequestConfig,
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
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);

  const { mutate: startOAuthFlow } = useMutation({
    mutationFn: async () => {
      const { data } = await getOAuthUrl(window.location.href);
      window.location.href = data;
    },
  });

  const { mutate: configureDirectory } =
    useConfigureCloudDirectory(cloudstorage);

  const { mutate: deleteStorage } = useDeleteStorage(update);

  const { mutate: syncStorage } = useSyncStorage(() => {
    update();
    triggerNavigate({
      operation: `Started Storage Sync`,
      itemName: CloudTypeToString(cloudstorage.type),
      url: "/settings/tasks",
    });
  });

  const { mutate: scanStorage } = useScanStorage(() => {
    update();
    triggerNavigate({
      operation: `Started Storage Scan`,
      itemName: CloudTypeToString(cloudstorage.type),
      url: "/settings/tasks",
    });
  });

  const onConfigureDirectory = (directory: string): void => {
    configureDirectory(directory);
    update();
    triggerNavigate({
      operation: `Downloading from ${CloudTypeToString(cloudstorage.type)}`,
      itemName: directory,
      url: "/settings/tasks",
    });
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
        <Stack spacing={1} direction="row" alignItems="start">
          <IconButtonWithTooltip
            tooltip="Delete"
            onClick={() => setOpenDeleteDialog(true)}
          >
            <DeleteIcon />
          </IconButtonWithTooltip>
          <OptionsMenu
            onSyncClick={() => syncStorage(cloudstorage.id)}
            onScanClick={() => scanStorage(cloudstorage.id)}
          />
        </Stack>
      </Grid>

      <ConfigureCloudDirectoryDialog
        storageId={cloudstorage.id}
        open={openDirectoryDialog}
        onCancel={() => setOpenDirectoryDialog(false)}
        onConfirm={onConfigureDirectory}
        cloudType={cloudstorage.type}
      />
      <ConfirmDialog
        open={openDeleteDialog}
        onCancel={() => setOpenDeleteDialog(false)}
        onConfirm={() => deleteStorage(cloudstorage.id)}
        title="Confirm to delete this cloud storage"
        description="Are you sure you want to delete this cloud storage? This action cannot be undone."
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

interface OptionsMenuProps {
  onSyncClick: () => void;
  onScanClick: () => void;
}

const OptionsMenu = ({
  onSyncClick,
  onScanClick,
}: OptionsMenuProps): ReactElement => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = (): void => {
    setAnchorEl(null);
  };

  return (
    <>
      <IconButton onClick={handleClick}>
        <MoreVertIcon />
      </IconButton>
      <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
        <OptionMenuItem
          text="Sync with Cloud"
          icon={<SyncIcon />}
          onClick={() => {
            onSyncClick();
            handleClose();
          }}
        />
        <OptionMenuItem
          text="Scan for Books"
          icon={<SearchIcon />}
          onClick={() => {
            onScanClick();
            handleClose();
          }}
        />
      </Menu>
    </>
  );
};

interface OptionMenuItemProps {
  text: string;
  icon?: ReactElement;
  onClick: () => void;
}

const OptionMenuItem = ({
  text,
  icon,
  onClick,
}: OptionMenuItemProps): ReactElement => (
  <MenuItem onClick={onClick}>
    {icon && <ListItemIcon>{icon}</ListItemIcon>}
    <ListItemText>{text}</ListItemText>
  </MenuItem>
);
