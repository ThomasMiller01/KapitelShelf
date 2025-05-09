import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import {
  Box,
  Stack,
  type SxProps,
  type Theme,
  Typography,
  useTheme,
} from "@mui/material";
import { useSnackbar } from "notistack";
import { type ReactElement, useMemo } from "react";
import { useDropzone } from "react-dropzone";

export const baseStyle = (theme: Theme): SxProps<Theme> => ({
  flex: 1,
  display: "flex",
  alignItems: "center",
  // eslint-disable-next-line no-magic-numbers
  padding: theme.spacing(2.5),
  borderWidth: 2,
  borderRadius: "10px",
  borderColor: theme.palette.divider,
  borderStyle: "dashed",
  backgroundColor: theme.palette.background.paper,
  color: theme.palette.text.secondary,
  outline: "none",
  transition: "border .24s ease-in-out",
  cursor: "pointer",
});

export const focusedStyle = (theme: Theme): SxProps<Theme> => ({
  borderColor: theme.palette.primary.main,
});

export const acceptStyle = (theme: Theme): SxProps<Theme> => ({
  borderColor: theme.palette.success.main,
});

export const rejectStyle = (theme: Theme): SxProps<Theme> => ({
  borderColor: theme.palette.error.main,
});

interface FileUploadDropzoneProps {
  onFileChange?: (file: File) => void;
  onFilesChange?: (files: File[]) => void;
  accept?: string[];
  height?: string;
}

const FileUploadDropzone = ({
  onFileChange,
  onFilesChange,
  accept = [],
  height = "fit-content",
}: FileUploadDropzoneProps): ReactElement => {
  const { enqueueSnackbar } = useSnackbar();
  const theme = useTheme();

  const onDrop = (files: File[]): void => {
    if (files === undefined || files === null || files.length === 0) {
      return;
    }

    const acceptedFiles: File[] = [];
    files.forEach((file) => {
      if (
        accept.length > 0 &&
        !accept.includes(file.type) &&
        accept.filter((x) => file.name.endsWith(x)).length === 0
      ) {
        enqueueSnackbar(`Invalid file type, allowed: ${accept.join(", ")}`, {
          variant: "warning",
        });
        return;
      }

      acceptedFiles.push(file);
    });

    if (onFilesChange !== undefined) {
      onFilesChange(acceptedFiles);
    } else if (onFileChange !== undefined && acceptedFiles.length > 0) {
      onFileChange(acceptedFiles[0]);
    }
  };

  const { getRootProps, getInputProps, isFocused, isDragAccept, isDragReject } =
    useDropzone({
      onDrop,
      accept: accept.reduce(
        (dict, key) => ({ [key]: [], ...dict }),
        {} as Record<string, string[]>
      ),
      maxFiles: onFilesChange !== undefined ? 0 : 1,
      multiple: onFilesChange !== undefined,
    });

  const style = useMemo(
    () => ({
      ...baseStyle(theme),
      ...(isFocused ? focusedStyle(theme) : {}),
      ...(isDragAccept ? acceptStyle(theme) : {}),
      ...(isDragReject ? rejectStyle(theme) : {}),
    }),
    [isFocused, isDragAccept, isDragReject, theme]
  );

  return (
    <Box {...getRootProps()} sx={style} justifyContent="center" height={height}>
      <input {...getInputProps()} />
      <Stack spacing={4} alignItems="center">
        <Typography textAlign="center" fontSize="1.1rem">
          Drag & drop {onFilesChange !== undefined ? "files" : "a file"} here,
          or click to select {onFilesChange !== undefined ? "files" : "a file"}
        </Typography>
        <CloudUploadIcon sx={{ fontSize: "2.5rem" }} />
      </Stack>
    </Box>
  );
};

export default FileUploadDropzone;
