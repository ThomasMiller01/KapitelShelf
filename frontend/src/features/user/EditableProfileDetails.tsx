import { yupResolver } from "@hookform/resolvers/yup";
import { Box, Button, Stack, TextField } from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useEffect, useState } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import WizardProfile from "../../assets/Wizard.png";
import type { UserDTO } from "../../lib/api/KapitelShelf.Api/api";
import type { UserFormValues } from "../../lib/schemas/UserSchema";
import { UserSchema } from "../../lib/schemas/UserSchema";
import { GetUserColor } from "../../utils/UserProfile";

// 600ms after user stops typing
const USERNAME_REST_MS = 600;

interface ActionProps {
  name: string;
  onClick: (user: UserDTO) => void;
  icon?: ReactNode;
}

interface EditableProfileDetailsProps {
  initial?: UserDTO | null;
  confirmAction?: ActionProps;
  cancelAction?: ActionProps;
}

const EditableProfileDetails = ({
  initial,
  confirmAction,
  cancelAction,
}: EditableProfileDetailsProps): ReactElement => {
  const methods = useForm({
    resolver: yupResolver(UserSchema),
    mode: "onBlur",
    defaultValues: {
      username: initial?.username ?? "",
    },
  });

  const {
    control,
    handleSubmit,
    trigger: triggerValidation,
    formState: { errors, isValid },
  } = methods;

  useEffect(() => {
    // run validation on mount
    triggerValidation();
  }, [triggerValidation, initial]);

  // update profile image color with delay
  const [profileImageColor, setProfileImageColor] = useState(
    GetUserColor(initial?.username)
  );
  useEffect(() => {
    const handle = setTimeout(
      () => setProfileImageColor(GetUserColor(control._formValues.username)),
      USERNAME_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [control._formValues.username]);

  const onSubmit = (data: UserFormValues): void => {
    if (confirmAction === undefined) {
      return;
    }

    const user: UserDTO = {
      username: data.username,
    };

    confirmAction.onClick(user);
  };

  return (
    <Box>
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack
            direction={{ xs: "column-reverse", sm: "row" }}
            spacing={{ xs: 2, sm: 4 }}
            alignItems="center"
            mb="15px"
          >
            <Box
              sx={{
                bgcolor: profileImageColor,
                pb: "10px",
                borderRadius: "32px",
              }}
            >
              <img
                style={{
                  minHeight: "170px",
                  maxHeight: "200px",
                }}
                src={WizardProfile}
                alt={"User Avatar"}
              />
            </Box>
            {/* Title */}
            <Controller
              name="username"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Username"
                  variant="filled"
                  fullWidth
                  error={Boolean(errors.username)}
                  helperText={errors.username?.message}
                />
              )}
            />
          </Stack>

          <Stack
            direction={{ xs: "column", md: "row" }}
            spacing={2}
            justifyContent="end"
            alignItems="end"
            mt="15px"
          >
            {confirmAction && (
              <ActionButton
                action={confirmAction}
                disabled={!isValid}
                variant="contained"
              />
            )}
            {cancelAction && (
              <ActionButton action={cancelAction} variant="outlined" />
            )}
          </Stack>
        </form>
      </FormProvider>
    </Box>
  );
};

const ActionButton = ({
  action,
  disabled = false,
  variant = "contained",
}: {
  action: ActionProps;
  disabled?: boolean;
  variant?: "contained" | "outlined";
}): ReactElement => (
  <Button
    variant={variant}
    startIcon={action.icon}
    type="submit"
    disabled={disabled}
    sx={{
      alignItems: "start",
      width: "fit-content",
      height: "fit-content",
      whiteSpace: "nowrap",
    }}
  >
    {action.name}
  </Button>
);

export default EditableProfileDetails;
