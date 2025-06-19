import {
  Avatar,
  Box,
  Card,
  CardActionArea,
  CardContent,
  CardMedia,
  styled,
  Typography,
} from "@mui/material";
import { type ReactElement, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useMobile } from "../../../hooks/useMobile";
import type { ItemCardLayoutProps } from "./ItemCardLayout";

export const NormalMetadataItem = styled(Typography)(({ theme }) => ({
  fontSize: "0.9rem",
  color: theme.palette.text.secondary,
  marginTop: "5px",
}));

const NormalItemCardLayout = ({
  title,
  link,
  onClick,
  image,
  fallbackImage,
  badge,
  squareBadge = true,
  metadata = [],
}: ItemCardLayoutProps): ReactElement => {
  const navigate = useNavigate();

  const { isMobile } = useMobile();

  const [imageSrc, setImageSrc] = useState(image ?? fallbackImage);

  const isClickable = Boolean(link || onClick);
  const handleClick = (): void => {
    if (!isClickable) {
      return;
    }

    if (onClick) {
      onClick();
    }

    if (link) {
      navigate(link);
    }
  };

  return (
    <Card
      sx={{
        width: {
          xs: "41vw", // mobile: about 2 cards per row
          sm: "30vw", // small tablet: 3 per row
          md: "22vw", // desktop: 4–5 per row
          lg: "15vw", // large screen: 7–8 per row
        },
        maxWidth: 200,
        height: "100%",
      }}
      onClick={handleClick}
    >
      <Box
        component={isClickable ? CardActionArea : "div"}
        height="100%"
        sx={{
          display: "flex",
          flexDirection: "column",
          justifyContent: "flex-start",
          alignItems: "flex-start",
        }}
      >
        <Box position="relative" width="100%">
          {/* Image */}
          <CardMedia
            component="img"
            image={imageSrc ?? undefined}
            onError={() => setImageSrc(fallbackImage ?? "")}
            alt={title || "Item image"}
            width="100%"
            sx={{
              aspectRatio: "2 / 3", // book-cover ratio
              objectFit: "cover",
            }}
          />

          {/* Badge */}
          {badge && (
            <Box position="absolute" bottom="5px" right="5px">
              <Avatar
                variant={squareBadge ? "rounded" : "circular"}
                sx={{
                  width: 30,
                  height: 30,
                  opacity: "0.82",
                }}
              >
                {badge}
              </Avatar>
            </Box>
          )}
        </Box>
        <CardContent sx={{ width: "100%", px: "12px", pt: "10px", pb: "8px" }}>
          {/* Title */}
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
              wordBreak: "break-word",
            }}
          >
            {title}
          </Typography>

          {/* Metadata */}
          {metadata}
        </CardContent>
      </Box>
    </Card>
  );
};

export default NormalItemCardLayout;
