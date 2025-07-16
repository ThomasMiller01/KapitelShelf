import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import WarningRoundedIcon from "@mui/icons-material/WarningRounded";
import { Badge, Button, Grid, Stack, Tooltip, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement } from "react";

import { useMobile } from "../../hooks/useMobile";
import { onedriveApi } from "../../lib/api/KapitelShelf.Api";
import type { CloudStorageDTO } from "../../lib/api/KapitelShelf.Api/api";
import { IconButtonWithTooltip } from "../base/IconButtonWithTooltip";
import { Property } from "../base/Property";
import { CloudStorageIcon } from "./CloudStorageIcon";

interface CloudStorageCardProps {
  cloudstorage: CloudStorageDTO;
}

export const CloudStorageCard = ({
  cloudstorage,
}: CloudStorageCardProps): ReactElement => {
  const { isMobile } = useMobile();

  const { mutate: startOAuthFlow } = useMutation({
    mutationKey: ["cloudstorage-onedrive-re-oauth-flow"],
    mutationFn: async () => {
      const { data } = await onedriveApi.cloudstorageOnedriveOauthGet(
        window.location.href
      );
      window.location.href = data;
    },
  });

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
        <CloudStorageIcon
          type={cloudstorage.type}
          disabled={cloudstorage.needsReAuthentication}
          sx={{ mx: isMobile ? "5px" : "15px" }}
        />
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
          <IconButtonWithTooltip tooltip="Delete">
            <DeleteIcon />
          </IconButtonWithTooltip>
        </Stack>
      </Grid>
    </Grid>
  );
};

interface CloudStorageDirectoryProps {
  directory: string | undefined | null;
  needsReAuthentication: boolean | undefined;
}

const CloudStorageDirectory: React.FC<CloudStorageDirectoryProps> = ({
  directory,
  needsReAuthentication,
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
            disabled={needsReAuthentication}
          >
            Configure
          </Button>
        </Badge>
      </Tooltip>
    );
  }

  return (
    <Stack direction="row" spacing={1}>
      <Typography sx={{ opacity: needsReAuthentication ? 0.5 : 1 }}>
        {directory}
      </Typography>
      <IconButtonWithTooltip
        tooltip="Change Directory"
        disabled={needsReAuthentication}
      >
        <EditIcon />
      </IconButtonWithTooltip>
    </Stack>
  );
};
