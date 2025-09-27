import * as yup from "yup";

export const ASINSchema = yup.object({
  asin: yup
    .string()
    .required("ASIN is required")
    .length(10, "ASIN must be exactly 10 characters")
    .transform((val) => (val ? val.toUpperCase() : val)) // auto-uppercase
    .matches(/^[A-Z0-9]+$/, "ASIN must contain only letters and digits"),
});

export type ASINFormValues = yup.InferType<typeof ASINSchema>;
