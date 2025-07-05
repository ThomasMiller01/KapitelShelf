import { yupResolver } from "@hookform/resolvers/yup";
import { Avatar, Box, Button, Stack, TextField, Tooltip } from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useEffect } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import type { UserFormValues } from "../../lib/schemas/UserSchema";
import { UserSchema } from "../../lib/schemas/UserSchema";
import {
  GetUserColor,
  ProfileImageTypeToName,
  ProfileImageTypeToSrc,
} from "../../utils/UserProfile";

interface ActionProps {
  name: string;
  onClick: (user: UserDTO | undefined) => void;
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
      image: initial?.image,
      color: initial?.color ?? GetUserColor(initial?.username),
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

  const onSubmit = (data: UserFormValues): void => {
    if (confirmAction === undefined) {
      return;
    }

    const user: UserDTO = {
      username: data.username,
      image: data.image,
      color: data.color,
    };

    confirmAction.onClick(user);
  };

  return (
    <Box>
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          {/* Color */}
          <Controller
            name="color"
            control={control}
            render={({ field: fcolor }) => (
              <Stack
                direction={{ xs: "column-reverse", sm: "row" }}
                spacing={{ xs: 2, sm: 4 }}
                alignItems="center"
                mb="15px"
              >
                <Box
                  sx={{
                    bgcolor: fcolor.value,
                    pb: "10px",
                    borderRadius: "32px",
                  }}
                >
                  {/* Image */}
                  <Controller
                    name="image"
                    control={control}
                    render={({ field: fimage }) => (
                      <Tooltip
                        title={
                          ProfileImageTypeToName[
                            fimage.value ?? ProfileImageTypeDTO.NUMBER_0
                          ]
                        }
                      >
                        <img
                          style={{
                            minHeight: "170px",
                            maxHeight: "200px",
                          }}
                          src={
                            ProfileImageTypeToSrc[
                              fimage.value ?? ProfileImageTypeDTO.NUMBER_0
                            ]
                          }
                          alt={
                            ProfileImageTypeToName[
                              fimage.value ?? ProfileImageTypeDTO.NUMBER_0
                            ]
                          }
                        />
                      </Tooltip>
                    )}
                  />
                </Box>
                <Stack spacing={2} alignItems="start">
                  {/* Username */}
                  <Controller
                    name="username"
                    control={control}
                    render={({ field: fusername }) => (
                      <TextField
                        {...fusername}
                        label="Username"
                        variant="filled"
                        fullWidth
                        error={Boolean(errors.username)}
                        helperText={errors.username?.message}
                      />
                    )}
                  />
                  <IconButtonWithTooltip tooltip="Change Color" size="small">
                    <Avatar variant="rounded" sx={{ bgcolor: fcolor.value }}>
                      {" "}
                    </Avatar>
                  </IconButtonWithTooltip>
                </Stack>
              </Stack>
            )}
          />

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
                isSubmit
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
  isSubmit = false,
}: {
  action: ActionProps;
  disabled?: boolean;
  variant?: "contained" | "outlined";
  isSubmit?: boolean;
}): ReactElement => (
  <Button
    variant={variant}
    startIcon={action.icon}
    type={isSubmit ? "submit" : "button"}
    onClick={
      !isSubmit && action.onClick !== undefined
        ? (): void => action.onClick(undefined)
        : undefined
    }
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
