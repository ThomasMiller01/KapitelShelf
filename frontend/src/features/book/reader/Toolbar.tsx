import { Stack, Typography } from "@mui/material";
import { ResponsiveDrawerAppBar } from "../../../components/base/ResponsiveDrawer";
import { useMobile } from "../../../hooks/useMobile";
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
  const { isMobile } = useMobile();

  return (
    <ResponsiveDrawerAppBar open={sidebarOpen} toggle={openSidebar}>
      <Stack
        direction={{ sm: "column", md: "row" }}
        spacing={1}
        alignItems="baseline"
        width="100%"
      >
        <Typography
          variant="h6"
          noWrap
          component="div"
          overflow="hidden"
          textOverflow="ellipsis"
          pr={isMobile ? 5 : 4}
          width="100%"
          color="textPrimary"
          textAlign="left"
        >
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
