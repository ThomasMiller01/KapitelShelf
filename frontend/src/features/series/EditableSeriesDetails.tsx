import { yupResolver } from "@hookform/resolvers/yup";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
import {
  Box,
  Button,
  Divider,
  Grid,
  Rating,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useEffect } from "react";
import { Controller, FormProvider, useForm } from "react-hook-form";

import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { useMobile } from "../../hooks/useMobile";
import { type SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";
import type { SeriesFormValues } from "../../lib/schemas/SeriesSchema";
import { SeriesSchema } from "../../lib/schemas/SeriesSchema";

interface ActionProps {
  name: string;
  onClick: (series: SeriesDTO) => void;
  icon?: ReactNode;
}

interface EditableSeriesDetailsProps {
  initial?: SeriesDTO;
  action?: ActionProps;
}

const EditableSeriesDetails = ({
  initial,
  action,
}: EditableSeriesDetailsProps): ReactElement => {
  const { isMobile } = useMobile();

  const methods = useForm({
    resolver: yupResolver(SeriesSchema),
    mode: "onBlur",
    defaultValues: {
      name: initial?.name ?? "",
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
  }, [triggerValidation]);

  const onSubmit = (data: SeriesFormValues): void => {
    if (action === undefined) {
      return;
    }

    const series: SeriesDTO = {
      name: data.name,
    };

    action.onClick(series);
  };

  return (
    <Box m="15px">
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Grid container spacing={{ xs: 1, md: 4 }} columns={11}>
            <Grid size={{ xs: 0, md: 3 }}></Grid>

            <Grid size={{ xs: 11, md: 8 }} mt="20px">
              <Stack spacing={2} width={isMobile ? "100%" : "60%"}>
                {/* Name */}
                <Controller
                  name="name"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Series Name"
                      variant="filled"
                      error={Boolean(errors.name)}
                      helperText={errors.name?.message}
                    />
                  )}
                />

                <Stack
                  direction="row"
                  spacing={2}
                  justifyContent="space-between"
                  alignItems="center"
                >
                  {/* User Rating */}
                  <Stack>
                    <Typography gutterBottom>Custom Rating (WiP)</Typography>
                    <Rating
                      defaultValue={0}
                      max={5}
                      precision={0.5}
                      size="large"
                    />
                  </Stack>

                  {/* Calculated Rating */}
                  <Stack alignItems="end">
                    <Stack direction="row" alignItems="center" spacing={0.5}>
                      <Typography color="textDisabled" gutterBottom>
                        Calculated
                      </Typography>
                      <IconButtonWithTooltip
                        tooltip="Calculated based on the book ratings from this series"
                        color="primary"
                        size="small"
                        disabled
                      >
                        <HelpOutlineIcon fontSize="small" />
                      </IconButtonWithTooltip>
                    </Stack>
                    <Rating
                      value={(initial?.rating ?? 0) / 2}
                      max={5}
                      precision={0.5}
                      readOnly
                      disabled
                    />
                  </Stack>
                </Stack>

                <Divider />

                <Stack
                  direction={{ xs: "column", md: "row" }}
                  spacing={2}
                  justifyContent="space-between"
                  alignItems="end"
                  mt="15px"
                >
                  <Box />
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
        </form>
      </FormProvider>
    </Box>
  );
};

export default EditableSeriesDetails;
