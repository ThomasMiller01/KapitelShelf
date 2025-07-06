import { yupResolver } from "@hookform/resolvers/yup";
import EditIcon from "@mui/icons-material/Edit";
import {
  Avatar,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useEffect, useState } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { ProfileImage } from "../../components/ProfileImage";
import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import type { UserFormValues } from "../../lib/schemas/UserSchema";
import { UserSchema } from "../../lib/schemas/UserSchema";
import { GetUserColor } from "../../utils/UserProfile";
import { ProfileImageList } from "./ProfileImageList";

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

  const [selectImageOpen, setSelectImageOpen] = useState(false);

  return (
    <Box>
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Controller
            name="color"
            control={control}
            render={({ field: fcolor }) => (
              <>
                <Controller
                  name="image"
                  control={control}
                  render={({ field: fimage }) => (
                    <Stack
                      direction={{ xs: "column-reverse", sm: "row" }}
                      spacing={{ xs: 2, sm: 4 }}
                      alignItems="center"
                      mb="15px"
                    >
                      <Box position="relative">
                        {/* Image */}
                        <ProfileImage
                          profileImageType={
                            fimage.value ?? ProfileImageTypeDTO.NUMBER_0
                          }
                          profileColor={fcolor.value ?? ""}
                        />
                        <IconButtonWithTooltip
                          tooltip="Change Image"
                          onClick={() => setSelectImageOpen(true)}
                          sx={{
                            position: "absolute",
                            bottom: 6,
                            right: 6,
                          }}
                        >
                          <EditIcon />
                        </IconButtonWithTooltip>
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
                        {/* Color */}
                        <IconButtonWithTooltip
                          tooltip="Change Color"
                          size="small"
                          onClick={() => alert("TODO")}
                        >
                          <Avatar
                            variant="rounded"
                            sx={{ bgcolor: fcolor.value }}
                          >
                            {" "}
                          </Avatar>
                        </IconButtonWithTooltip>
                      </Stack>
                      <ListSelectionDialog
                        name="profile image"
                        open={selectImageOpen}
                        onClose={() => setSelectImageOpen(false)}
                      >
                        <ProfileImageList
                          onClick={(profileImageType: ProfileImageTypeDTO) => {
                            setSelectImageOpen(false);
                            fimage.onChange(profileImageType);
                          }}
                          profileColor={fcolor.value ?? ""}
                        />
                      </ListSelectionDialog>
                    </Stack>
                  )}
                />
              </>
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

interface ListSelectionDialogProps {
  name: string;
  open: boolean;
  onClose: () => void;
  children: ReactElement;
}

const ListSelectionDialog: React.FC<ListSelectionDialogProps> = ({
  name,
  open,
  onClose,
  children,
}) => (
  <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
    <DialogTitle>Select a {name} from the list below</DialogTitle>
    <DialogContent sx={{ pt: "10px !important", pb: "0" }}>
      {children}
    </DialogContent>
    <DialogActions>
      <Button onClick={onClose}>Cancel</Button>
    </DialogActions>
  </Dialog>
);

export default EditableProfileDetails;
