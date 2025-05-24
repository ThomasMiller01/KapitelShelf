import { Box } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useCallback } from "react";

import FileUploadDropzone from "../../components/base/FileUploadDropzone";
import FancyText from "../../components/FancyText";
import { useMobile } from "../../hooks/useMobile";
import { useNotification } from "../../hooks/useNotification";
import { booksApi } from "../../lib/api/KapitelShelf.Api";
import { BookFileTypes } from "../../utils/FileTypesUtils";

const ImportBookPage = (): ReactElement => {
  const { isMobile } = useMobile();
  const { triggerNavigate } = useNotification();

  const { mutateAsync: mutateImportBook } = useMutation({
    mutationKey: ["import-book"],
    mutationFn: async (file: File) => {
      const { data } = await booksApi.booksImportPost(file);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing Book",
      },
    },
  });

  const importFile = useCallback(
    async (file: File): Promise<void> => {
      const createdBook = await mutateImportBook(file);
      if (
        createdBook?.id === undefined ||
        createdBook.title === undefined ||
        createdBook.title === null
      ) {
        // only continue, if creation was successful
        return;
      }

      triggerNavigate({
        operation: "Imported",
        itemName: createdBook.title,
        url: `/library/books/${createdBook.id}`,
      });
    },
    [mutateImportBook, triggerNavigate]
  );

  const onImport = useCallback(
    async (files: File[]): Promise<void> => {
      // use for instead of foreach, because foreach doesnt wait for the awaits
      for (const file of files) {
        await importFile(file);
      }
    },
    [importFile]
  );

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
          onFilesChange={onImport}
        />
      </Box>
    </Box>
  );
};

export default ImportBookPage;
