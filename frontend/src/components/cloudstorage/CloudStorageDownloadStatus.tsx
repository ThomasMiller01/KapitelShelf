import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import DownloadingIcon from "@mui/icons-material/Downloading";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";

import { cloudstorageApi } from "../../lib/api/KapitelShelf.Api";
import type { CloudStorageDTO } from "../../lib/api/KapitelShelf.Api/api";

interface CloudStorageDownloadStatusProps extends SvgIconOwnProps {
  cloudstorage: CloudStorageDTO;
}

export const CloudStorageDownloadStatus: React.FC<
  CloudStorageDownloadStatusProps
> = ({ cloudstorage }) => {
  const [downloadStatus, setDownloadStatus] = useState(
    cloudstorage.isDownloaded
  );

  // sync isDownloaded with parent, when the change directory button is clicked e.g.
  useEffect(() => {
    setDownloadStatus(cloudstorage.isDownloaded);
  }, [cloudstorage.isDownloaded]);

  useQuery({
    queryKey: ["cloudstorage-update-download-status", cloudstorage.id],
    queryFn: async () => {
      if (cloudstorage.id === undefined) {
        return null;
      }
      const { data } = await cloudstorageApi.cloudstorageStoragesStorageIdGet(
        cloudstorage.id
      );

      setDownloadStatus(data.isDownloaded);

      return null;
    },
    enabled: cloudstorage.cloudDirectory !== null && !downloadStatus, // start/stop polling
    refetchInterval: 5000,
  });

  // download could not be started yet, first the cloud directory has to be set
  if (cloudstorage.cloudDirectory === null) {
    return <></>;
  }

  if (downloadStatus) {
    return (
      <Tooltip title="Downloaded">
        <CheckCircleOutlineIcon
          color="success"
          fontSize="small"
          sx={{ bgcolor: "background.paper", borderRadius: "15px" }}
        />
      </Tooltip>
    );
  }

  return (
    <Tooltip title="Downloading ...">
      <DownloadingIcon
        color="info"
        fontSize="small"
        sx={{ bgcolor: "background.paper", borderRadius: "15px" }}
      />
    </Tooltip>
  );
};
