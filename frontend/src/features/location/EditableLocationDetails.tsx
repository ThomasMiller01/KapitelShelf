import {
  Box,
  FormControl,
  FormHelperText,
  InputLabel,
  Link,
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

import FileUploadButton from "../../components/base/FileUploadButton";
import { useMobile } from "../../hooks/useMobile";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import type { BookFormValues } from "../../lib/schemas/BookSchema";
import { BookFileUrl, RenameFile, UrlToFile } from "../../utils/FileUtils";
import {
  LocalTypes,
  LocationTypeToString,
  UrlTypes,
} from "../../utils/LocationUtils";

interface EditableLocationDetailsProps {
  initial?: BookDTO;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: Control<any>;
  onFileChange?: (file?: File) => void;
}

const EditableLocationDetails = ({
  initial,
  control,
  onFileChange,
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
        <LocationSettings
          initial={initial}
          control={control}
          onFileChange={onFileChange}
        />
      </Stack>
    </Box>
  );
};

interface LocationSettingsProps {
  initial?: BookDTO;
  control: Control;
  onFileChange?: (file?: File) => void;
}

const LocationSettings = ({
  initial,
  control,
  onFileChange,
}: LocationSettingsProps): ReactElement => {
  const locationType = useWatch({
    control,
    name: "locationType",
    defaultValue: initial?.location?.type,
  });
  const { setValue } = useFormContext<BookFormValues>();

  const [locationTypeInt, setLocationTypeInt] = useState(1);
  useEffect(() => {
    const lti = parseInt(locationType);
    setLocationTypeInt(lti);

    if (LocalTypes.includes(lti)) {
      setValue("locationUrl", "");
    } else if (UrlTypes.includes(lti)) {
      if (onFileChange !== undefined) {
        onFileChange(undefined);
      }
    } else {
      setValue("locationUrl", "");
      if (onFileChange !== undefined) {
        onFileChange(undefined);
      }
    }
  }, [locationType, setValue, onFileChange]);

  if (LocalTypes.includes(locationTypeInt)) {
    return (
      <LocalLocationSettings initial={initial} onFileChange={onFileChange} />
    );
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

interface LocalLocationSettingsProps {
  initial?: BookDTO;
  onFileChange?: (file: File) => void;
}

const LocalLocationSettings = ({
  initial,
  onFileChange: onFileChangeEvent,
}: LocalLocationSettingsProps): ReactElement => {
  const [currentFile, setCurrentFile] = useState<File>();
  const [currentFileUrl, setCurrentFileUrl] = useState<string>();

  useEffect(() => {
    // set initial book file
    const bookFileUrl = BookFileUrl(initial);
    if (bookFileUrl !== undefined) {
      UrlToFile(bookFileUrl).then((file) => {
        const renamedFile = RenameFile(
          file,
          initial?.location?.fileInfo?.fileName ?? "book.epub"
        );
        setCurrentFile(renamedFile);
      });
    }
  }, [initial]);

  // download the current file
  useEffect(() => {
    if (currentFile === undefined) {
      return;
    }

    const url = URL.createObjectURL(currentFile);
    setCurrentFileUrl(url);
    return (): void => URL.revokeObjectURL(url);
  }, [currentFile, setCurrentFileUrl]);

  const onFileChange = useCallback(
    (file: File) => {
      setCurrentFile(file);

      if (onFileChangeEvent === undefined) {
        return;
      }

      onFileChangeEvent(file);
    },
    [onFileChangeEvent]
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
      <Typography
        component={currentFileUrl ? Link : Typography}
        href={currentFileUrl}
        download={currentFile?.name}
      >
        {currentFile?.name}
      </Typography>
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
  } = useFormContext<BookFormValues>();

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
