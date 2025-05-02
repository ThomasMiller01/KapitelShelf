import {
  Box,
  Button,
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useState } from "react";

import {
  LocalTypes,
  LocationTypeToString,
  UrlTypes,
} from "../utils/LocationTypeUtils";

const EditableLocationDetails = (): ReactElement => {
  const [locationType, setLocationType] = useState("1");
  return (
    <Box>
      <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
        <LocationSelection
          locationType={locationType}
          setLocationType={setLocationType}
        />
        <LocationSettings locationType={locationType} />
      </Stack>
    </Box>
  );
};

interface LocationSelectionProps {
  locationType: string;
  setLocationType: (locationType: string) => void;
}

const LocationSelection = ({
  locationType,
  setLocationType,
}: LocationSelectionProps): ReactElement => (
  <Box>
    <FormControl variant="filled" sx={{ width: 150 }}>
      <InputLabel>Book Location</InputLabel>
      <Select
        value={locationType}
        onChange={({ target: { value } }) => setLocationType(value)}
      >
        <MenuItem value="0">Physical</MenuItem>
        <MenuItem value="1">KapitelShelf</MenuItem>
        <MenuItem value="2">Kindle</MenuItem>
        <MenuItem value="3">Skoobe</MenuItem>
        <MenuItem value="4">Onleihe</MenuItem>
        <MenuItem value="5">Library</MenuItem>
      </Select>
      {UrlTypes.includes(parseInt(locationType)) && (
        <FormHelperText> </FormHelperText>
      )}
    </FormControl>
  </Box>
);

interface LocationSettingsProps {
  locationType: string;
}

const LocationSettings = ({
  locationType,
}: LocationSettingsProps): ReactElement => {
  const locationTypeInt = parseInt(locationType);

  if (LocalTypes.includes(locationTypeInt)) {
    return (
      <Stack
        direction={{ xs: "column", md: "row" }}
        spacing={{ xs: 1, md: 2 }}
        width="100%"
        alignItems="center"
      >
        <Button
          variant="contained"
          sx={{ width: "fit-content", height: "fit-content" }}
        >
          Upload Book
        </Button>
        <Typography>TODO: Current Book File</Typography>
      </Stack>
    );
  } else if (UrlTypes.includes(locationTypeInt)) {
    return (
      <Box width="100%">
        <TextField
          label="Url"
          helperText={`Link to the book on ${
            LocationTypeToString[locationTypeInt ?? -1]
          }`}
          variant="filled"
          fullWidth
        />
      </Box>
    );
  }

  return <></>;
};

export default EditableLocationDetails;
