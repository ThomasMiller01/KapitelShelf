import UploadIcon from "@mui/icons-material/Upload";
import { Box, Button, Stack, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";

import FileUploadDropzone from "../components/base/FileUploadDropzone";
import FancyText from "../components/FancyText";
import { useMobile } from "../hooks/useMobile";
import { useNotification } from "../hooks/useNotification";
import { booksApi } from "../lib/api/KapitelShelf.Api";
import { BookFileTypes } from "../utils/FileTypesUtils";

const ImportBookPage = (): ReactElement => {
  const { isMobile } = useMobile();
  const { triggerNavigate } = useNotification();

  const [currentFile, setCurrentFile] = useState<File>();

  const { mutateAsync: mutateImportBook } = useMutation({
    mutationKey: ["import-book"],
    mutationFn: async () => {
      const { data } = await booksApi.booksImportPost(currentFile);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing Book",
      },
    },
  });

  const onImport = async (): Promise<void> => {
    const createdBook = await mutateImportBook();
    if (
      createdBook?.id === undefined ||
      createdBook.title === undefined ||
      createdBook.title === null
    ) {
      // only continue, if creation was successful
      return;
    }

    triggerNavigate({
      operation: "Imported the following book",
      itemName: createdBook.title,
      url: `/library/books/${createdBook.id}`,
    });
  };

  return (
    <Box padding="24px">
      <FancyText variant="h4" textAlign="center" gutterBottom>
        Import a new Book
      </FancyText>
      <Box
        minWidth="50%"
        maxWidth={isMobile ? "100%" : "50%"}
        margin="30px auto 0 auto"
        height="300px"
      >
        <FileUploadDropzone
          accept={BookFileTypes}
          height={isMobile ? "300px" : "220px"}
          onFileChange={setCurrentFile}
        />
        <Stack
          direction={{ xs: "column", md: "row" }}
          spacing={{ xs: 4, md: 2 }}
          justifyContent="space-between"
          alignItems="center"
          mt="30px"
        >
          {currentFile ? (
            <Stack
              direction={{ xs: "column", md: "row" }}
              spacing={0.5}
              alignItems={{ xs: "start", md: "center" }}
            >
              <Typography variant="button" color="text.secondary">
                Selected:
              </Typography>
              <Typography variant="body2" sx={{ wordBreak: "break-all" }}>
                {currentFile?.name}
              </Typography>
            </Stack>
          ) : (
            <Box />
          )}
          <Button
            variant="contained"
            sx={{ width: "fit-content" }}
            startIcon={<UploadIcon />}
            disabled={currentFile === undefined}
            onClick={onImport}
          >
            Import
          </Button>
        </Stack>
      </Box>
    </Box>
  );
};

export default ImportBookPage;
