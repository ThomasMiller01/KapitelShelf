import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import DownloadingIcon from "@mui/icons-material/Downloading";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";

import { useEffect, useState } from "react";
import type { CloudStorageDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useStorageDownloadStatus } from "../../lib/requests/cloudstorages/useStorageDownloadStatus";

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

  const { data: isDownloaded } = useStorageDownloadStatus(
    cloudstorage.id,
    cloudstorage.cloudDirectory !== null && !downloadStatus
  );

  // sync isDownloaded with parent, when the change directory button is clicked e.g.
  useEffect(() => {
    if (isDownloaded === undefined || isDownloaded === null) {
      return;
    }

    setDownloadStatus(isDownloaded);
  }, [isDownloaded]);

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
