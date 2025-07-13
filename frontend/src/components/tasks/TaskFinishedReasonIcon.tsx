import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import type { SvgIconOwnProps } from "@mui/material";

import { FinishedReasonNullable } from "../../lib/api/KapitelShelf.Api/api";

interface TaskFinishedReasonIconProps extends SvgIconOwnProps {
  reason: FinishedReasonNullable | undefined;
}

export const TaskFinishedReasonIcon: React.FC<TaskFinishedReasonIconProps> = ({
  reason,
  ...props
}) => {
  switch (reason) {
    case FinishedReasonNullable.NUMBER_0:
      return <CheckCircleOutlineIcon {...props} color="success" />;
    case FinishedReasonNullable.NUMBER_1:
      return <ErrorOutlineIcon {...props} color="error" />;
    default:
      return <></>;
  }
};
