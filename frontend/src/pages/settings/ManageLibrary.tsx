import { Box, Tab, Tabs, Typography } from "@mui/material";
import { useMemo, type ReactElement } from "react";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import { ManageAuthorsList } from "../../features/authors";
import { ManageBooksList } from "../../features/book";
import { ManageCategoriesList } from "../../features/categories";
import { ManageSeriesList } from "../../features/series";
import { ManageTagsList } from "../../features/tags";
import { TabPanel } from "../../shared/components/base/TabPanel";

const TABS = [
  { label: "Series", value: "series" },
  { label: "Books", value: "books" },
  { label: "Authors", value: "authors" },
  { label: "Categories", value: "categories" },
  { label: "Tags", value: "tags" },
] as const;

export const ManageLibraryPage = (): ReactElement => {
  const navigate = useNavigate();
  const { section } = useParams();

  const activeTab = useMemo(
    () => TABS.findIndex((t) => t.value === section),
    [section],
  );

  const handleTabChange = (_: unknown, index: number) => {
    navigate(`/settings/manage-library/${TABS[index].value}`);
  };

  if (activeTab === -1) {
    return <Navigate to="/settings/manage-library/series" replace />;
  }

  return (
    <Box padding="20px">
      <Typography variant="h5">Manage your Library</Typography>
      <Box sx={{ my: 2 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab label="Series" />
          <Tab label="Books" />
          <Tab label="Authors" />
          <Tab label="Categories" />
          <Tab label="Tags" />
        </Tabs>
        <TabPanel value={activeTab} index={0}>
          <ManageSeriesList />
        </TabPanel>
        <TabPanel value={activeTab} index={1}>
          <ManageBooksList />
        </TabPanel>
        <TabPanel value={activeTab} index={2}>
          <ManageAuthorsList />
        </TabPanel>
        <TabPanel value={activeTab} index={3}>
          <ManageCategoriesList />
        </TabPanel>
        <TabPanel value={activeTab} index={4}>
          <ManageTagsList />
        </TabPanel>
      </Box>
    </Box>
  );
};
