import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import type { TooltipProps } from "@mui/material";
import {
  AppBar,
  IconButton,
  Stack,
  Toolbar,
  Tooltip,
  tooltipClasses,
  Typography,
} from "@mui/material";
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
  // Defines the back button action:
  // - undefined: go back in the navigation history
  // - string: go to the defined url
  backUrl?: string | undefined;
}

const BackButton = ({
  backTooltip,
  backUrl,
}: BackButtonProps): ReactElement => {
  const navigate = useNavigate();

  let backAction: () => void;
  if (backUrl === undefined) {
    // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
    backAction = () => navigate(-1);
  } else {
    // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
    backAction = () => navigate(backUrl);
  }

  const button = (
    <IconButton edge="start" onClick={backAction} color="inherit">
      <ArrowBackIcon sx={{ fontSize: "1.8rem" }} />
    </IconButton>
  );

  return backTooltip ? (
    <BackButtonTooltip title={backTooltip}>{button}</BackButtonTooltip>
  ) : (
    button
  );
};

interface ItemAppBarProps extends BackButtonProps {
  title?: string | null | undefined;
  addons?: ReactNode[];
  actions?: ReactNode[];
}

const ItemAppBar = ({
  title,
  addons = [],
  actions = [],
  backTooltip = "Go back",
  backUrl = undefined,
}: ItemAppBarProps): ReactElement => (
  <AppBar
    position="static"
    elevation={1}
    sx={{ bgcolor: "background.paper", color: "text.primary" }}
  >
    <Toolbar sx={{ py: "8px" }}>
      <BackButton backTooltip={backTooltip} backUrl={backUrl} />
      <Stack direction={{ xs: "column", md: "row" }} spacing={1} width="100%">
        <Typography
          variant="h6"
          noWrap={false}
          sx={{
            textAlign: "left",
            alignContent: "center",
            ml: "10px !important",
            mr: "10px !important",
            // dont wrap title too early
            flexShrink: 1, // allow it to shrink below its content width
            flexGrow: 0, // don’t force it to take extra space
            minWidth: 0,
          }}
        >
          {title}
        </Typography>
        <Stack
          direction="row"
          spacing={2}
          justifyContent="space-between"
          alignItems="center"
          sx={{
            flexGrow: 1, // fill remaining space
            flexShrink: 1, // allow it to shrink if the title needs more
            minWidth: 0,
          }}
        >
          <Stack direction="row" spacing={2} alignItems="center">
            {addons}
          </Stack>
          <Stack
            direction="row"
            spacing={2}
            alignItems="center"
            justifyContent={"end"}
          >
            {actions}
          </Stack>
        </Stack>
      </Stack>
    </Toolbar>
  </AppBar>
);

export default ItemAppBar;
