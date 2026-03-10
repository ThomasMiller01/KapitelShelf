import type { Theme } from "@mui/material";
import { Box } from "@mui/material";
import React, { useLayoutEffect, useRef } from "react";

interface ShadowBookContentProps {
  html: string;
  theme: Theme;
  fontScale: number;
}

export const ShadowBookContent: React.FC<ShadowBookContentProps> = ({
  html,
  theme,
  fontScale,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    const host = hostRef.current;
    if (!host) {
      return;
    }

    const shadowRoot = host.shadowRoot ?? host.attachShadow({ mode: "open" });
    shadowRoot.innerHTML = `<style>${ContentStyles(theme, fontScale)}</style>${html}`;
  }, [fontScale, html, theme]);

  return <Box ref={hostRef} sx={{ height: "100%" }} />;
};

const ContentStyles = (theme: Theme, fontScale: number): string => `
:host {
  color: ${theme.palette.text.primary} !important;
  background: transparent;
  font-family: ${theme.typography.fontFamily};
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

:host p {
  margin: 0 0 0.5em 0 !important;
  color: ${theme.palette.text.primary} !important;
  font-family: ${theme.typography.fontFamily} !important;
  text-align: justify;
  text-align-last: left;
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
