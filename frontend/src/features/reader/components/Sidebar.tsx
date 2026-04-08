import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { ResponsiveDrawer } from "../../../shared/components/base/ResponsiveDrawer";
import {
  BookContent,
  BookTocItem,
} from "../utils/BookContentModels";
import TableOfContents from "./TableOfContents";

interface SidebarProps {
  bookId: string | undefined;
  content: BookContent;
  isCompactLayout: boolean;
  sidebarOpen: boolean;
  closeSidebar: () => void;
  onTocItemSelect?: (item: BookTocItem) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  bookId,
  content,
  isCompactLayout,
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
      mobileOverride={isCompactLayout}
    >
      <TableOfContents
        items={content?.navigation.tableOfContents ?? []}
        onSelectItem={onTocItemSelect}
      />
    </ResponsiveDrawer>
  );
};
