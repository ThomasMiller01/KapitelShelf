/* eslint-disable no-magic-numbers */
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import DownloadIcon from "@mui/icons-material/Download";
import { Button, Stack } from "@mui/material";
import { type ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import type {
  LocationDTO,
  LocationTypeDTO,
} from "../lib/api/KapitelShelf.Api/api";
import {
  LocalTypes,
  LocationTypeToString,
  RealWorldTypes,
  UrlTypes,
} from "../utils/LocationTypeUtils";

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

const EncodeBookFileUri = (uri: string): string => {
  const normalized = uri.replace(/\\/g, "/");
  return normalized
    .split("/")
    .map((segment) => encodeURIComponent(segment))
    .join("/");
};

interface LocationDetailsProps {
  location: LocationDTO;
}

const LocationDetails = ({ location }: LocationDetailsProps): ReactElement => {
  const navigate = useNavigate();

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
    return (
      <Stack direction="row" justifyContent="center" spacing={2} mt="15px">
        <Button
          variant="outlined"
          startIcon={<AutoStoriesIcon />}
          onClick={() => navigate(`/library/books/<bookid>/read`)}
        >
          Read
        </Button>
        <Button
          variant="outlined"
          startIcon={<DownloadIcon />}
          onClick={() =>
            navigate(
              `/data/${EncodeBookFileUri(location.fileInfo?.filePath ?? "")}`
            )
          }
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
          startIcon={<AutoStoriesIcon />}
          onClick={() => (window.location.href = location.url ?? "")}
        >
          Read on {LocationTypeToString[location.type ?? -1]}
        </Button>
      </Stack>
    );
  }

  return <></>;
};

export default LocationDetails;
