import AddToPhotosIcon from "@mui/icons-material/AddToPhotos";
import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import {
  Avatar,
  Dialog,
  Link,
  List,
  ListItem,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

import bookCover from "../../assets/books/nocover.png";
import { useApi } from "../../contexts/ApiProvider";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api";
import { LocationUrl } from "../../utils/LocationUtils";
import { FormatTimeUntil } from "../../utils/TimeUtils";
import ItemCardLayout, {
  MetadataItem,
} from "../layout/ItemCard/ItemCardLayout";

interface ResultCardProps {
  book: BookDTO;
}

export const ResultCard: React.FC<ResultCardProps> = ({ book }) => {
  const { profile } = useUserProfile();
  const { isMobile } = useMobile();
  const { clients } = useApi();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { mutateAsync: mutateAddBookToLibrary } = useMutation({
    mutationKey: ["adding-watchlist-result-to-library"],
    mutationFn: async () => {
      if (book.id === undefined) {
        return;
      }

      const { data } =
        await clients.watchlist.watchlistResultResultIdLibraryPut(book.id);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Adding book to library",
        showSuccess: true,
        showLoading: true,
      },
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["watchlist", profile?.id] });
    },
  });

  const [menuOpen, setMenuOpen] = useState(false);

  const onClickAddToLibrary = async (): Promise<void> => {
    setMenuOpen(false);

    const bookDto = await mutateAddBookToLibrary();
    if (bookDto === undefined) {
      return;
    }

    navigate(`/library/books/${bookDto.id}`);
  };

  const timeUntilRelease = FormatTimeUntil(book.releaseDate, false, "calender");
  const isReleased = timeUntilRelease === "-";

  return (
    <>
      <ItemCardLayout
        itemVariant="normal"
        small
        raised
        title={book.title}
        link={!isReleased ? LocationUrl(book.location!) : undefined}
        externalLink
        onClick={isReleased ? (): void => setMenuOpen(true) : undefined}
        description={book.description}
        image={book.cover?.filePath}
        fallbackImage={bookCover}
        badge={
          book.seriesNumber !== 0 ? book.seriesNumber?.toString() : undefined
        }
        metadata={[
          <MetadataItem
            sx={{
              fontSize: MetadataFontSize(isMobile),
              height: MetadataFontSize(isMobile),
            }}
            key="pages"
          >
            {book.pageNumber &&
              book.pageNumber !== 0 &&
              `${book.pageNumber} pages`}
          </MetadataItem>,
          <MetadataItem
            sx={{
              fontSize: "0.75rem",
              color: isReleased ? "success.light" : "info.light",
            }}
            key="release-date"
          >
            {isReleased
              ? "Book is Released!"
              : `Release ${timeUntilRelease} ...`}
          </MetadataItem>,
        ]}
      />
      <Dialog onClose={() => setMenuOpen(false)} open={menuOpen} maxWidth="xs">
        <List sx={{ margin: "0 auto" }}>
          <ListItem
            disablePadding
            component={Link}
            href={LocationUrl(book.location!)}
            target="_blank"
            rel="noreferrer"
          >
            <ListItemButton onClick={() => setMenuOpen(false)}>
              <ListItemAvatar>
                <Avatar>
                  <OpenInNewIcon color="primary" />
                </Avatar>
              </ListItemAvatar>
              <ListItemText primary="Open in Browser" />
            </ListItemButton>
          </ListItem>
          <ListItem disablePadding>
            <ListItemButton onClick={onClickAddToLibrary}>
              <ListItemAvatar>
                <Avatar>
                  <AddToPhotosIcon color="primary" />
                </Avatar>
              </ListItemAvatar>
              <ListItemText primary="Add to Library" />
            </ListItemButton>
          </ListItem>
        </List>
      </Dialog>
    </>
  );
};

const MetadataFontSize = (isMobile: boolean): string => {
  if (isMobile) {
    // mobile
    return "0.6rem !important";
  }

  // desktop
  return "0.7rem !important";
};
