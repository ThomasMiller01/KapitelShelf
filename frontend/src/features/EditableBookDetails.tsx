import { Box, Button, Grid, Stack, TextField, Typography } from "@mui/material";
import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";

const EditableBookDetails = (): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <Box mt={isMobile ? "30px" : "15px"}>
      <Grid container spacing={{ xs: 1, md: 12 }} columns={11}>
        <Grid size={{ xs: 11, md: 3 }}>
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
            <Button variant="contained" sx={{ width: "fit-content" }}>
              Upload Cover
            </Button>
          </Stack>
        </Grid>

        <Grid size={{ xs: 11, md: 8 }} mt="20px">
          <Stack spacing={2} width={isMobile ? "100%" : "60%"}>
            <TextField label="Title" variant="filled" />

            <TextField
              label="Description"
              multiline
              rows={4}
              variant="filled"
            />

            <Stack direction="row" spacing={2} justifyContent="space-between">
              <TextField label="Page Number" variant="filled" />

              <TextField label="Release Date" variant="filled" />
            </Stack>

            <TextField label="Author" variant="filled" />

            <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
              <TextField label="Series" variant="filled" fullWidth />
              <TextField label="Volume" variant="filled" />
            </Stack>

            <Typography>TODO: Location</Typography>

            <Typography>TODO: Categories</Typography>

            <Typography>TODO: Tags</Typography>
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export default EditableBookDetails;
