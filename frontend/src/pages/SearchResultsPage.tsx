import { Box } from "@mui/material";
import type { ReactElement } from "react";
import { useSearchParams } from "react-router-dom";

import { SearchResults } from "../features/search";
import ItemAppBar from "../shared/components/base/ItemAppBar";

const SearchResultsPage = (): ReactElement => {
  const [searchParams] = useSearchParams();
  const searchterm = searchParams.get("q");

  return (
    <Box>
      <ItemAppBar title={`Search results for "${searchterm}"`} />
      <Box padding="24px">
        <SearchResults searchterm={searchterm ?? ""} />
      </Box>
    </Box>
  );
};

export default SearchResultsPage;
