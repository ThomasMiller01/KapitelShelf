import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import CategoryIcon from "@mui/icons-material/Category";
import CollectionsBookmarkIcon from "@mui/icons-material/CollectionsBookmark";
import DescriptionIcon from "@mui/icons-material/Description";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import PersonIcon from "@mui/icons-material/Person";
import {
  Box,
  Chip,
  Divider,
  Grid,
  Rating,
  Stack,
  Typography,
} from "@mui/material";
import type { ReactNode } from "react";
import { useMemo, type ReactElement } from "react";

import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { useCoverImage } from "../../hooks/useCoverImage";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import LocationDetails from "../location/LocationDetails";
import { EditableUserBookMetadata } from "./EditableUserBookMetadata";

interface MetadataItemProps {
  icon: ReactNode;
  children: ReactNode;
}

const MetadataGridItem = ({
  icon,
  children,
}: MetadataItemProps): ReactElement => (
  <Grid>
    <Stack direction="row" spacing={0.8} alignItems="start">
      {icon && (
        <Box sx={{ "& > *": { fontSize: "1.2rem !important" } }}>{icon}</Box>
      )}
      <Typography variant="body2">{children}</Typography>
    </Stack>
  </Grid>
);

interface BookDetailsProps {
  book: BookDTO;
}

const BookDetails = ({ book }: BookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { coverImage, onLoadingError } = useCoverImage({ initial: book });
  const { profile } = useUserProfile();

  const currentUserBookMetadata = useMemo(() => {
    return book.userMetadata?.find((x) => x.userId === profile?.id);
  }, [book.userMetadata, profile?.id]);

  return (
    <Box p={3}>
      <Grid container spacing={{ xs: 1, md: 4 }} columns={11}>
        <Grid size={{ xs: 0, md: 0.5 }}></Grid>

        <Grid size={{ xs: 11, md: 2.5 }}>
          <Stack>
            <img
              src={coverImage}
              onError={onLoadingError}
              style={{
                width: isMobile ? "60%" : "100%",
                borderRadius: 2,
                boxShadow: "3",
                marginLeft: isMobile ? "auto" : 0,
                marginRight: isMobile ? "auto" : 0,
              }}
            />

            {book.location && (
              <LocationDetails bookId={book.id} location={book.location} />
            )}
          </Stack>
        </Grid>

        <Grid size={{ xs: 11, md: 7.5 }} mt="20px">
          <Typography
            variant="h5"
            gutterBottom
            sx={{ wordBreak: "break-word" }}
          >
            {book.title}
          </Typography>

          <Typography
            variant="body1"
            color="text.secondary"
            mb="15px"
            sx={{ wordBreak: "break-word" }}
          >
            {book.description}
          </Typography>

          <Grid container rowSpacing={1} columnSpacing={2.5} mb={2}>
            {book.pageNumber !== null && (
              <MetadataGridItem icon={<DescriptionIcon />}>
                {book.pageNumber} pages
              </MetadataGridItem>
            )}
            {book.author && (
              <MetadataGridItem icon={<PersonIcon />}>
                {book.author.firstName} {book.author.lastName}
              </MetadataGridItem>
            )}
            {book.releaseDate && (
              <MetadataGridItem icon={<CalendarMonthIcon />}>
                {new Date(book.releaseDate).toLocaleDateString(undefined, {
                  year: "numeric",
                  month: "2-digit",
                  day: "2-digit",
                })}
              </MetadataGridItem>
            )}
            <MetadataGridItem icon={<CollectionsBookmarkIcon />}>
              {book.series?.name} #{book.seriesNumber}
            </MetadataGridItem>
          </Grid>

          <Stack direction="row" spacing={1} mb="5px">
            <CategoryIcon sx={{ mr: "5px !important" }} />
            <Stack
              direction="row"
              spacing={1}
              mb={1.5}
              flexWrap="wrap"
              alignItems="center"
            >
              {book.categories &&
                book.categories.map((category) => (
                  <Chip
                    key={category.id}
                    label={category.name}
                    color="primary"
                    size="small"
                    sx={{ my: "4px !important" }}
                  />
                ))}
            </Stack>
            {book.categories?.length === 0 && (
              <Typography variant="subtitle1">No Categories</Typography>
            )}
          </Stack>

          <Stack direction="row" spacing={1}>
            <LocalOfferIcon sx={{ mr: "5px !important" }} />
            <Stack
              direction="row"
              spacing={1}
              flexWrap="wrap"
              alignItems="center"
            >
              {book.tags &&
                book.tags.map((tag) => (
                  <Chip
                    key={tag.id}
                    label={tag.name}
                    variant="outlined"
                    size="small"
                    sx={{ my: "4px !important" }}
                  />
                ))}
            </Stack>
            {book.tags?.length === 0 && (
              <Typography variant="subtitle1">No Tags</Typography>
            )}
          </Stack>

          <Stack spacing={4} mt={2.5}>
            {/* Rating */}
            {book.rating && (
              <Stack direction="row" spacing={1} alignItems="center">
                <Rating
                  value={book.rating / 2}
                  defaultValue={0}
                  max={5}
                  precision={0.5}
                  readOnly
                />
                <IconButtonWithTooltip
                  tooltip="Average rating from all users"
                  size="small"
                  color="secondary"
                >
                  <HelpOutlineIcon fontSize="small" />
                </IconButtonWithTooltip>
              </Stack>
            )}

            <Divider />

            {/* Current User has no Book Metadata yet */}
            {!currentUserBookMetadata && (
              <EditableUserBookMetadata isCurrentUser />
            )}

            {/* User Book Metadata */}
            <Stack spacing={1.5}>
              {book.userMetadata?.map((x) => (
                <EditableUserBookMetadata
                  key={x.id}
                  userMetadata={x}
                  isCurrentUser={x.userId === profile?.id}
                />
              ))}
            </Stack>
          </Stack>
        </Grid>

        <Grid size={{ xs: 0, md: 0.5 }}></Grid>
      </Grid>
    </Box>
  );
};

export default BookDetails;
