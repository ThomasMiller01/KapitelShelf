import type { Dayjs } from "dayjs";
import * as yup from "yup";

export const BookSchema = yup.object({
  title: yup.string().required("Title is required"),
  description: yup.string(),
  releaseDate: yup.mixed<Dayjs>().nullable(),
  pageNumber: yup
    .number()
    .transform((value, original) => (original === "" ? null : value))
    .typeError("Page number must be a number")
    .positive("Page number must be positive")
    .integer("Page number must be an integer")
    .nullable(),
  series: yup.string().nullable(),
  seriesNumber: yup
    .number()
    .transform((value, original) => (original === "" ? null : value))
    .typeError("Volume must be a number")
    .min(0, "Volume must be zero or positive")
    .integer("Volume must be an integer")
    .nullable(),
  author: yup.string().nullable(),
  locationType: yup.number(),
  locationUrl: yup
    .string()
    .transform((value, original) => (original === "" ? null : value))
    .nullable()
    .url("Link must be a valid URL"),
  categories: yup.array().of(yup.string()),
  tags: yup.array().of(yup.string()),
});

export type BookFormValues = yup.InferType<typeof BookSchema>;
