import {
  Avatar,
  Box,
  Card,
  CardActionArea,
  CardContent,
  CardMedia,
  Grid,
  Stack,
  Typography,
} from "@mui/material";
import { type ReactElement, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useMobile } from "../../../hooks/useMobile";
import type { ItemCardLayoutProps, MetadataItemProps } from "./ItemCardLayout";

export const DetailedMetadataItem = ({
  icon,
  children,
  ...props
}: MetadataItemProps): ReactElement => (
  <Grid>
    <Stack direction="row" spacing={0.8} alignItems="start">
      {icon && (
        <Box sx={{ "& > *": { fontSize: "1rem !important" } }}>{icon}</Box>
      )}
      <Typography variant="body2" {...props}>
        {children}
      </Typography>
    </Stack>
  </Grid>
);

const DetailedItemCardLayout = ({
  title,
  description,
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
        width: "100%",
      }}
      onClick={handleClick}
    >
      <Box
        component={isClickable ? CardActionArea : "div"}
        height="100%"
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "flex-start",
          alignItems: "flex-start",
        }}
      >
        <Box position="relative" height="100%">
          {/* Cover */}
          <CardMedia
            component="img"
            image={imageSrc ?? undefined}
            onError={() => setImageSrc(fallbackImage ?? "")}
            alt={title || "Item image"}
            height={200}
            sx={{
              aspectRatio: "2.5 / 3", // book-cover ratio
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
        <CardContent
          sx={{
            width: "100%",
            height: "100%",
            display: "flex",
            flexDirection: "column",
            px: "12px",
            pt: "10px",
            pb: "8px",
          }}
        >
          {/* Title */}
          <Typography
            variant="h6"
            gutterBottom
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
          {/* Description */}
          <Typography
            variant="body2"
            color="text.secondary"
            display="-webkit-box"
            textOverflow="ellipsis"
            overflow="hidden"
            gutterBottom
            sx={{
              WebkitLineClamp: 4,
              WebkitBoxOrient: "vertical",
              wordBreak: "break-word",
            }}
          >
            {description}
          </Typography>
          {/* Metadata */}
          <Grid container rowSpacing={0.5} columnSpacing={2.5}>
            {metadata}
          </Grid>
        </CardContent>
      </Box>
    </Card>
  );
};

export default DetailedItemCardLayout;
