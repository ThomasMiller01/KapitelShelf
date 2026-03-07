import { Stack, Typography } from "@mui/material";
import { ResponsiveDrawerAppBar } from "../../../components/base/ResponsiveDrawer";
import { BookContent } from "../../../utils/bookReader/BookContent";

interface ToolbarProps {
  content: BookContent;
  sidebarOpen: boolean;
  openSidebar: () => void;
}

export const Toolbar: React.FC<ToolbarProps> = ({
  content,
  sidebarOpen,
  openSidebar,
}) => {
  return (
    <ResponsiveDrawerAppBar open={sidebarOpen} toggle={openSidebar}>
      <Stack
        direction={{ sm: "column", md: "row" }}
        spacing={1}
        alignItems="baseline"
      >
        <Typography variant="h6" noWrap component="div">
          {content.metadata.title}
        </Typography>
        <Typography
          variant="body2"
          color="text.secondary"
          noWrap
          component="div"
        >
          {content.metadata.subtitle}
        </Typography>
      </Stack>
    </ResponsiveDrawerAppBar>
  );
};
