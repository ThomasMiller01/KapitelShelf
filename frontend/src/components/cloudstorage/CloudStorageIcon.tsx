import CloudIcon from "@mui/icons-material/Cloud";
import type { SvgIconOwnProps } from "@mui/material";
import { Tooltip } from "@mui/material";

import { CloudTypeDTO } from "../../lib/api/KapitelShelf.Api/api";
import { CloudTypeToString } from "../../utils/CloudStorageUtils";

interface CloudStorageIconProps extends SvgIconOwnProps {
  type: CloudTypeDTO | undefined;
  disabled?: boolean;
}

export const CloudStorageIcon: React.FC<CloudStorageIconProps> = ({
  type,
  disabled = false,
  sx,
  ...props
}) => {
  switch (type) {
    case CloudTypeDTO.NUMBER_0:
      return (
        <Tooltip title={CloudTypeToString(CloudTypeDTO.NUMBER_0)}>
          <CloudIcon {...props} sx={{ opacity: disabled ? 0.5 : 1, ...sx }} />
        </Tooltip>
      );
    default:
      return <></>;
  }
};
