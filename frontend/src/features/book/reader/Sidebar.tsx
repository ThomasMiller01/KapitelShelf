import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { ResponsiveDrawer } from "../../../components/base/ResponsiveDrawer";
import {
  BookContent,
  BookTocItem,
} from "../../../utils/bookReader/BookContent";
import TableOfContents from "./TableOfContents";

interface SidebarProps {
  bookId: string | undefined;
  content: BookContent;
  sidebarOpen: boolean;
  closeSidebar: () => void;
  onTocItemSelect?: (item: BookTocItem) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  bookId,
  content,
  sidebarOpen,
  closeSidebar,
  onTocItemSelect,
}) => {
  return (
    <ResponsiveDrawer
      open={sidebarOpen}
      onClose={closeSidebar}
      actionLink={`/library/books/${bookId}`}
      actionText="Back to Library"
      actionIcon={<ArrowBackIcon />}
    >
      <TableOfContents
        items={content?.navigation.tableOfContents ?? []}
        onSelectItem={onTocItemSelect}
      />
    </ResponsiveDrawer>
  );
};
