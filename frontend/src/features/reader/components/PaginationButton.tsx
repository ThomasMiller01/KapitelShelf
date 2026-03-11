import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { IconButton } from "@mui/material";
import React from "react";

import { useMobile } from "../../../hooks/useMobile";

interface PaginationButtonProps {
  onClick: () => void;
  disabled: boolean;
  direction: "prev" | "next";
}

export const PaginationButton: React.FC<PaginationButtonProps> = ({
  onClick,
  disabled,
  direction,
}) => {
  const { isMobile } = useMobile();

  const Icon = direction === "prev" ? NavigateBeforeIcon : NavigateNextIcon;

  return (
    <IconButton
      onClick={onClick}
      disabled={disabled}
      size="large"
      disableRipple
      sx={{
        borderTopLeftRadius: direction === "prev" ? "50px" : "10px",
        borderBottomLeftRadius: direction === "prev" ? "50px" : "10px",
        borderTopRightRadius: direction === "prev" ? "10px" : "50px",
        borderBottomRightRadius: direction === "prev" ? "10px" : "50px",
        width: "150px",
        height: "100%",
        "&:hover": {
          backgroundColor: "transparent",
        },
        ...(isMobile && {
          position: "absolute",
          left: direction === "prev" ? 5 : "auto",
          right: direction === "next" ? 5 : "auto",
          zIndex: 1,
          touchAction: "pan-y",
        }),
      }}
    >
      {!isMobile && <Icon fontSize="large" />}
    </IconButton>
  );
};
