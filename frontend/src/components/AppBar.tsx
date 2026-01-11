import AddBoxIcon from "@mui/icons-material/AddBox";
import DarkModeIcon from "@mui/icons-material/DarkMode";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import LightModeIcon from "@mui/icons-material/LightMode";
import NoteAddIcon from "@mui/icons-material/NoteAdd";
import {
  Box,
  IconButton,
  SpeedDial,
  SpeedDialAction,
  Stack,
  useColorScheme,
} from "@mui/material";
import { type ReactElement, useState } from "react";
import { useNavigate } from "react-router-dom";

import { ProfileMenu } from "../features/user/ProfileMenu";
import { useMobile } from "../hooks/useMobile";
import { ResponsiveDrawerAppBar } from "./base/ResponsiveDrawer";
import { SearchBar } from "./search/SearchBar";

interface TopAppBarProps {
  open: boolean;
  toggle: () => void;
}

export const AppBar = ({ open, toggle }: TopAppBarProps): ReactElement => {
  const { isMobile } = useMobile();
  const { mode, systemMode, setMode } = useColorScheme();

  if (!mode) {
    // mode is undefined on first render
    return <></>;
  }

  const currentMode = mode === "system" ? systemMode : mode;

  return (
    <ResponsiveDrawerAppBar open={open} toggle={toggle}>
      <Box
        sx={{
          width: "100%",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <SearchBar />
        <Stack
          direction="row"
          spacing={{ md: 2 }}
          alignItems="center"
          ml={isMobile ? "15px" : "30px"}
        >
          <AddBookActions />

          {/* Color mode button not shown on mobile, instead part of user context menu */}
          {!isMobile && (
            <IconButton
              onClick={() => setMode(currentMode === "dark" ? "light" : "dark")}
            >
              {currentMode === "dark" ? <LightModeIcon /> : <DarkModeIcon />}
            </IconButton>
          )}
          <ProfileMenu />
        </Stack>
      </Box>
    </ResponsiveDrawerAppBar>
  );
};

const AddBookActions = (): ReactElement => {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);

  return (
    <SpeedDial
      ariaLabel="Add Book"
      direction="down"
      icon={<AddBoxIcon />}
      open={open}
      onClose={() => setOpen(false)}
      onOpen={() => setOpen(true)}
      sx={{
        height: "36px",
        ">.MuiButtonBase-root": {
          width: "36px",
          height: "36px",
          bgcolor: "transparent",
          boxShadow: "none",
          color: "text.primary",
        },
        "& .MuiSpeedDial-actions": {
          position: "absolute",
          top: "40px",
          "& .MuiSpeedDialAction-fab": {
            width: "40px",
            height: "40px",
            color: "text.primary",
          },
        },
      }}
    >
      <SpeedDialAction
        icon={<NoteAddIcon />}
        slotProps={{ tooltip: { title: "Create Book" } }}
        onClick={() => {
          setOpen(false);
          navigate("/library/books/create");
        }}
      />
      <SpeedDialAction
        icon={<FileUploadIcon />}
        slotProps={{ tooltip: { title: "Import Book" } }}
        onClick={() => {
          setOpen(false);
          navigate("/library/books/import");
        }}
      />
    </SpeedDial>
  );
};
