import * as yup from "yup";

import type { ProfileImageTypeDTO } from "../api/KapitelShelf.Api/api";

export const UserSchema = yup.object({
  username: yup.string().required("Username is required"),
  image: yup.mixed<ProfileImageTypeDTO>(),
  color: yup.string(),
});

export type UserFormValues = yup.InferType<typeof UserSchema>;
