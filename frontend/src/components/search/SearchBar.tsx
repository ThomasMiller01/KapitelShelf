import SearchIcon from "@mui/icons-material/Search";
import {
  Box,
  ClickAwayListener,
  IconButton,
  InputAdornment,
  Paper,
  Popper,
  TextField,
} from "@mui/material";
import { type ReactElement, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useMobile } from "../../hooks/useMobile";
import { SearchSuggestions } from "./SearchSuggestions";

export const SearchBar = (): ReactElement => {
  const { isMobile } = useMobile();
  const navigate = useNavigate();

  const [searchterm, setSearchterm] = useState("");

  const suggestionsAnchorRef = useRef<HTMLInputElement>(null);
  const [suggestionsOpen, setSuggestionsOpen] = useState(false);

  const handleSuggestionsClose = (): void => {
    setSuggestionsOpen(false);
  };

  const handleSuggestionsClick = (): void => {
    handleSuggestionsClose();
    setSearchterm("");
  };

  const handleSearchClick = (): void => {
    handleSuggestionsClose();
    setSearchterm("");

    navigate(`/search?q=${searchterm}`);
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent): void => {
    if (e.key === "Enter") {
      handleSearchClick();
    }
  };

  return (
    <ClickAwayListener onClickAway={handleSuggestionsClose}>
      <Box sx={{ width: "100%" }}>
        <Box
          sx={{
            width: "100%",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <TextField
            value={searchterm}
            onChange={(e) => setSearchterm(e.target.value)}
            inputRef={suggestionsAnchorRef}
            onFocus={() => setSuggestionsOpen(true)}
            onKeyDown={handleSearchKeyDown}
            label="Search"
            variant="outlined"
            size="small"
            autoComplete="off"
            fullWidth
            slotProps={{
              input: {
                autoComplete: "off",
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton edge="end" onClick={handleSearchClick}>
                      <SearchIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              },
            }}
          />
        </Box>
        <Popper
          open={suggestionsOpen}
          anchorEl={suggestionsAnchorRef.current}
          placement="bottom-start"
          style={{
            zIndex: 1300,
            minWidth: "340px",
            width: suggestionsAnchorRef.current?.offsetWidth,
            maxWidth: "80%",
          }}
          sx={{
            marginLeft: isMobile ? "-10px !important" : "auto",
            maxHeight: "75%",
            overflowY: "auto",
            borderBottom: searchterm !== "" ? "5px solid" : "",
            borderBottomColor: "background.paper",
            borderRadius: "15px",
          }}
        >
          <Paper
            sx={{
              width: "100%",
              bgcolor: "background.default",
            }}
          >
            <SearchSuggestions
              searchterm={searchterm}
              onClick={handleSuggestionsClick}
            />
          </Paper>
        </Popper>
      </Box>
    </ClickAwayListener>
  );
};
