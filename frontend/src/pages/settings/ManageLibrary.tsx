import { Alert, Box, Tab, Tabs, Typography } from "@mui/material";
import { useMemo, type ReactElement } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { TabPanel } from "../../components/base/TabPanel";
import { ManageBooksList } from "../../features/book/ManageBooksList";
import { ManageSeriesList } from "../../features/series/ManageSeriesList";

const TABS = [
  { label: "Books", value: "books" },
  { label: "Series", value: "series" },
  { label: "Authors", value: "authors" },
  { label: "Categories", value: "categories" },
  { label: "Tags", value: "tags" },
] as const;

export const ManageLibraryPage = (): ReactElement => {
  const navigate = useNavigate();
  const { section } = useParams();

  const activeTab = useMemo(() => {
    const index = TABS.findIndex((t) => t.value === section);
    return index >= 0 ? index : 0;
  }, [section]);

  const handleTabChange = (_: unknown, index: number) => {
    navigate(`/settings/manage-library/${TABS[index].value}`);
  };

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
          <Tab label="Books" />
          <Tab label="Series" />
          <Tab label="Authors" />
          <Tab label="Categories" />
          <Tab label="Tags" />
        </Tabs>
        <TabPanel value={activeTab} index={0}>
          <ManageBooksList />
        </TabPanel>
        <TabPanel value={activeTab} index={1}>
          <ManageSeriesList />
        </TabPanel>
        <TabPanel value={activeTab} index={2}>
          <Alert severity="info">This section is not yet implemented.</Alert>
        </TabPanel>
        <TabPanel value={activeTab} index={3}>
          <Alert severity="info">This section is not yet implemented.</Alert>
        </TabPanel>
        <TabPanel value={activeTab} index={4}>
          <Alert severity="info">This section is not yet implemented.</Alert>
        </TabPanel>
      </Box>
    </Box>
  );
};
