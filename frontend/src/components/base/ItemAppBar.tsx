import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { AppBar, IconButton, Toolbar, Typography } from "@mui/material";
import type { ReactElement, ReactNode } from "react";
import { useNavigate } from "react-router-dom";

interface ItemAppBarProps {
  title: string | null | undefined;
  addons?: ReactNode[];
}

const ItemAppBar = ({ title, addons = [] }: ItemAppBarProps): ReactElement => {
  const navigate = useNavigate();

  return (
    <AppBar position="static" color="default" elevation={1}>
      <Toolbar>
        <IconButton
          edge="start"
          onClick={() => navigate(-1)}
          color="inherit"
          aria-label="back"
        >
          <ArrowBackIcon />
        </IconButton>
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
};

export default ItemAppBar;
