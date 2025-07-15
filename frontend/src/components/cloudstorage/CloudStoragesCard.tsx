import DeleteIcon from "@mui/icons-material/Delete";
import ReportIcon from "@mui/icons-material/Report";
import { Button, Grid, Stack, Typography } from "@mui/material";
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
      rowSpacing={0.5}
      columnSpacing={3}
      alignItems="center"
      sx={{
        minHeight: 56,
        px: 2,
        py: 1,
        borderRadius: 2,
        bgcolor: "background.paper",
        boxShadow: 1,
        width: "100%",
      }}
    >
      {/* Type */}
      <Grid>
        <CloudStorageIcon
          type={cloudstorage.type}
          disabled={cloudstorage.needsReAuthentication}
          sx={{ mx: isMobile ? "0" : "15px" }}
        />
      </Grid>

      {/* Re-Authenticate */}
      {cloudstorage.needsReAuthentication && (
        <Grid>
          <Button
            variant="contained"
            color="warning"
            startIcon={<ReportIcon />}
            onClick={() => startOAuthFlow()}
            sx={{ mb: "15px" }}
          >
            Re-Authenticate
          </Button>
        </Grid>
      )}

      {/* Directory */}
      {cloudstorage.cloudDirectory && (
        <Grid>
          <Property
            label="Directory"
            disabled={cloudstorage.needsReAuthentication}
          >
            {cloudstorage.cloudDirectory}
          </Property>
        </Grid>
      )}

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
