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
import { type ReactElement, useCallback, useEffect, useState } from "react";
import {
  type Control,
  Controller,
  useFormContext,
  useWatch,
} from "react-hook-form";

import FileUploadButton from "../components/base/FileUploadButton";
import { useMobile } from "../hooks/useMobile";
import { useNotImplemented } from "../hooks/useNotImplemented";
import type { CreateBookFormValues } from "../lib/schemas/CreateBookSchema";
import {
  LocalTypes,
  LocationTypeToString,
  UrlTypes,
} from "../utils/LocationUtils";

interface EditableLocationDetailsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: Control<any>;
}

const EditableLocationDetails = ({
  control,
}: EditableLocationDetailsProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <Box>
      <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
        <Box>
          {/* Location Type */}
          <Controller
            name="locationType"
            control={control}
            defaultValue="1"
            render={({ field }) => (
              <FormControl variant="filled" sx={{ width: 150 }}>
                <InputLabel>Location</InputLabel>
                <Select {...field}>
                  <MenuItem value="0">Physical</MenuItem>
                  <MenuItem value="1">KapitelShelf</MenuItem>
                  <MenuItem value="2">Kindle</MenuItem>
                  <MenuItem value="3">Skoobe</MenuItem>
                  <MenuItem value="4">Onleihe</MenuItem>
                  <MenuItem value="5">Library</MenuItem>
                </Select>
                {!isMobile && UrlTypes.includes(parseInt(field.value)) && (
                  <FormHelperText> </FormHelperText>
                )}
              </FormControl>
            )}
          />
        </Box>
        <LocationSettings control={control} />
      </Stack>
    </Box>
  );
};

interface LocationSettingsProps {
  control: Control;
}

const LocationSettings = ({ control }: LocationSettingsProps): ReactElement => {
  const locationType = useWatch({
    control,
    name: "locationType",
    defaultValue: "1",
  });
  const { setValue } = useFormContext<CreateBookFormValues>();

  const [locationTypeInt, setLocationTypeInt] = useState(1);
  useEffect(() => {
    const lti = parseInt(locationType);
    setLocationTypeInt(lti);

    if (LocalTypes.includes(lti)) {
      setValue("locationUrl", "");
    }
  }, [locationType, setValue]);

  if (LocalTypes.includes(locationTypeInt)) {
    return <LocalLocationSettings />;
  } else if (UrlTypes.includes(locationTypeInt)) {
    return (
      <UrlLocationSettings
        control={control}
        locationTypeInt={locationTypeInt}
      />
    );
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
        {/* Location File */}
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
  control: Control;
  locationTypeInt: number;
}

const UrlLocationSettings = ({
  control,
  locationTypeInt,
}: UrlLocationSettingsProps): ReactElement => {
  const {
    formState: { errors },
  } = useFormContext<CreateBookFormValues>();

  return (
    <Box width="100%">
      {/* Location Url */}
      <Controller
        name="locationUrl"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Url"
            error={Boolean(errors.locationUrl)}
            helperText={
              errors.locationUrl?.message ??
              `Link to the book on ${
                LocationTypeToString[locationTypeInt ?? -1]
              }`
            }
            variant="filled"
            fullWidth
          />
        )}
      />
    </Box>
  );
};

export default EditableLocationDetails;
