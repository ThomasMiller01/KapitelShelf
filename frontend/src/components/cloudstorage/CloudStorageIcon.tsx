import CloudIcon from "@mui/icons-material/Cloud";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";

import { CloudType } from "../../lib/api/KapitelShelf.Api/api";

interface CloudStorageIconProps extends SvgIconOwnProps {
  type: CloudType | undefined;
  disabled?: boolean;
}

export const CloudStorageIcon: React.FC<CloudStorageIconProps> = ({
  type,
  disabled = false,
  sx,
  ...props
}) => {
  switch (type) {
    case CloudType.NUMBER_0:
      return (
        <Tooltip title="OneDrive">
          <CloudIcon {...props} sx={{ opacity: disabled ? 0.5 : 1, ...sx }} />
        </Tooltip>
      );
    default:
      return <></>;
  }
};
