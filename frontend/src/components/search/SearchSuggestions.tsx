import { Box, Grid } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";

import { useMobile } from "../../hooks/useMobile";
import { booksApi } from "../../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import { NoItemsFoundCard } from "../base/feedback/NoItemsFoundCard";
import BookCard from "../BookCard";

// 600ms after user stops typing
const SEARCHTERM_REST_MS = 600;

interface SearchSuggestionsProps {
  searchterm: string;
  onClick?: () => void;
}

export const SearchSuggestions = ({
  searchterm,
  onClick,
}: SearchSuggestionsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { mutateAsync: mutateGetSearchSuggestions, isSuccess } = useMutation({
    mutationKey: ["search-suggestions", searchterm],
    mutationFn: async (term: string) => {
      if (term === "") {
        return [];
      }

      const { data } = await booksApi.booksSearchSuggestionsGet(term);
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

  if (searchterm !== "" && suggestions.length === 0 && isSuccess) {
    return <NoItemsFoundCard itemName="Books" small />;
  }

  if (searchterm === "" || suggestions.length === 0) {
    return <></>;
  }

  return (
    <Box
      sx={{
        padding: "15px",
        paddingBottom: "10px",
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
              onClick={onClick}
            />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
