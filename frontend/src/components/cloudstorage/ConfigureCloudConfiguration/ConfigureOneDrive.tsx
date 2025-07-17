import {
  Alert,
  Box,
  Grid,
  Link,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";
import React, { useEffect, useState } from "react";

import type { ConfigureCloudDTO } from "../../../lib/api/KapitelShelf.Api/api";

interface ConfigureOneDriveProps {
  onConfigurationChange: (configuration: ConfigureCloudDTO) => void;
}

export const ConfigureOneDrive: React.FC<ConfigureOneDriveProps> = ({
  onConfigurationChange,
}) => {
  const [oauthClientId, setOauthClientId] = useState("");

  useEffect(() => {
    onConfigurationChange({
      oAuthClientId: oauthClientId,
    });
  }, [onConfigurationChange, oauthClientId]);

  return (
    <Grid container spacing={4}>
      {/* Configure OneDrive Cloud */}
      <Grid size={{ xs: 12, md: 6 }}>
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Typography variant="h6">OAuth Client ID</Typography>
            <TextField
              label="Microsoft OAuth Client ID"
              variant="filled"
              value={oauthClientId}
              fullWidth
              onChange={(e) => setOauthClientId(e.target.value)}
              placeholder="Enter your Azure app client ID"
            />
          </Stack>
        </Paper>
      </Grid>

      {/* Get OAuth ClientId Guide */}
      <Grid size={{ xs: 12, md: 6 }}>
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            How to get your Client ID for OneDrive
          </Typography>
          <Stack spacing={2} mb={3}>
            <Step number={1}>
              <Typography variant="body2" color="text.secondary">
                Go to{" "}
                <Link
                  href="https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade"
                  target="_blank"
                  rel="noreferrer"
                >
                  https://portal.azure.com
                </Link>{" "}
                and sign in with your Microsoft account.
              </Typography>
            </Step>
            <Step number={2}>
              <Typography variant="body2" color="text.secondary">
                Click <b>New registration</b>
              </Typography>
            </Step>
            <Step number={3}>
              <Typography variant="body2" color="text.secondary">
                Enter an app name like <b>KapitelShelf</b> and choose{" "}
                <b>Accounts in any organization or personal accounts</b>.
              </Typography>
            </Step>
            <Step number={4.1}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Set the redirect platform to{" "}
                <b>Public client/native (mobile & desktop)</b>.
              </Typography>
            </Step>
            <Step number={4.2}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Set the redirect URI to the domain of your KapitelShelf API,
                followed by <b>/cloudstorage/onedrive/oauth/callback</b>.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                E.g.{" "}
                <b>
                  http://localhost:53682/cloudstorage/onedrive/oauth/callback
                </b>
              </Typography>
            </Step>
            <Step number={5}>
              <Typography variant="body2" color="text.secondary">
                Click <b>Register</b> to create the app.
              </Typography>
            </Step>
            <Step number={6}>
              <Typography variant="body2" color="text.secondary">
                Copy the <b>Application (client) ID</b> - that is your OAuth
                Client ID.
              </Typography>
            </Step>
            <Step number={7}>
              <Typography variant="body2" color="text.secondary">
                Paste it into the field on the left to configure OneDrive cloud
                access.
              </Typography>
            </Step>
          </Stack>
          <Alert severity="info">
            Why do I need to create my own Microsoft Azure Application?
            <br />
            <Link
              href="https://github.com/ThomasMiller01/KapitelShelf/blob/main/docs/faq.md#why-do-i-need-to-create-my-own-microsoft-azure-application-when-using-onedrive"
              target="_blank"
              rel="noreferrer"
              sx={{ cursor: "pointer" }}
            >
              See FAQ
            </Link>
          </Alert>
        </Paper>
      </Grid>
    </Grid>
  );
};

type StepProps = {
  number: number;
  children: ReactElement | ReactElement[];
};

const Step: React.FC<StepProps> = ({ number, children }) => (
  <Box sx={{ wordBreak: "break-word" }}>
    <Typography variant="subtitle1" fontWeight={600}>
      Step {number}
    </Typography>
    {children}
  </Box>
);
