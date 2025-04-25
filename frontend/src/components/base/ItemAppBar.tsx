import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import type { TooltipProps } from "@mui/material";
import {
  AppBar,
  IconButton,
  Toolbar,
  tooltipClasses,
  Typography,
} from "@mui/material";
import { Tooltip } from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement, ReactNode } from "react";
import { useNavigate } from "react-router-dom";

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
    <IconButton
      edge="start"
      onClick={() => navigate(-1)}
      color="inherit"
      aria-label="back"
    >
      <ArrowBackIcon />
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
  backTooltip?: string;
  addons?: ReactNode[];
}

const ItemAppBar = ({
  title,
  backTooltip,
  addons = [],
}: ItemAppBarProps): ReactElement => (
  <AppBar position="static" color="default" elevation={1}>
    <Toolbar>
      <BackButton backTooltip={backTooltip} />
      <Typography
        variant="h6"
        noWrap
        sx={{
          textAlign: "left",
          marginLeft: "20px",
          marginRight: "20px",
          width: "fit-content",
        }}
      >
        {title}
      </Typography>
      {addons}
    </Toolbar>
  </AppBar>
);

export default ItemAppBar;
