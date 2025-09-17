import {
  Alert,
  Box,
  Button,
  Container,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type ReactElement, useEffect, useMemo, useState } from "react";

import { useApi } from "../contexts/ApiProvider";
import {
  GetMobileApiBaseUrl,
  IsValidUrl,
  SetMobileApiBaseUrl,
} from "../utils/MobileUtils";

export const ConfigureBackendUrlPage = (): ReactElement => {
  const { setBasePath } = useApi();

  const initial = useMemo<string | null>(() => GetMobileApiBaseUrl(), []);
  const [value, setValue] = useState<string | null>(initial);
  const [touched, setTouched] = useState<boolean>(false);
  const [error, setError] = useState<string>("");

  useEffect(() => {
    if (!touched) {
      setError("");
      return;
    }

    if (!value || !IsValidUrl(value)) {
      setError("Please enter a valid URL including protocol (https://…).");
    } else {
      setError("");
    }
  }, [value, touched]);

  const handleContinue = (): void => {
    if (!value || !IsValidUrl(value)) {
      return;
    }

    const basePath = value.trim();

    setBasePath(basePath);
    SetMobileApiBaseUrl(basePath);
    window.location.reload();
  };

  const isValid = (value?.length ?? 0) > 0 && IsValidUrl(value);

  return (
    <Container
      maxWidth="sm"
      sx={{
        height: "90vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      <Paper elevation={3} sx={{ p: 3 }}>
        <Stack spacing={3}>
          <Box>
            <Typography variant="h4">Configure Backend</Typography>
            <Typography
              variant="body2"
              sx={{ mt: 1, opacity: 0.8, fontSize: "0.8rem" }}
            >
              Enter the base URL of your KapitelShelf backend (e.g.{" "}
              <code>https://kapitelshelf.com/api</code>).
            </Typography>
          </Box>

          <TextField
            label="KapitelShelf API URL"
            placeholder="https://your-backend.example.com/api"
            value={value}
            variant="filled"
            fullWidth
            onChange={(e) => setValue(e.target.value)}
            onBlur={() => setTouched(true)}
            error={touched && !isValid}
            helperText={
              touched && !isValid
                ? "Enter a full URL including http(s)://"
                : " "
            }
          />

          {touched && error && (
            <Alert severity="warning" role="alert">
              {error}
            </Alert>
          )}

          <Button
            variant="contained"
            size="large"
            onClick={handleContinue}
            disabled={!isValid}
            sx={{ borderRadius: 3 }}
          >
            Continue
          </Button>
        </Stack>
      </Paper>
    </Container>
  );
};
