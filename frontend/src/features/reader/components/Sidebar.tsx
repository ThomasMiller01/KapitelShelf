import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { ResponsiveDrawer } from "../../../components/base/ResponsiveDrawer";
import {
  BookContent,
  BookTocItem,
} from "../../../utils/reader/BookContentModels";
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
  onTocItemSelect: onTocItemSelectBase,
}) => {
  const onTocItemSelect = (item: BookTocItem) => {
    closeSidebar();
    onTocItemSelectBase?.(item);
  };

  return (
    <ResponsiveDrawer
      open={sidebarOpen}
      onClose={closeSidebar}
      actionLink={`/library/books/${bookId}`}
      actionText="Back to Library"
      actionIcon={<ArrowBackIcon />}
      disableMobileTopInset
    >
      <TableOfContents
        items={content?.navigation.tableOfContents ?? []}
        onSelectItem={onTocItemSelect}
      />
    </ResponsiveDrawer>
  );
};
