import { Box } from "@mui/material";

import { SettingItem } from "../../../components/settings/SettingItem";
import type { ObjectSettingsDTO } from "../../../lib/api/KapitelShelf.Api";

interface AiFeaturesSelectionProps {
  settings: ObjectSettingsDTO[];
}

export const AiFeaturesSelection: React.FC<AiFeaturesSelectionProps> = ({
  settings,
}) => {
  const features = settings.find((x) => x.key === "ai.enabled.features");

  console.log(features);

  return (
    <Box sx={{ mt: 3 }}>
      <SettingItem
        setting={features}
        type="list{string}-as-boolean"
        label="AI Features"
        description="Select which AI-powered features are enabled in KapitelShelf."
        options={[]}
      />
    </Box>
  );
};
