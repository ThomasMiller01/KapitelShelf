import { Alert, Box, Tab, Tabs, Typography } from "@mui/material";
import { useState, type ReactElement } from "react";
import { TabPanel } from "../../components/base/TabPanel";

export const ManageLibraryPage = (): ReactElement => {
  const [activeTab, setActiveTab] = useState(0);

  return (
    <Box padding="20px">
      <Typography variant="h5">Manage your Library</Typography>
      <Box sx={{ my: 2 }}>
        <Tabs
          value={activeTab}
          onChange={(_, value) => setActiveTab(value)}
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
          <Alert severity="info">This section is not yet implemented.</Alert>
        </TabPanel>
        <TabPanel value={activeTab} index={1}>
          <Alert severity="info">This section is not yet implemented.</Alert>
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
