import SearchIcon from "@mui/icons-material/Search";
import { Box, IconButton, InputAdornment, TextField } from "@mui/material";
import { type ReactElement, useState } from "react";

import { SearchSuggestions } from "./SearchSuggestions";

export const SearchBar = (): ReactElement => {
  const [searchterm, setSearchterm] = useState("");

  return (
    <Box sx={{ width: "100%", position: "relative" }}>
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
      <SearchSuggestions searchterm={searchterm} />
    </Box>
  );
};
