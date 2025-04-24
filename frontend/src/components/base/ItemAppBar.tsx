import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { AppBar, IconButton, Toolbar, Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

interface ItemAppBarProps {
  title: string | null | undefined;
}

const ItemAppBar = ({ title }: ItemAppBarProps): ReactElement => {
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
          sx={{ flexGrow: 1, textAlign: "left", marginLeft: "20px" }}
        >
          {title}
        </Typography>
      </Toolbar>
    </AppBar>
  );
};

export default ItemAppBar;
