import { Avatar, IconButton, Stack, Typography } from "@mui/material";
import React from "react";

import { ScrollableList } from "../../../../components/base/ScrollableList";
import {
  READER_CONTENT_FONT_OPTIONS,
  useReaderColorScheme,
} from "../ThemeProvider";

export const FontFamilySelector: React.FC = (): React.ReactElement => {
  const { contentFont, setContentFont } = useReaderColorScheme();

  return (
    <Stack spacing={1.5}>
      <Typography variant="caption" color="text.secondary" fontSize="1rem">
        Font Family
      </Typography>
      <ScrollableList
        itemWidth={75}
        itemGap={8}
        showNavigation={false}
        verticalPadding={0}
      >
        {READER_CONTENT_FONT_OPTIONS.map((option) => {
          const isSelected = option.value === contentFont;

          return (
            <IconButton
              key={option.value}
              onClick={() => setContentFont(option.value)}
            >
              <Stack alignItems="center">
                <Avatar
                  sx={(theme) => ({
                    width: 56,
                    height: 56,
                    fontFamily: option.family,
                    fontSize: "2rem",
                    fontWeight: "bold",
                    bgcolor: "transparent",
                    color: isSelected
                      ? theme.palette.primary.contrastText
                      : theme.palette.text.primary,
                    textShadow: isSelected
                      ? `0 0 6px ${theme.palette.text.primary}`
                      : "none",
                  })}
                >
                  Aa
                </Avatar>
                <Typography
                  variant="caption"
                  textAlign="center"
                  sx={(theme) => ({
                    lineHeight: 1.15,
                    minHeight: "2.2em",
                    fontSize: "0.75rem",
                    color: isSelected
                      ? theme.palette.primary.contrastText
                      : theme.palette.text.primary,
                    textShadow: isSelected
                      ? `1px 1px 2px ${theme.palette.text.primary}`
                      : "none",
                  })}
                >
                  {option.label}
                </Typography>
              </Stack>
            </IconButton>
          );
        })}
      </ScrollableList>
    </Stack>
  );
};
