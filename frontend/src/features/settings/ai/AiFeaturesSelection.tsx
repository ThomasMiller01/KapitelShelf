import { Box } from "@mui/material";

import { SettingItem } from "../../../components/settings/SettingItem";
import type { ObjectSettingsDTO } from "../../../lib/api/KapitelShelf.Api";

interface AiFeaturesSelectionProps {
  settings: ObjectSettingsDTO[];
  aiConfigured: boolean;
}

export const AiFeaturesSelection: React.FC<AiFeaturesSelectionProps> = ({
  settings,
  aiConfigured,
}) => {
  const features = settings.find((x) => x.key === "ai.enabled.features");

  return (
    <Box sx={{ mt: 3 }}>
      <SettingItem
        setting={features}
        enabled={aiConfigured}
        type="list{string}-as-boolean"
        label="AI Features"
        description="Select which AI-powered features are enabled in KapitelShelf."
        options={[
          {
            value: "BookImportMetadataGeneration",
            label: "Import Metadata Generation",
            description:
              "Automatically generate metadata for imported books using AI.",
          },
        ]}
      />
    </Box>
  );
};
