import AddCircleIcon from "@mui/icons-material/AddCircle";
import EditSquareIcon from "@mui/icons-material/EditSquare";
import { Box, Grid } from "@mui/material";
import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import UserProfileCard from "../../components/UserProfileCard";
import { useUserProfile } from "../../hooks/useUserProfile";
import { useUsersList } from "../../lib/requests/users/useUsersList";
import { ClearMobileApiBaseUrl, IsMobileApp } from "../../utils/MobileUtils";

export const ProfileList = (): ReactElement => {
  const { setProfile } = useUserProfile();
  const navigate = useNavigate();

  const { data: userProfiles, isLoading, isError, refetch } = useUsersList();

  if (isLoading) {
    return (
      <Box padding="20px">
        <LoadingCard useLogo delayed itemName="User Profiles" showRandomFacts />
      </Box>
    );
  }

  const changeMobileApiUrl = (): void => {
    ClearMobileApiBaseUrl();
    window.location.reload();
  };

  if (isError || userProfiles === undefined || userProfiles === null) {
    return (
      <Box padding="20px">
        <RequestErrorCard
          itemName="User Profiles"
          onRetry={refetch}
          secondAction={IsMobileApp() ? changeMobileApiUrl : null}
          secondActionText={IsMobileApp() ? "Change API URL" : null}
          secondActionIcon={IsMobileApp() ? <EditSquareIcon /> : null}
        />
      </Box>
    );
  }

  return (
    <Grid
      spacing={{ xs: 2, md: 4 }}
      justifyContent="center"
      width="100%"
      container
    >
      {userProfiles.map((userProfile) => (
        <Grid
          size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 1.5 }}
          key={userProfile.id}
        >
          <UserProfileCard userProfile={userProfile} onClick={setProfile} />
        </Grid>
      ))}
      <Grid
        size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 1.5 }}
        alignContent="center"
      >
        <IconButtonWithTooltip
          tooltip="Create new user profile"
          sx={{ mx: "2rem", my: "1rem" }}
          onClick={() => navigate("/create-user-profile")}
        >
          <AddCircleIcon sx={{ fontSize: "5rem", color: "text.primary" }} />
        </IconButtonWithTooltip>
      </Grid>
    </Grid>
  );
};
