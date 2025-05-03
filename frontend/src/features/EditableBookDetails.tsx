import CategoryIcon from "@mui/icons-material/Category";
import ImportContactsIcon from "@mui/icons-material/ImportContacts";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box, Button, Divider, Grid, Stack, TextField } from "@mui/material";
import { DateField } from "@mui/x-date-pickers/DateField";
import dayjs from "dayjs";
import { type ReactElement, useEffect, useState } from "react";

import bookCover from "../assets/books/nocover.png";
import FileUploadButton from "../components/base/FileUploadButton";
import ItemList from "../components/base/ItemList";
import { useEditableBook } from "../hooks/useEditableBook";
import { useMobile } from "../hooks/useMobile";
import { useNotImplemented } from "../hooks/useNotImplemented";
import EditableLocationDetails from "./EditableLocationDetails";

const EditableBookDetails = (): ReactElement => {
  const { isMobile } = useMobile();
  const trigger = useNotImplemented();

  const {
    book,
    handleTextChange,
    handleNumberChange,
    handleReleaseDateChange,
    handleAuthorChange,
    handleSeriesChange,
    setCategories,
    setTags,
  } = useEditableBook();

  const [authorInput, setAuthorInput] = useState("");
  useEffect(() => {
    handleAuthorChange(authorInput);
  }, [authorInput, handleAuthorChange]);

  return (
    <Box mt="15px">
      <Grid container spacing={{ xs: 1, md: 4 }} columns={11}>
        <Grid size={{ xs: 0, md: 1 }}></Grid>

        <Grid size={{ xs: 11, md: 2.5 }}>
          <Stack spacing={1} alignItems="center">
            <img
              src={bookCover}
              alt={"Book Cover"}
              style={{
                width: "100%",
                borderRadius: 2,
                boxShadow: "3",
                marginLeft: isMobile ? "auto" : 0,
                marginRight: isMobile ? "auto" : 0,
              }}
            />
            <FileUploadButton>Upload Cover</FileUploadButton>
          </Stack>
        </Grid>

        <Grid size={{ xs: 11, md: 6.5 }} mt="20px">
          <Stack spacing={2} width={isMobile ? "100%" : "60%"}>
            <TextField
              label="Title"
              variant="filled"
              value={book.title ?? ""}
              onChange={handleTextChange("title")}
            />

            <TextField
              label="Description"
              multiline
              rows={4}
              variant="filled"
              value={book.description ?? ""}
              onChange={handleTextChange("description")}
            />

            <Stack direction="row" spacing={2} justifyContent="space-between">
              <TextField
                label="Page Number"
                variant="filled"
                type="number"
                value={book.pageNumber ?? ""}
                onChange={handleNumberChange("pageNumber")}
              />

              <DateField
                label="Release Date"
                variant="filled"
                value={book.releaseDate ? dayjs(book.releaseDate) : null}
                onChange={handleReleaseDateChange}
              />
            </Stack>

            <TextField
              label="Author"
              variant="filled"
              value={authorInput}
              onChange={(e) => setAuthorInput(e.target.value)}
            />

            <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
              <TextField
                label="Series"
                variant="filled"
                fullWidth
                value={book.series?.name ?? ""}
                onChange={handleSeriesChange}
              />
              <TextField
                label="Volume"
                variant="filled"
                type="number"
                value={book.seriesNumber ?? ""}
                onChange={handleNumberChange("seriesNumber")}
              />
            </Stack>

            {/* TODO location */}
            <EditableLocationDetails />

            <Stack direction="row" spacing={1} alignItems="start">
              <CategoryIcon sx={{ mr: "5px !important" }} />
              <ItemList
                itemName="Category"
                initial={book.categories?.map((x) => x.name ?? "") ?? []}
                onChange={setCategories}
              />
            </Stack>

            <Stack direction="row" spacing={1} alignItems="start">
              <LocalOfferIcon sx={{ mr: "5px !important" }} />
              <ItemList
                itemName="Tag"
                variant="outlined"
                initial={book.tags?.map((x) => x.name ?? "") ?? []}
                onChange={setTags}
              />
            </Stack>

            <Divider />

            <Stack
              direction={{ xs: "column", md: "row" }}
              spacing={2}
              justifyContent="space-between"
              alignItems="end"
              mt="15px"
            >
              <Button
                variant="outlined"
                startIcon={<ImportContactsIcon />}
                sx={{
                  alignItems: "start",
                  width: "fit-content",
                  height: "fit-content",
                  whiteSpace: "nowrap",
                }}
                onClick={() => trigger(63)}
              >
                Import Metadata
              </Button>
              <Button
                variant="contained"
                startIcon={<NoteAddIcon />}
                onClick={() => console.log(book)}
                sx={{
                  alignItems: "start",
                  width: "fit-content",
                  height: "fit-content",
                  whiteSpace: "nowrap",
                }}
              >
                Create Book
              </Button>
            </Stack>
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export default EditableBookDetails;
