import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Box,
  Collapse,
  Divider,
  IconButton,
  List,
  ListItemButton,
  Typography,
} from "@mui/material";
import { useState, type ReactElement } from "react";

import type { BookTocItem } from "../../../utils/reader/BookContentModels";

interface TableOfContentsProps {
  items: BookTocItem[];
  onSelectItem?: (item: BookTocItem) => void;
  maxHeight?: string;
}

const TableOfContents: React.FC<TableOfContentsProps> = ({
  maxHeight,
  ...props
}) => {
  return (
    <Box
      bgcolor="background.paper"
      px={2}
      py={1}
      sx={{ maxHeight: maxHeight, overflow: "auto" }}
    >
      <Typography variant="h6" sx={{ px: 1, pt: 1, pb: 0.5 }}>
        Table of Contents
      </Typography>
      <Divider sx={{ mb: 1 }} />
      <TableOfContentsContent {...props} />
    </Box>
  );
};

interface TableOfContentsContentProps
  extends Omit<TableOfContentsProps, "maxHeight"> {}

const TableOfContentsContent: React.FC<TableOfContentsContentProps> = ({
  items,
  onSelectItem,
}) => {
  if (items.length === 0) {
    return (
      <Box sx={{ px: 2, py: 1.5 }}>
        <Typography variant="body2" color="text.secondary">
          No table of contents available.
        </Typography>
      </Box>
    );
  }

  return (
    <List>
      {items.map((item: BookTocItem): ReactElement => {
        return (
          <TableOfContentsItem
            key={item.id}
            item={item}
            level={0}
            onSelectItem={onSelectItem}
          />
        );
      })}
    </List>
  );
};

interface TableOfContentsItemProps {
  item: BookTocItem;
  level: number;
  onSelectItem?: (item: BookTocItem) => void;
}

const TableOfContentsItem: React.FC<TableOfContentsItemProps> = ({
  item,
  level,
  onSelectItem,
}) => {
  const hasChildren = item.children.length > 0;
  const [isExpanded, setIsExpanded] = useState<boolean>(level < 1);

  const handleToggle = (): void => {
    setIsExpanded((previous: boolean): boolean => {
      return !previous;
    });
  };

  const handleClick = (): void => {
    onSelectItem?.(item);
  };

  return (
    <>
      <ListItemButton
        onClick={handleClick}
        sx={{
          borderRadius: 1,
          gap: 0.5,
          pl: 1,
          py: 0.75,
          my: 0.5,
        }}
      >
        <Box
          sx={{
            width: 18,
            minWidth: 18,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          {hasChildren ? (
            <IconButton
              size="small"
              onClick={(event): void => {
                event.stopPropagation();
                handleToggle();
              }}
              sx={{
                p: 0.25,
              }}
            >
              {isExpanded ? (
                <ExpandMoreIcon sx={{ fontSize: 16 }} />
              ) : (
                <ChevronRightIcon sx={{ fontSize: 16 }} />
              )}
            </IconButton>
          ) : (
            <Box
              sx={{
                width: 4,
                height: 4,
                borderRadius: "50%",
              }}
            />
          )}
        </Box>

        <Typography
          variant="body2"
          sx={{
            flexGrow: 1,
            fontSize: level === 0 ? "0.9rem" : "0.82rem",
            fontWeight: level === 0 ? 500 : 400,
            lineHeight: 1.35,
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
          }}
        >
          {item.label}
        </Typography>
      </ListItemButton>

      {hasChildren && (
        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
          <List disablePadding>
            {item.children.map((child: BookTocItem): ReactElement => {
              return (
                <TableOfContentsItem
                  key={child.id}
                  item={child}
                  level={level + 1}
                  onSelectItem={onSelectItem}
                />
              );
            })}
          </List>
        </Collapse>
      )}
    </>
  );
};

export default TableOfContents;
