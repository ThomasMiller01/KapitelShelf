import FileUploadIcon from "@mui/icons-material/FileUpload";
import type { ButtonProps } from "@mui/material/Button";
import Button from "@mui/material/Button";
import { styled } from "@mui/material/styles";
import { useSnackbar } from "notistack";
import { type ReactElement, type ReactNode, useEffect, useState } from "react";

const VisuallyHiddenInput = styled("input")({
  clip: "rect(0 0 0 0)",
  clipPath: "inset(50%)",
  height: 1,
  overflow: "hidden",
  position: "absolute",
  bottom: 0,
  left: 0,
  whiteSpace: "nowrap",
  width: 1,
});

interface FileUploadButtonProps extends ButtonProps {
  onFileChange?: (file: File) => void;
  accept?: string[];
  children: ReactNode;
}

const FileUploadButton = ({
  onFileChange,
  accept = [],
  children,
  startIcon = <FileUploadIcon />,
  ...props
}: FileUploadButtonProps): ReactElement => {
  const { enqueueSnackbar } = useSnackbar();

  const [currentFile, setCurrentFile] = useState<File>();
  useEffect(() => {
    if (currentFile === undefined || currentFile === null) {
      return;
    }

    if (accept.length > 0 && !accept.includes(currentFile.type)) {
      enqueueSnackbar(`Invalid file type, allowed: ${accept.join(", ")}`, {
        variant: "warning",
      });

      setCurrentFile(undefined);
      return;
    }

    if (onFileChange !== undefined) {
      onFileChange(currentFile);
    }
  }, [currentFile, accept, onFileChange, enqueueSnackbar]);

  return (
    <Button
      component="label"
      role={undefined}
      variant="contained"
      tabIndex={-1}
      startIcon={startIcon}
      {...props}
    >
      {children}
      <VisuallyHiddenInput
        type="file"
        onChange={(event) =>
          event.target.files && setCurrentFile(event.target.files[0])
        }
        accept={accept.join(",")}
      />
    </Button>
  );
};

export default FileUploadButton;
