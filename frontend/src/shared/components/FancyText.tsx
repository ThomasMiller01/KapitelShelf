import type { TypographyProps } from "@mui/material";
import { Typography } from "@mui/material";
import { styled } from "@mui/material/styles";
import React from "react";

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface FancyTextProps extends TypographyProps {}

const StyledFancyText = styled(Typography)(() => ({
  fontFamily: "Playwrite AU SA",
  fontWeight: "200",
  lineHeight: "2",
  fontFeatureSettings: '"calt"',
}));

export const FancyText: React.FC<FancyTextProps> = ({ children, ...props }) => (
  <StyledFancyText {...props}>{children}</StyledFancyText>
);

export default FancyText;
