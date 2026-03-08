import { Box, Theme, useTheme } from "@mui/material";
import React, { useLayoutEffect, useRef, useState } from "react";
import { useMobile } from "../../../hooks/useMobile";
import { BookSection } from "../../../utils/bookReader/BookContent";

interface ContentSectionProps {
  section: BookSection;
  currentPage: number;
  onTotalPagesChange: (total: number) => void;
}

export const ContentSection: React.FC<ContentSectionProps> = ({
  section,
  currentPage,
  onTotalPagesChange,
}) => {
  const theme = useTheme();
  const containerRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);
  const { isMobile } = useMobile();

  useLayoutEffect(() => {
    const container = containerRef.current;
    const content = contentRef.current;
    if (!container || !content) return;

    const measure = () => {
      const width = content.offsetWidth;
      setContainerWidth(width);
      const pages = width > 0 ? Math.ceil(content.scrollWidth / width) : 1;
      onTotalPagesChange(Math.max(1, pages));
    };

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return () => observer.disconnect();
  }, [section, onTotalPagesChange]);

  return (
    <Box
      ref={containerRef}
      sx={{
        aspectRatio: isMobile ? "none" : "2 / 3",
        maxWidth: "100%",
        maxHeight: "100%",
        overflow: "hidden",
        background: theme.palette.background.paper,
        borderRadius: 1,
        py: isMobile ? 1 : 2,
      }}
    >
      <Box
        ref={contentRef}
        sx={{
          height: "100%",
          columns: "1",
          columnFill: "auto",
          columnGap: 0,
          transform: `translateX(${-(currentPage * containerWidth)}px)`,
        }}
      >
        <Box
          sx={{
            px: isMobile ? 1.5 : 3,
            height: "100%",
            ...ContentStyles(theme),
          }}
          dangerouslySetInnerHTML={{ __html: section.content || "" }}
        />
      </Box>
    </Box>
  );
};

const ContentStyles = (theme: Theme): any => {
  return {
    "&": {
      color: `${theme.palette.text.primary} !important`,
      background: "transparent",
      fontFamily: theme.typography.fontFamily,
      fontSize: "1rem",
      lineHeight: 1.7,
      wordBreak: "break-word",
      overflowWrap: "break-word",
    },

    "& p": {
      margin: "0 0 0.8em 0 !important",
      color: `${theme.palette.text.primary} !important`,
      fontFamily: `${theme.typography.fontFamily} !important`,
      textAlign: "justify",
      textAlignLast: "left",
    },

    "& a": {
      color: theme.palette.primary.main,
      textDecoration: "underline",
      textDecorationColor: theme.palette.primary.main,
      "&:hover": {
        color: theme.palette.primary.dark,
        textDecorationColor: theme.palette.primary.dark,
      },
      "&:visited": {
        color: theme.palette.primary.main,
      },
    },

    "& img": {
      maxWidth: "100%",
      height: "auto",
      display: "block",
    },

    "& blockquote": {
      borderLeft: `4px solid ${theme.palette.divider}`,
      paddingLeft: "1em",
      marginLeft: 0,
      color: theme.palette.text.secondary,
      fontStyle: "italic",
    },

    "& ul": {
      paddingLeft: "1.6em",
      marginBottom: "1em",
    },

    "& ol": {
      paddingLeft: "1.6em",
      marginBottom: "1em",
    },

    "& li": {
      marginBottom: "0.4em",
    },

    "& pre": {
      overflowX: "auto",
      padding: "12px",
      borderRadius: "6px",
      background: theme.palette.background.paper,
      fontFamily: "monospace",
    },

    "& code": {
      fontFamily: "monospace",
      background: theme.palette.action.hover,
      padding: "2px 4px",
      borderRadius: "4px",
    },

    "& hr": {
      border: "none",
      borderTop: `1px solid ${theme.palette.divider}`,
      margin: "2em 0",
    },
  };
};
