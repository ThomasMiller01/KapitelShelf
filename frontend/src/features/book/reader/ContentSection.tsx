import { serializeStyles } from "@emotion/serialize";
import { Box, Theme, Typography, useTheme } from "@mui/material";
import React, { useRef } from "react";
import { ScrollbarStyles } from "../../../styles/GlobalStyles";
import { BookSection } from "../../../utils/bookReader/BookContent";

interface ContentSectionProps {
  section: BookSection;
}

export const ContentSection: React.FC<ContentSectionProps> = ({ section }) => {
  const theme = useTheme();
  const iframeRef = useRef<HTMLIFrameElement>(null);

  if (section.contentType === "html" || section.contentType === "xhtml") {
    const injectStyles = () => {
      const iframe = iframeRef.current;
      if (!iframe) return;

      const doc = iframe.contentDocument;
      if (!doc) return;

      const style = doc.createElement("style");
      style.innerHTML = serializeStyles([ContentStyles(theme)]).styles;
      doc.head.appendChild(style);
    };
    return (
      <ContentSectionWrapper>
        <iframe
          key={section.id}
          ref={iframeRef}
          src={section.content}
          title={section.title ?? section.id}
          onLoad={injectStyles}
          style={{
            width: "100%",
            height: "100%",
            border: "none",
          }}
        />
      </ContentSectionWrapper>
    );
  }

  if (section.contentType === "text") {
    return (
      <ContentSectionWrapper>
        <Box
          sx={{
            p: 3,
            height: "100%",
            overflowY: "auto",
            whiteSpace: "pre-wrap",
            ...ContentStyles(theme),
          }}
        >
          <Typography>{section.content}</Typography>
        </Box>
      </ContentSectionWrapper>
    );
  }

  return (
    <ContentSectionWrapper>
      <Box sx={{ p: 2 }}>
        <Typography>
          Unsupported section content type: {section.contentType}
        </Typography>
      </Box>
    </ContentSectionWrapper>
  );
};

interface ContentSectionWrapperProps {
  children: React.ReactNode | React.ReactNode[];
}

const ContentSectionWrapper: React.FC<ContentSectionWrapperProps> = ({
  children,
}) => {
  return (
    <Box sx={{ aspectRatio: "2 / 3", height: "100%", maxWidth: "100%" }}>
      {children}
    </Box>
  );
};

const ContentStyles = (theme: Theme): any => {
  return {
    "&": {
      margin: "0 auto",
      color: `${theme.palette.text.primary} !important`,
      background: "transparent",
      fontFamily: theme.typography.fontFamily,
      fontSize: "1rem",
      lineHeight: 1.7,
      wordBreak: "break-word",
      overflowWrap: "break-word",
      hyphens: "auto",
    },

    "& p": {
      margin: "0 0 1.1em 0 !important",
    },

    "& h1, & h2, & h3, & h4, & h5, & h6": {
      color: theme.palette.text.primary,
      fontWeight: 600,
      lineHeight: 1.3,
      marginTop: "1.8em",
      marginBottom: "0.6em",
    },

    "& h1": {
      fontSize: "1.8rem",
    },

    "& h2": {
      fontSize: "1.5rem",
    },

    "& h3": {
      fontSize: "1.3rem",
    },

    "& a, & a:visited": {
      color: `${theme.palette.primary.main} !important`,
      textDecoration: "none",
    },

    "& a": {
      borderBottom: `1px solid ${theme.palette.primary.main}33`,
    },

    "& img": {
      maxWidth: "100%",
      height: "auto",
      display: "block",
      margin: "1.2em auto",
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

    ...ScrollbarStyles,
  };
};
