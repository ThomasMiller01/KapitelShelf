import EditSquareIcon from "@mui/icons-material/EditSquare";
import { Button, Divider, Paper, Typography } from "@mui/material";
import { useState } from "react";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import { useUserProfile } from "../../hooks/useUserProfile";
import { ClearMobileApiBaseUrl } from "../../utils/MobileUtils";

export const MobileSettings: React.FC = () => {
  const { clearProfile } = useUserProfile();
  const [changeApiBaseUrlOpen, setChangeApiBaseUrlOpen] = useState(false);
  const onApiBaseUrlChange = (): void => {
    setChangeApiBaseUrlOpen(false);
    ClearMobileApiBaseUrl();
    clearProfile();
    window.location.reload();
  };

  return (
    <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
      <Typography variant="h6">Mobile</Typography>
      <Divider sx={{ mb: 2 }} />
      <Button
        variant="contained"
        startIcon={<EditSquareIcon />}
        onClick={() => setChangeApiBaseUrlOpen(true)}
      >
        Change API URL
      </Button>
      <DeleteDialog
        open={changeApiBaseUrlOpen}
        onCancel={() => setChangeApiBaseUrlOpen(false)}
        onConfirm={onApiBaseUrlChange}
        title="Confirm to change the API URL"
        description="Are you sure you want to change the API URL? This will log you out of the app."
        confirmText="Change URL"
      />
    </Paper>
  );
};
