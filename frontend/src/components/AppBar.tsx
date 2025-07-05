import AddBoxIcon from "@mui/icons-material/AddBox";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box, SpeedDial, SpeedDialAction, Stack } from "@mui/material";
import type { ReactElement } from "react";
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
          spacing={{ xs: 2, md: 3 }}
          alignItems="center"
          ml={isMobile ? "15px" : "30px"}
        >
          <AddBookActions />
          {/* <Box ml={isMobile ? "10px !important" : ""}>
            <ThemeToggle />
          </Box> */}
          <ProfileMenu />
        </Stack>
      </Box>
    </ResponsiveDrawerAppBar>
  );
};

const AddBookActions = (): ReactElement => {
  const navigate = useNavigate();

  return (
    <SpeedDial
      ariaLabel="Add Book"
      direction="down"
      icon={<AddBoxIcon />}
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
        onClick={() => navigate("/library/books/create")}
      />
      <SpeedDialAction
        icon={<FileUploadIcon />}
        slotProps={{ tooltip: { title: "Import Book" } }}
        onClick={() => navigate("/library/books/import")}
      />
    </SpeedDial>
  );
};
