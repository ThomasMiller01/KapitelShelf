import { Box, Stack } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";

import { searchApi } from "../../lib/api/KapitelShelf.Api";

// 600ms after user stops typing
const SEARCHTERM_REST_MS = 600;

interface SearchSuggestionsProps {
  searchterm: string;
}

export const SearchSuggestions = ({
  searchterm,
}: SearchSuggestionsProps): ReactElement => {
  const { mutateAsync: mutateGetSearchSuggestions } = useMutation({
    mutationKey: ["search-suggestions", searchterm],
    mutationFn: async (st: string) => {
      if (st === "") {
        return [];
      }

      const { data } = await searchApi.searchSuggestionsGet(st);
      return data;
    },
  });

  const [suggestions, setSuggestions] = useState<string[]>([]);
  useEffect(() => {
    const handle = setTimeout(
      () =>
        mutateGetSearchSuggestions(searchterm).then((response) =>
          setSuggestions(response)
        ),
      SEARCHTERM_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [searchterm, mutateGetSearchSuggestions]);

  if (searchterm === "" || suggestions.length === 0) {
    return <></>;
  }

  return (
    <Box
      sx={{
        position: "absolute",
        bgcolor: "background.paper",
      }}
    >
      <Stack>
        {suggestions.map((suggestion) => (
          <Box>{suggestion}</Box>
        ))}
      </Stack>
    </Box>
  );
};
