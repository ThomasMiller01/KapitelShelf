import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Chip,
  Divider,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { SiOllama } from "react-icons/si";

import { SettingItem } from "../../../components/settings/SettingItem";
import type { ObjectSettingsDTO } from "../../../lib/api/KapitelShelf.Api";
import { AiFeaturesSelection } from "./AiFeaturesSelection";
import { OllamaSetting } from "./OllamaSettings";

interface AiSettingsProps {
  settings: ObjectSettingsDTO[];
}

export const AiSettings: React.FC<AiSettingsProps> = ({ settings }) => {
  const provider = settings.find((x) => x.key === "ai.provider");
  const providerConfigured = settings.find(
    (x) => x.key === "ai.provider.configured",
  );

  return (
    <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
      <Stack direction="row" spacing={2} alignItems="center">
        <Typography variant="h6">AI</Typography>
        <Chip
          label={providerConfigured?.value ? "Configured" : "Not Configured"}
          color={providerConfigured?.value ? "success" : "error"}
          size="small"
        />
      </Stack>
      <Divider sx={{ mb: 2 }} />
      <SettingItem
        setting={provider}
        type="enum"
        label="AI Provider"
        options={[
          {
            label: "Disable AI features",
            value: "None",
          },
          {
            label: (
              <Stack direction="row" spacing={1} alignItems="center">
                <SiOllama fontSize="1.2rem" />
                <Typography>Ollama</Typography>
              </Stack>
            ),
            value: "Ollama",
          },
        ]}
        description="Select which AI provider should be used for AI-powered features."
      />
      {provider?.value !== "None" && (
        <ConfigureProvider
          settings={settings}
          provider={provider}
          providerConfigured={providerConfigured}
        />
      )}
      <AiFeaturesSelection
        settings={settings}
        aiConfigured={providerConfigured?.value}
      />
    </Paper>
  );
};

interface ConfigureProviderProps {
  settings: ObjectSettingsDTO[];
  provider: ObjectSettingsDTO | undefined;
  providerConfigured: ObjectSettingsDTO | undefined;
}

const ConfigureProvider: React.FC<ConfigureProviderProps> = ({
  settings,
  provider,
  providerConfigured,
}) => {
  if (provider === undefined) {
    return <></>;
  }

  return (
    <Box sx={{ mt: 3 }}>
      <Accordion
        defaultExpanded={!providerConfigured?.value}
        disableGutters
        sx={{ boxShadow: "none", border: "1px solid", borderColor: "divider" }}
      >
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          sx={{
            flexDirection: "row-reverse",
            "& .MuiAccordionSummary-content": {
              marginLeft: 1,
            },
          }}
        >
          <Typography variant="h6">Configure {provider?.value}</Typography>
        </AccordionSummary>
        <AccordionDetails sx={{ pt: 0 }}>
          {provider?.value === "Ollama" && (
            <OllamaSetting settings={settings} />
          )}
        </AccordionDetails>
      </Accordion>
    </Box>
  );
};
