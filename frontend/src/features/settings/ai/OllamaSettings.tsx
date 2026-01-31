import { Alert, Box, Stack, Typography } from "@mui/material";

import { SettingItem } from "../../../components/settings/SettingItem";
import type { ObjectSettingsDTO } from "../../../lib/api/KapitelShelf.Api";

interface OllamaSettingProps {
  settings: ObjectSettingsDTO[];
}

export const OllamaSetting: React.FC<OllamaSettingProps> = ({ settings }) => {
  const model = settings.find((x) => x.key === "ai.ollama.model");
  const url = settings.find((x) => x.key === "ai.ollama.url");

  return (
    <Box>
      <SettingItem
        setting={url}
        type="string"
        label="Ollama Server URL"
        placeholder="http://host.docker.internal:11434"
        description="Specify the URL of the Ollama server."
        details={
          <Alert
            severity="info"
            sx={{
              padding: "0px 14px",
              height: "fit-content",
              "& .MuiAlert-message": {
                height: "fit-content",
              },
              "& .MuiAlert-icon": {
                height: "fit-content",
                svg: { width: "0.9em", height: "0.9em" },
              },
            }}
          >
            The server must be reachable from the KapitelShelf backend.
          </Alert>
        }
      />
      <SettingItem
        setting={model}
        type="enum"
        label="Model"
        options={[
          {
            label: (
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography fontWeight="bold">gemma2:2b</Typography>
                <Typography>(low resources)</Typography>
              </Stack>
            ),
            value: "gemma2:2b",
          },
          {
            label: (
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography fontWeight="bold">gemma2:9b</Typography>
                <Typography>(recommended)</Typography>
              </Stack>
            ),
            value: "gemma2:9b",
          },
          {
            label: (
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography fontWeight="bold">gemma2:27b</Typography>
                <Typography>(higher quality)</Typography>
              </Stack>
            ),
            value: "gemma2:27b",
          },
        ]}
        description="Select the AI model used for Ollama."
      />
    </Box>
  );
};
