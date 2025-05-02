import FileUploadIcon from "@mui/icons-material/FileUpload";
import type { ButtonProps } from "@mui/material/Button";
import Button from "@mui/material/Button";
import { styled } from "@mui/material/styles";
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
  children: ReactNode;
  onFileChange?: (file: File) => void;
}

const FileUploadButton = ({
  children,
  onFileChange,
  ...props
}: FileUploadButtonProps): ReactElement => {
  const [currentFile, setCurrentFile] = useState<File>();
  useEffect(() => {
    if (currentFile === undefined || currentFile === null) {
      return;
    }

    if (onFileChange !== undefined) {
      onFileChange(currentFile);
    }
  }, [currentFile, onFileChange]);

  return (
    <Button
      component="label"
      role={undefined}
      variant="contained"
      tabIndex={-1}
      startIcon={<FileUploadIcon />}
      {...props}
    >
      {children}
      <VisuallyHiddenInput
        type="file"
        onChange={(event) =>
          event.target.files && setCurrentFile(event.target.files[0])
        }
        multiple
      />
    </Button>
  );
};

export default FileUploadButton;
