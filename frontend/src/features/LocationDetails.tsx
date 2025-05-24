/* eslint-disable no-magic-numbers */
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import DownloadIcon from "@mui/icons-material/Download";
import { Button, Link, Stack } from "@mui/material";
import { type ReactElement } from "react";

import { useNotImplemented } from "../hooks/useNotImplemented";
import type {
  LocationDTO,
  LocationTypeDTO,
} from "../lib/api/KapitelShelf.Api/api";
import {
  FileUrl,
  LocalTypes,
  LocationTypeToString,
  RealWorldTypes,
  UrlTypes,
} from "../utils/LocationUtils";

const RealWorldTypeToText = (type: LocationTypeDTO | undefined): string => {
  switch (type) {
    case 0:
      return "as Physical";

    case 5:
      return "in the Library";

    default:
      return "";
  }
};

interface LocationDetailsProps {
  bookId: string | undefined;
  location: LocationDTO;
}

const LocationDetails = ({
  bookId,
  location,
}: LocationDetailsProps): ReactElement => {
  const trigger = useNotImplemented();
  if (RealWorldTypes.includes(location.type ?? -1)) {
    return (
      <Stack direction="row" justifyContent="center" mt="15px">
        <Button
          variant="outlined"
          startIcon={<AutoStoriesIcon />}
          disabled
          sx={{
            color: "inherit !important",
            borderColor: "inherit !important",
            backgroundColor: "inherit !important",
          }}
        >
          Available {RealWorldTypeToText(location.type)}
        </Button>
      </Stack>
    );
  } else if (LocalTypes.includes(location.type ?? -1)) {
    const fileUrl = FileUrl({
      id: bookId,
      location: {
        fileInfo: location.fileInfo,
      },
    });

    return (
      <Stack direction="row" justifyContent="center" spacing={2} mt="15px">
        <Button
          variant="outlined"
          startIcon={<AutoStoriesIcon />}
          onClick={() => trigger(107)}
          disabled={fileUrl === undefined}
        >
          Read
        </Button>
        <Button
          variant="outlined"
          startIcon={<DownloadIcon />}
          component={Link}
          href={fileUrl}
          target="_blank"
          rel="noreferer"
          disabled={fileUrl === undefined}
        >
          Download
        </Button>
      </Stack>
    );
  } else if (UrlTypes.includes(location.type ?? -1)) {
    return (
      <Stack direction="row" justifyContent="center" mt="15px">
        <Button
          variant="outlined"
          component={Link}
          startIcon={<AutoStoriesIcon />}
          href={location.url ?? undefined}
          target="_blank"
          rel="noreferer"
        >
          Read on {LocationTypeToString[location.type ?? -1]}
        </Button>
      </Stack>
    );
  }

  return <></>;
};

export default LocationDetails;
