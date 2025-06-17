import { Box, Grid } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";

import { useMobile } from "../../hooks/useMobile";
import { searchApi } from "../../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import BookCard from "../BookCard";

// 600ms after user stops typing
const SEARCHTERM_REST_MS = 600;

interface SearchSuggestionsProps {
  searchterm: string;
}

export const SearchSuggestions = ({
  searchterm,
}: SearchSuggestionsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { mutateAsync: mutateGetSearchSuggestions } = useMutation({
    mutationKey: ["search-suggestions", searchterm],
    mutationFn: async (term: string) => {
      if (term === "") {
        return [];
      }

      const { data } = await searchApi.searchSuggestionsGet(term);
      return data;
    },
  });

  const [suggestions, setSuggestions] = useState<BookDTO[]>([]);
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
        padding: "15px",
      }}
    >
      <Grid container spacing={2}>
        {suggestions.map((suggestion) => (
          <Grid
            key={suggestion.id}
            size={{ xs: 6, sm: 12, md: 6, lg: 4, xl: 3 }}
          >
            <BookCard
              book={suggestion}
              showAuthor
              itemVariant={isMobile ? "normal" : "detailed"}
            />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
