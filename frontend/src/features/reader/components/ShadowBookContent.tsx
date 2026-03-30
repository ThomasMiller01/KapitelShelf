import type { Theme } from "@mui/material";
import { Box } from "@mui/material";
import React, { useLayoutEffect, useRef } from "react";

interface ShadowBookContentProps {
  html: string;
  theme: Theme;
  fontScale: number;
  contentFontFamily: string;
}

export const ShadowBookContent: React.FC<ShadowBookContentProps> = ({
  html,
  theme,
  fontScale,
  contentFontFamily,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    const host = hostRef.current;
    if (!host) {
      return;
    }

    const shadowRoot = host.shadowRoot ?? host.attachShadow({ mode: "open" });
    shadowRoot.innerHTML = `<style>${ContentStyles(
      theme,
      fontScale,
      contentFontFamily,
    )}</style>${html}`;
  }, [contentFontFamily, fontScale, html, theme]);

  return <Box ref={hostRef} sx={{ height: "100%" }} />;
};

const ContentStyles = (
  theme: Theme,
  fontScale: number,
  contentFontFamily: string,
): string => `
:host {
  color: ${theme.palette.text.primary} !important;
  background: transparent;
  font-family: ${contentFontFamily};
  font-size: ${fontScale}rem;
  line-height: 1.7;
  word-break: break-word;
  overflow-wrap: break-word;
  display: block;
  height: 100%;
}

:host * {
  break-after: auto !important;
  break-before: auto !important;
  page-break-after: auto !important;
  page-break-before: auto !important;
  break-inside: auto !important;
  page-break-inside: auto !important;
}

:host p,
:host h1,
:host h2,
:host h3,
:host h4,
:host h5,
:host h6,
:host ul,
:host ol,
:host li,
:host blockquote,
:host table,
:host thead,
:host tbody,
:host tfoot,
:host tr,
:host td,
:host th,
:host dl,
:host dt,
:host dd,
:host figcaption,
:host a,
:host em,
:host strong,
:host span {
  color: ${theme.palette.text.primary} !important;
  font-family: ${contentFontFamily} !important;
}

:host p {
  margin: 0 0 0.5em 0 !important;
  text-align: justify;
  text-align-last: left;
  line-height: 1.7;
}

:host a {
  color: ${theme.palette.primary.main} !important;
  text-decoration: underline;
  text-decoration-color: ${theme.palette.primary.main};
}

:host a:hover {
  color: ${theme.palette.primary.dark};
  text-decoration-color: ${theme.palette.primary.dark};
}

:host a:visited {
  color: ${theme.palette.primary.main};
}

:host img {
  max-width: 100%;
  height: auto;
  display: block;
}

:host blockquote {
  border-left: 4px solid ${theme.palette.divider};
  padding-left: 1em;
  margin-left: 0;
  color: ${theme.palette.text.secondary};
  font-style: italic;
}

:host ul,
:host ol {
  padding-left: 1.6em;
  margin-bottom: 1em;
}

:host li {
  margin-bottom: 0.4em;
}

:host pre {
  overflow-x: auto;
  padding: 12px;
  border-radius: 6px;
  background: ${theme.palette.background.paper};
  font-family: monospace;
}

:host code {
  font-family: monospace;
  background: ${theme.palette.action.hover};
  padding: 2px 4px;
  border-radius: 4px;
}

:host hr {
  border: none;
  border-top: 1px solid ${theme.palette.divider};
  margin: 2em 0;
}
`;
