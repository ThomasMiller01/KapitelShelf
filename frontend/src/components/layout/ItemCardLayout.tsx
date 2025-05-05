import type { TypographyProps } from "@mui/material";
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
import type { ReactNode } from "react";
import { type ReactElement, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useMobile } from "../../hooks/useMobile";

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
  link?: string | null | undefined;
  image?: string | null | undefined;
  fallbackImage?: string | null | undefined;
  badge?: string | null | undefined;
  metadata: ReactNode[];
}

const ItemCardLayout = ({
  title,
  link,
  image,
  fallbackImage,
  badge,
  metadata = [],
}: ItemCardLayoutProps): ReactElement => {
  const navigate = useNavigate();

  const { isMobile } = useMobile();

  const [imageSrc, setImageSrc] = useState(
    image ? `/data/${image}` : fallbackImage
  );

  return (
    <Card sx={{ height: "100%" }} onClick={() => link && navigate(link)}>
      <Box component={link ? CardActionArea : "div"}>
        <Box position="relative">
          <CardMedia
            component="img"
            height={isMobile ? "200" : "250"}
            image={imageSrc ?? undefined}
            onError={() => setImageSrc(fallbackImage ?? "")}
            alt={title || "Item image"}
          />
          {badge && (
            <Box position="absolute" bottom="5px" right="5px">
              <Avatar
                variant="rounded"
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
        <CardContent sx={{ px: "12px", pt: "10px", pb: "8px" }}>
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
      </Box>
    </Card>
  );
};

export default ItemCardLayout;
