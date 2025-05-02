import {
  Box,
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useCallback, useState } from "react";

import FileUploadButton from "../components/base/FileUploadButton";
import { useNotImplemented } from "../hooks/useNotImplemented";
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
      <InputLabel>Location</InputLabel>
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
    return <LocalLocationSettings />;
  } else if (UrlTypes.includes(locationTypeInt)) {
    return <UrlLocationSettings locationTypeInt={locationTypeInt} />;
  }

  return <></>;
};

const LocalLocationSettings = (): ReactElement => {
  const [currentFile, setCurrentFile] = useState<File>();
  const trigger = useNotImplemented();

  const onFileChange = useCallback(
    (file: File) => {
      setCurrentFile(file);
      // eslint-disable-next-line no-magic-numbers
      trigger(58);
    },
    [trigger]
  );

  return (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={{ xs: 1, md: 2 }}
      width="100%"
      alignItems="center"
    >
      <Box>
        <FileUploadButton
          onFileChange={onFileChange}
          sx={{ whiteSpace: "nowrap" }}
        >
          Upload Book
        </FileUploadButton>
      </Box>
      <Typography>{currentFile?.name}</Typography>
    </Stack>
  );
};

interface UrlLocationSettingsProps {
  locationTypeInt: number;
}

const UrlLocationSettings = ({
  locationTypeInt,
}: UrlLocationSettingsProps): ReactElement => (
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

export default EditableLocationDetails;
