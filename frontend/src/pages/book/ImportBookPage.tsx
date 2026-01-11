import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { Box, Divider } from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { type ReactElement, useCallback } from "react";
import { NavLink } from "react-router-dom";

import { ButtonWithTooltip } from "../../components/base/ButtonWithTooltip";
import FileUploadDropzone from "../../components/base/FileUploadDropzone";
import FancyText from "../../components/FancyText";
import { useApi } from "../../contexts/ApiProvider";
import { useMobile } from "../../hooks/useMobile";
import { useNotification } from "../../hooks/useNotification";
import { useUserProfile } from "../../hooks/useUserProfile";
import type { ImportResultDTO } from "../../lib/api/KapitelShelf.Api/api";
import { BookFileTypes } from "../../utils/FileTypesUtils";

const ImportBookPage = (): ReactElement => {
  const { isMobile } = useMobile();
  const { clients } = useApi();
  const { profile } = useUserProfile();
  const { triggerNavigate, triggerError } = useNotification();
  const queryClient = useQueryClient();

  const { mutateAsync: mutateImportBook } = useMutation({
    mutationKey: ["import-book"],
    mutationFn: async (file: File) => {
      const { data } = await clients.books.booksImportPost(profile?.id, file);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing Book",
      },
    },
  });

  const onSingleImportResult = useCallback(
    async (importResult: ImportResultDTO): Promise<void> => {
      // only a single book was imported

      // check if the import was sucessful
      if (importResult.errors && importResult.errors.length > 0) {
        triggerError({
          operation: "Importing Book",
          errorMessage: importResult.errors[0],
        });
        return;
      }

      if (importResult.importedBooks && importResult.importedBooks.length > 0) {
        triggerNavigate({
          operation: "Imported the following book",
          itemName: importResult.importedBooks[0].title ?? "",
          url: `/library/books/${importResult.importedBooks[0].id}`,
        });
      }
    },
    [triggerError, triggerNavigate]
  );

  const onBulkImportResult = useCallback(
    async (importResult: ImportResultDTO): Promise<void> => {
      // multiple books were imported

      // check for errors in the import result
      if (importResult.errors && importResult.errors.length > 0) {
        triggerError({
          operation: "Importing Books",
          errorMessage: `${importResult.errors.length} Error(s) occurred`,
        });
        setTimeout(() => {
          queryClient.invalidateQueries({ queryKey: ["notifications-stats"] });
        }, 500);
      }

      // if there were any books imported, offer navigation to the library
      if (importResult.importedBooks && importResult.importedBooks.length > 0) {
        triggerNavigate({
          operation: "Importing successful",
          itemName: `${importResult.importedBooks.length} Book(s)`,
          url: `/library`,
        });
      }
    },
    [triggerError, triggerNavigate]
  );

  const importFile = useCallback(
    async (file: File): Promise<void> => {
      const importResult = await mutateImportBook(file);

      if (importResult.isBulkImport) {
        onBulkImportResult(importResult);
      } else {
        onSingleImportResult(importResult);
      }
    },
    [mutateImportBook, onSingleImportResult, onBulkImportResult]
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
        height={isMobile ? "300px" : "220px"}
      >
        <FileUploadDropzone
          accept={BookFileTypes}
          height="100%"
          onFilesChange={onImport}
        />
      </Box>
      <Box
        minWidth="50%"
        maxWidth={isMobile ? "100%" : "50%"}
        margin="30px auto 0 auto"
      >
        <Divider sx={{ my: 5 }}>OR</Divider>
      </Box>
      <Box width="fit-content" margin="30px auto 0 auto">
        <ButtonWithTooltip
          component={NavLink}
          tooltip="Import a book from Amazon via its ASIN"
          variant="contained"
          startIcon={<CloudUploadIcon />}
          to="/library/books/import-from-asin"
        >
          Import From ASIN
        </ButtonWithTooltip>
      </Box>
    </Box>
  );
};

export default ImportBookPage;
