import { yupResolver } from "@hookform/resolvers/yup";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { Box, Button, Link, Stack, TextField } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import FancyText from "../../components/FancyText";
import { useApi } from "../../contexts/ApiProvider";
import { useNotification } from "../../hooks/useNotification";
import type { ASINFormValues } from "../../lib/schemas/ASINSchema";
import { ASINSchema } from "../../lib/schemas/ASINSchema";

const ImportBookFromASINPage = (): ReactElement => {
  const { clients } = useApi();
  const { triggerNavigate, triggerError } = useNotification();

  const { mutateAsync: mutateImportBookFromAsin } = useMutation({
    mutationKey: ["import-book-from-asin"],
    mutationFn: async (asin: string) => {
      const { data } = await clients.books.booksImportAsinPost(asin);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing book",
      },
    },
  });

  const methods = useForm({
    resolver: yupResolver(ASINSchema),
    mode: "onBlur",
    defaultValues: {
      asin: "",
    },
  });

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = methods;

  const onSubmit = async (data: ASINFormValues): Promise<void> => {
    const importResult = await mutateImportBookFromAsin(data.asin);

    // check if the import was sucessful
    if (importResult.errors && importResult.errors.length > 0) {
      triggerError({
        operation: "Importing Book",
        errorMessage: importResult.errors[0],
      });
      reset();
      return;
    }

    if (importResult.importedBooks && importResult.importedBooks.length > 0) {
      triggerNavigate({
        operation: "Imported the following book",
        itemName: importResult.importedBooks[0].title ?? "",
        url: `/library/books/${importResult.importedBooks[0].id}`,
      });
      reset();
    }
  };

  return (
    <Box padding="24px">
      <FancyText variant="h4" textAlign="center" gutterBottom>
        Import a book from ASIN
      </FancyText>
      <Box margin="60px auto" width="fit-content">
        <FormProvider {...methods}>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Stack
              direction={{ xs: "column", md: "row" }}
              spacing={4}
              alignItems="center"
            >
              {/* ASIN */}
              <Controller
                name="asin"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="ASIN"
                    variant="filled"
                    autoFocus
                    error={Boolean(errors.asin)}
                    helperText={errors.asin?.message || <LearnAboutASINsText />}
                  />
                )}
              />

              <Button
                variant="contained"
                startIcon={<CloudUploadIcon />}
                type="submit"
                disabled={!isValid}
              >
                Import Book
              </Button>
            </Stack>
          </form>
        </FormProvider>
      </Box>
    </Box>
  );
};

const LearnAboutASINsText = (): ReactElement => (
  <>
    Learn about ASINs on{" "}
    <Link
      href="https://sell.amazon.com/blog/what-is-an-asin"
      target="_blank"
      rel="noreferrer"
    >
      amazon.com
    </Link>
    .
  </>
);

export default ImportBookFromASINPage;
