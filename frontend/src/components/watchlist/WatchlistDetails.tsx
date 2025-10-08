import KeyboardDoubleArrowLeftIcon from "@mui/icons-material/KeyboardDoubleArrowLeft";
import KeyboardDoubleArrowRightIcon from "@mui/icons-material/KeyboardDoubleArrowRight";
import LaunchIcon from "@mui/icons-material/Launch";
import { Badge, Box, Grid, Grow, Stack, Typography } from "@mui/material";
import React, { useState } from "react";
import { Link } from "react-router-dom";
import { TransitionGroup } from "react-transition-group";

import { useMobile } from "../../hooks/useMobile";
import type { WatchlistDTO } from "../../lib/api/KapitelShelf.Api";
import { IconButtonWithTooltip } from "../base/IconButtonWithTooltip";
import { ResultCard } from "./ResultCard";

interface WatchlistDetailsProps {
  watchlist: WatchlistDTO;
}

export const WatchlistDetails: React.FC<WatchlistDetailsProps> = ({
  watchlist,
}) => {
  const { isMobile } = useMobile();
  const [showAll, setShowAll] = useState(false);
  const items = watchlist.items ?? [];

  return (
    <Box width="fit-content">
      <Link
        to={`/library/series/${watchlist.series?.id}`}
        style={{
          textDecoration: "none",
          width: "fit-content",
          display: "inline-block",
        }}
      >
        <Stack
          direction="row"
          spacing={1}
          alignItems="center"
          mb={1}
          width="fit-content"
        >
          <Typography
            variant="h6"
            gutterBottom
            color="text.primary"
            sx={{
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
              maxWidth: isMobile ? "280px" : "200px",
            }}
          >
            {watchlist.series?.name}
          </Typography>
          <LaunchIcon sx={{ fontSize: "1.2rem", color: "text.primary" }} />
        </Stack>
      </Link>

      {items.length > 0 && (
        <Grid container spacing={2}>
          {/* next released volume */}
          <Grid key={items[0].id}>
            <ResultCard book={items[0]} />
          </Grid>

          {/* additional volumes */}
          <TransitionGroup component={null}>
            {showAll &&
              items.slice(1).map((book, idx) => (
                <Grow
                  key={book.id}
                  in={showAll}
                  timeout={350}
                  style={{
                    transitionDelay: showAll ? `${idx * 100}ms` : "0ms",
                    display: "inline-block",
                  }}
                >
                  <Grid>
                    <ResultCard book={book} />
                  </Grid>
                </Grow>
              ))}
          </TransitionGroup>

          {/* show additional volumes button */}
          {items.length > 1 && (
            <Grid
              alignItems="end"
              display="flex"
              minWidth={isMobile ? "100px" : "initial"}
              justifyContent="center"
              ml={isMobile ? "-35px" : "-10px"}
            >
              <Badge
                badgeContent={showAll ? undefined : `${items.length - 1}`}
                color="secondary"
              >
                <IconButtonWithTooltip
                  tooltip={`Show ${showAll ? "less" : "all"} volumes`}
                  size="medium"
                  onClick={() => setShowAll((prev) => !prev)}
                >
                  {showAll ? (
                    <KeyboardDoubleArrowLeftIcon fontSize="inherit" />
                  ) : (
                    <KeyboardDoubleArrowRightIcon fontSize="inherit" />
                  )}
                </IconButtonWithTooltip>
              </Badge>
            </Grid>
          )}
        </Grid>
      )}

      {(watchlist.items?.length ?? []) === 0 && (
        <Typography variant="body2" color="text.secondary" mt={-0.5}>
          The next volume has not been announced yet.
        </Typography>
      )}
    </Box>
  );
};
