import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import type { TooltipProps } from "@mui/material";
import {
  AppBar,
  IconButton,
  Stack,
  Toolbar,
  tooltipClasses,
  Typography,
} from "@mui/material";
import { Tooltip } from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement, ReactNode } from "react";
import { useNavigate } from "react-router-dom";

import { useMobile } from "../../hooks/useMobile";

const BackButtonTooltip = styled((props: TooltipProps) => (
  <Tooltip {...props} classes={{ popper: props.className }} />
))(() => ({
  [`& .${tooltipClasses.tooltip}`]: {
    fontSize: "0.95rem",
  },
}));

interface BackButtonProps {
  backTooltip?: string;
}

const BackButton = ({ backTooltip }: BackButtonProps): ReactElement => {
  const navigate = useNavigate();

  const button = (
    <IconButton edge="start" onClick={() => navigate(-1)} color="inherit">
      <ArrowBackIcon sx={{ fontSize: "1.8rem" }} />
    </IconButton>
  );

  return backTooltip ? (
    <BackButtonTooltip title={backTooltip}>{button}</BackButtonTooltip>
  ) : (
    button
  );
};

interface ItemAppBarProps {
  title: string | null | undefined;
  addons?: ReactNode[];
}

const ItemAppBar = ({ title, addons = [] }: ItemAppBarProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <AppBar position="static" color="default" elevation={1}>
      <Toolbar sx={{ py: "8px" }}>
        <BackButton backTooltip={"Go back"} />
        <Stack direction={{ xs: "column", md: "row" }} spacing={1} width="100%">
          <Typography
            variant="h6"
            sx={{
              textAlign: "left",
              ml: "10px !important",
              mr: "10px !important",
              width: "fit-content",
            }}
          >
            {title}
          </Typography>
          <Stack
            direction="row"
            spacing={2}
            justifyContent={isMobile ? "end" : "start"}
          >
            {addons}
          </Stack>
        </Stack>
      </Toolbar>
    </AppBar>
  );
};

export default ItemAppBar;
