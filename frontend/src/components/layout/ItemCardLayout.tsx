import type { TypographyProps } from "@mui/material";
import {
  Avatar,
  Box,
  Card,
  CardContent,
  CardMedia,
  styled,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useState } from "react";

interface MetadataItemProps extends TypographyProps {
  isMobile?: boolean;
}

export const MetadataItem = styled(Typography, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<MetadataItemProps>(({ theme, isMobile }) => ({
  fontSize: isMobile ? "0.7rem" : "0.9rem",
  color: theme.palette.text.secondary,
  marginTop: "5px",
}));

interface ItemCardLayoutProps {
  title: string | null | undefined;
  image?: string | null | undefined;
  fallbackImage?: string | null | undefined;
  badge?: string | null | undefined;
  metadata: ReactNode[];
}

const ItemCardLayout = ({
  title,
  image,
  fallbackImage,
  badge,
  metadata = [],
}: ItemCardLayoutProps): ReactElement => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  const [imageSrc, setImageSrc] = useState(image ?? fallbackImage);

  return (
    <Card sx={{ height: "100%", position: "relative" }}>
      {image && (
        <CardMedia
          component="img"
          height={isMobile ? "200" : "250"}
          image={imageSrc ?? undefined}
          onError={() => setImageSrc(fallbackImage)}
          alt={title || "Item image"}
        />
      )}
      {badge && (
        <Box position="absolute" top="2px" right="5px">
          <Avatar
            sx={{
              width: 30,
              height: 30,
              backgroundColor: "rgba(0, 0, 0, 0.3)",
              opacity: "0.8",
              fontSize: "0.8rem",
              fontWeight: 400,
              lineHeight: "2",
              fontFeatureSettings: '"calt"',
              fontFamily: "Playwrite AU SA",
            }}
          >
            {badge}
          </Avatar>
        </Box>
      )}
      <CardContent sx={{ padding: "12px !important" }}>
        <Typography
          variant="h6"
          sx={{
            fontSize: isMobile ? "0.8rem !important" : "1rem !important",
            display: "-webkit-box",
            WebkitLineClamp: 2,
            WebkitBoxOrient: "vertical",
            overflow: "hidden",
            textOverflow: "ellipsis",
            lineHeight: 1.2,
            minHeight: "2.4em",
            wordBreak: "break-all",
          }}
        >
          {title}
        </Typography>
        {metadata}
      </CardContent>
    </Card>
  );
};

export default ItemCardLayout;
