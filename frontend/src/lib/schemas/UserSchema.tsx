import * as yup from "yup";

export const UserSchema = yup.object({
  username: yup.string().required("Username is required"),
});

export type UserFormValues = yup.InferType<typeof UserSchema>;
