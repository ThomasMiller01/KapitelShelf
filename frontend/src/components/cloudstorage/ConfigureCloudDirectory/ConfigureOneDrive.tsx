import { Box } from "@mui/material";
import React, { useEffect, useState } from "react";

interface ConfigureOneDriveProps {
  onDirectorySelect: (directory: string) => void;
}

export const ConfigureOneDrive: React.FC<ConfigureOneDriveProps> = ({
  onDirectorySelect,
}) => {
  const [directory, setDirectory] = useState("");

  useEffect(() => {
    onDirectorySelect(directory);
  }, [onDirectorySelect, directory]);

  return <Box>Select directory</Box>;
};
