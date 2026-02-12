import * as yup from "yup";

export const SeriesSchema = yup.object({
  name: yup.string().required("Name is required"),
  rating: yup.number().nullable(),
});

export type SeriesFormValues = yup.InferType<typeof SeriesSchema>;
