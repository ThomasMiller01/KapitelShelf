import { yupResolver } from "@hookform/resolvers/yup";
import CategoryIcon from "@mui/icons-material/Category";
import ImportContactsIcon from "@mui/icons-material/ImportContacts";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import { Box, Button, Divider, Grid, Stack, TextField } from "@mui/material";
import { DateField } from "@mui/x-date-pickers/DateField";
import dayjs from "dayjs";
import type { ReactNode } from "react";
import { type ReactElement, useEffect, useState } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import { AutoComplete } from "../../components/base/AutoComplete";
import FileUploadButton from "../../components/base/FileUploadButton";
import ItemList from "../../components/base/ItemList";
import { useApi } from "../../contexts/ApiProvider";
import { useCoverImage } from "../../hooks/useCoverImage";
import { useMobile } from "../../hooks/useMobile";
import type { MetadataDTO } from "../../lib/api/KapitelShelf.Api/api";
import {
  type AuthorDTO,
  type BookDTO,
  type CategoryDTO,
  type SeriesDTO,
  type TagDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import { useProxyCover } from "../../lib/requests/books/useProxyCover";
import type { BookFormValues } from "../../lib/schemas/BookSchema";
import { BookSchema } from "../../lib/schemas/BookSchema";
import { ImageTypes } from "../../utils/FileTypesUtils";
import { BookFileUrl, RenameFile, UrlToFile } from "../../utils/FileUtils";
import { toLocationTypeDTO } from "../../utils/LocationUtils";
import EditableLocationDetails from "../location/EditableLocationDetails";
import MetadataDialog from "../metadata/MetadataDialog";

interface ActionProps {
  name: string;
  onClick: (book: BookDTO, cover: File, bookFile?: File) => void;
  icon?: ReactNode;
}

interface EditableBookDetailsProps {
  initial?: BookDTO;
  action?: ActionProps;
}

const EditableBookDetails = ({
  initial,
  action,
}: EditableBookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { clients } = useApi();

  let initialAuthor = initial?.author?.firstName ?? "";
  if (initial?.author?.lastName) {
    initialAuthor += " " + initial.author.lastName;
  }

  const methods = useForm({
    resolver: yupResolver(BookSchema),
    mode: "onBlur",
    defaultValues: {
      title: initial?.title ?? "",
      description: initial?.description ?? "",
      releaseDate: initial?.releaseDate ? dayjs(initial.releaseDate) : null,
      pageNumber: initial?.pageNumber,
      series: initial?.series?.name,
      seriesNumber: initial?.seriesNumber,
      author: initialAuthor,
      locationUrl: initial?.location?.url ?? "",
      locationType: initial?.location?.type ?? 1,
      categories:
        initial?.categories?.flatMap((x) => (x.name ? [x.name] : [])) ?? [],
      tags: initial?.tags?.flatMap((x) => (x.name ? [x.name] : [])) ?? [],
    },
  });

  const {
    control,
    handleSubmit,
    trigger: triggerValidation,
    formState: { errors, isValid },
  } = methods;

  useEffect(() => {
    const bookFileUrl = BookFileUrl(initial);
    if (bookFileUrl !== undefined) {
      UrlToFile(bookFileUrl).then((file) => {
        const renamedFile = RenameFile(
          file,
          initial?.location?.fileInfo?.fileName ?? "book",
        );
        setBookFile(renamedFile);
      });
    }

    // run validation on mount
    triggerValidation();
  }, [triggerValidation, initial]);

  const { coverImage, coverFile, updateCoverFromFile, onLoadingError } =
    useCoverImage({
      initial,
    });
  const [bookFile, setBookFile] = useState<File>();

  const onSubmit = (data: BookFormValues): void => {
    if (action === undefined || coverFile === undefined) {
      return;
    }

    const parseAuthor = (author: string): AuthorDTO => {
      let firstName: string;
      let lastName: string;

      // split author into firstName and lastName
      const parts = author.split(" ");
      if (parts.length === 1) {
        // no last name
        firstName = parts[0];
        lastName = "";
      } else {
        // everything except last goes into firstName
        firstName = parts.slice(0, -1).join(" ");
        lastName = parts[parts.length - 1];
      }

      return {
        firstName,
        lastName,
      };
    };

    const book: BookDTO = {
      title: data.title,
      description: data.description,
      releaseDate: data.releaseDate?.toISOString() ?? undefined,
      pageNumber: data.pageNumber ?? undefined,
      series: data.series ? ({ name: data.series } as SeriesDTO) : undefined,
      seriesNumber: data.seriesNumber ?? undefined,
      author: data.author ? parseAuthor(data.author) : undefined,
      location: {
        type: toLocationTypeDTO(data.locationType ?? -1),
        url: data.locationUrl !== "" ? data.locationUrl : undefined,
        fileInfo: initial?.location?.fileInfo, // keep initial location fileInfo, will be updated in later request
      },
      cover: initial?.cover, // keep initial cover, will be updated in later request
      categories: data.categories?.map((x): CategoryDTO => ({ name: x })),
      tags: data.tags?.map((x): TagDTO => ({ name: x })),
    };

    action.onClick(book, coverFile, bookFile);
  };

  const { mutateAsync: getProxyCover } = useProxyCover();

  // import metadata
  const [importMetadataDialogOpen, setImportMetadataDialogOpen] =
    useState(false);
  const handleImportMetadata = (metadata: MetadataDTO): void => {
    setImportMetadataDialogOpen(false);

    let author = methods.getValues("author");
    if (metadata.authors && metadata.authors.length > 0) {
      author = metadata.authors[0];
    }

    let categories = methods.getValues("categories");
    const { categories: importedCategories } = metadata;
    if (importedCategories && importedCategories.length > 0) {
      categories = importedCategories;
    }

    let tags = methods.getValues("tags");
    const { tags: importedTags } = metadata;
    if (importedTags && importedTags.length > 0) {
      tags = importedTags;
    }

    // update the form values with the imported metadata
    methods.reset({
      title: metadata.title ?? methods.getValues("title"),
      description: metadata.description ?? methods.getValues("description"),
      author,
      releaseDate: metadata.releaseDate
        ? dayjs(metadata.releaseDate)
        : methods.getValues("releaseDate"),
      pageNumber: metadata.pages ?? methods.getValues("pageNumber"),
      categories,
      tags,
      series: metadata.series ?? methods.getValues("series"),
      seriesNumber: metadata.volume ?? methods.getValues("seriesNumber"),
      locationType: methods.getValues("locationType"),
      locationUrl: methods.getValues("locationUrl"),
    });

    // cover
    if (metadata.coverUrl) {
      // use proxy-cover endpoint to prevent CORS issues from google
      getProxyCover(metadata.coverUrl).then((response) => {
        updateCoverFromFile(response.data);
      });
    } else {
      // no cover is set, leave current cover
    }
  };

  return (
    <Box margin="15px">
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Grid container spacing={{ xs: 1, md: 4 }} columns={11}>
            <Grid size={{ xs: 0, md: 1 }}></Grid>

            <Grid size={{ xs: 11, md: 2.5 }}>
              <Stack spacing={1} alignItems="center">
                <img
                  src={coverImage}
                  onError={onLoadingError}
                  style={{
                    width: "100%",
                    borderRadius: 2,
                    boxShadow: "3",
                    marginLeft: isMobile ? "auto" : 0,
                    marginRight: isMobile ? "auto" : 0,
                  }}
                />

                <FileUploadButton
                  onFileChange={updateCoverFromFile}
                  accept={ImageTypes}
                >
                  Upload Cover
                </FileUploadButton>
              </Stack>
            </Grid>

            <Grid size={{ xs: 11, md: 6.5 }} mt="20px">
              <Stack spacing={2} width={isMobile ? "100%" : "60%"}>
                {/* Title */}
                <Controller
                  name="title"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Title"
                      variant="filled"
                      error={Boolean(errors.title)}
                      helperText={errors.title?.message}
                    />
                  )}
                />

                {/* Description */}
                <Controller
                  name="description"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Description"
                      variant="filled"
                      multiline
                      minRows={4}
                    />
                  )}
                />

                <Stack
                  direction="row"
                  spacing={2}
                  justifyContent="space-between"
                >
                  {/* Page Number */}
                  <Controller
                    name="pageNumber"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        label="Page Number"
                        variant="filled"
                        type="number"
                        error={Boolean(errors.pageNumber)}
                        helperText={errors.pageNumber?.message}
                      />
                    )}
                  />

                  {/* Release Date */}
                  <Controller
                    name="releaseDate"
                    control={control}
                    render={({ field }) => (
                      <DateField
                        {...field}
                        label="Release Date"
                        variant="filled"
                        error={Boolean(errors.releaseDate)}
                      />
                    )}
                  />
                </Stack>

                {/* Author */}
                <Controller
                  name="author"
                  control={control}
                  render={({ field }) => (
                    <AutoComplete
                      {...field}
                      label="Author"
                      variant="filled"
                      fetchSuggestions={async (value) => {
                        const { data } =
                          await clients.authors.authorsAutocompleteGet(value);
                        return data;
                      }}
                    />
                  )}
                />

                <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
                  {/* Series */}
                  <Controller
                    name="series"
                    control={control}
                    render={({ field }) => (
                      <AutoComplete
                        {...field}
                        label="Series"
                        variant="filled"
                        fullWidth
                        error={Boolean(errors.series)}
                        helperText={errors.series?.message}
                        fetchSuggestions={async (value) => {
                          const { data } =
                            await clients.series.seriesAutocompleteGet(value);
                          return data;
                        }}
                      />
                    )}
                  />

                  {/* Volume */}
                  <Controller
                    name="seriesNumber"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        label="Volume"
                        variant="filled"
                        type="number"
                        error={Boolean(errors.seriesNumber)}
                        helperText={errors.seriesNumber?.message}
                      />
                    )}
                  />
                </Stack>

                {/* Location */}
                <EditableLocationDetails
                  initial={initial}
                  control={control}
                  onFileChange={setBookFile}
                />

                {/* Categories */}
                <Stack direction="row" spacing={1} alignItems="start">
                  <CategoryIcon sx={{ mr: "5px !important" }} />
                  <Controller
                    name="categories"
                    control={control}
                    render={({ field }) => (
                      <ItemList
                        itemName="Category"
                        items={field.value?.map((x) => x ?? "")}
                        onChange={field.onChange}
                        useAutocomplete
                        autocompleteFetchSuggestions={async (x) => {
                          const { data } =
                            await clients.books.booksAutocompleteCategoryGet(x);
                          return data;
                        }}
                      />
                    )}
                  />
                </Stack>

                {/* Tags */}
                <Stack direction="row" spacing={1} alignItems="start">
                  <LocalOfferIcon sx={{ mr: "5px !important" }} />
                  <Controller
                    name="tags"
                    control={control}
                    render={({ field }) => (
                      <ItemList
                        itemName="Tag"
                        items={field.value?.map((x) => x ?? "")}
                        onChange={field.onChange}
                        variant="outlined"
                        useAutocomplete
                        autocompleteFetchSuggestions={async (x) => {
                          const { data } =
                            await clients.books.booksAutocompleteTagGet(x);
                          return data;
                        }}
                      />
                    )}
                  />
                </Stack>

                <Divider />

                <Stack
                  direction={{ xs: "column", md: "row" }}
                  spacing={2}
                  justifyContent="space-between"
                  alignItems="end"
                  mt="15px"
                >
                  <Button
                    variant="outlined"
                    startIcon={<ImportContactsIcon />}
                    sx={{
                      alignItems: "start",
                      width: "fit-content",
                      height: "fit-content",
                      whiteSpace: "nowrap",
                    }}
                    onClick={() => setImportMetadataDialogOpen(true)}
                  >
                    Import Metadata
                  </Button>
                  {action && (
                    <Button
                      variant="contained"
                      startIcon={action.icon}
                      type="submit"
                      disabled={!isValid}
                      sx={{
                        alignItems: "start",
                        width: "fit-content",
                        height: "fit-content",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {action.name}
                    </Button>
                  )}
                </Stack>
              </Stack>
            </Grid>
          </Grid>

          {/* Metadata Import Dialog */}
          <Controller
            name="title"
            control={control}
            render={({ field }) => (
              <MetadataDialog
                title={field.value}
                open={importMetadataDialogOpen}
                onCancel={() => setImportMetadataDialogOpen(false)}
                onConfirm={handleImportMetadata}
              />
            )}
          />
        </form>
      </FormProvider>
    </Box>
  );
};

export default EditableBookDetails;
