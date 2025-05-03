import CategoryIcon from "@mui/icons-material/Category";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import { Box, Grid, Stack, TextField } from "@mui/material";
import { DateField } from "@mui/x-date-pickers/DateField";
import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import FileUploadButton from "../components/base/FileUploadButton";
import ItemList from "../components/base/ItemList";
import { useMobile } from "../hooks/useMobile";
import EditableLocationDetails from "./EditableLocationDetails";

const EditableBookDetails = (): ReactElement => {
  const { isMobile } = useMobile();

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
            <TextField label="Title" variant="filled" />

            <TextField
              label="Description"
              multiline
              rows={4}
              variant="filled"
            />

            <Stack direction="row" spacing={2} justifyContent="space-between">
              <TextField label="Page Number" variant="filled" type="number" />

              <DateField label="Release Date" variant="filled" />
            </Stack>

            <TextField label="Author" variant="filled" />

            <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
              <TextField label="Series" variant="filled" fullWidth />
              <TextField label="Volume" variant="filled" type="number" />
            </Stack>

            <EditableLocationDetails />

            <Stack direction="row" spacing={1} alignItems="start">
              <CategoryIcon sx={{ mr: "5px !important" }} />
              <ItemList
                itemName="Category"
                onChange={(items: string[]) => {}}
              />
            </Stack>

            <Stack direction="row" spacing={1} alignItems="start">
              <LocalOfferIcon sx={{ mr: "5px !important" }} />
              <ItemList
                itemName="Tag"
                onChange={(items: string[]) => {}}
                variant="outlined"
              />
            </Stack>
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export default EditableBookDetails;
