// @ts-expect-error swiper is installed
import "swiper/css";
// @ts-expect-error swiper is installed
import "swiper/css/free-mode";
// @ts-expect-error swiper is installed
import "swiper/css/navigation";

import { Box, Typography } from "@mui/material";
import React from "react";
import { FreeMode, Navigation } from "swiper/modules";
import { Swiper, SwiperSlide } from "swiper/react";

import { useMobile } from "../hooks/useMobile";

type ScrollableListProps = {
  title?: string;
  children: React.ReactNode;
  itemWidth?: number; // px
  itemGap?: number; // px
};

export const ScrollableList: React.FC<ScrollableListProps> = ({
  title,
  children,
  itemWidth = 120,
  itemGap = 24,
}) => {
  const { isMobile } = useMobile();

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        flex: 1,
        minWidth: 0,
      }}
    >
      {title && (
        <Typography variant="h5" sx={{ ml: 1, mt: 1 }} gutterBottom={false}>
          {title}
        </Typography>
      )}
      <Box
        sx={{
          width: "100%",
          maxWidth: "100%",
          minWidth: 0,
          overflow: "hidden",
          ".swiper": {
            minWidth: 0,
            maxWidth: "100vw",
            width: "100% !important",
            overflow: "hidden",
          },
          ".swiper-slide": {
            width: "auto",
            flexShrink: 0,
            display: "block",
          },
          ".swiper-wrapper": {
            display: "flex",
          },
          ".swiper-button-prev": {
            color: "black",
            textShadow: "0 0 15px lightgrey",
            left: "15px",
          },
          ".swiper-button-next": {
            color: "black",
            textShadow: "0 0 15px lightgrey",
            right: "20px",
          },
          ".swiper-button-disabled": {
            display: "none",
          },
        }}
      >
        <Swiper
          modules={[FreeMode, Navigation]}
          freeMode={{ enabled: true, momentumRatio: 0.8, minimumVelocity: 0.1 }}
          navigation={!isMobile}
          spaceBetween={itemGap}
          slidesPerView="auto"
          autoHeight
          style={{ margin: "0 8px", padding: "18px 0" }}
        >
          {React.Children.map(children, (child, _) => (
            <SwiperSlide>
              <Box sx={{ width: itemWidth }}>{child}</Box>
            </SwiperSlide>
          ))}
        </Swiper>
      </Box>
    </Box>
  );
};
