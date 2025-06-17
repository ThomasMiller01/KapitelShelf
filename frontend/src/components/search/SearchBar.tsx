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

import { useMobile } from "../../hooks/useMobile";
import { SearchSuggestions } from "./SearchSuggestions";

export const SearchBar = (): ReactElement => {
  const { isMobile } = useMobile();

  const suggestionsAnchorRef = useRef<HTMLInputElement>(null);
  const [suggestionsOpen, setSuggestionsOpen] = useState(false);

  const handleSuggestionsClose = (): void => {
    setSuggestionsOpen(false);
  };

  const [searchterm, setSearchterm] = useState("");

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
            label="Search"
            variant="outlined"
            size="small"
            fullWidth
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton edge="end">
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
          }}
        >
          <Paper sx={{ width: "100%" }}>
            <SearchSuggestions searchterm={searchterm} />
          </Paper>
        </Popper>
      </Box>
    </ClickAwayListener>
  );
};
