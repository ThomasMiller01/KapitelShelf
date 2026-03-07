import { ResponsiveDrawer } from "../../../components/base/ResponsiveDrawer";
import { BookContent } from "../../../utils/bookReader/BookContent";
import TableOfContents from "./TableOfContents";

interface SidebarProps {
  content: BookContent;
  sidebarOpen: boolean;
  closeSidebar: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  content,
  sidebarOpen,
  closeSidebar,
}) => {
  return (
    <ResponsiveDrawer open={sidebarOpen} onClose={closeSidebar}>
      <TableOfContents items={content?.navigation.tableOfContents ?? []} />
    </ResponsiveDrawer>
  );
};
