import { Box, Grid } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import FancyText from "../components/FancyText";
import UserProfileCard from "../components/UserProfileCard";
import { useUserProfile } from "../contexts/UserProfileContext";
import { usersApi } from "../lib/api/KapitelShelf.Api";

export const UserProfileSelectionPage = (): ReactElement => {
  const { setProfile } = useUserProfile();

  const {
    data: userProfiles,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["user-profile-list"],
    queryFn: async () => {
      const { data } = await usersApi.usersGet();
      return data;
    },
  });

  if (isLoading) {
    return (
      <Box padding="20px">
        <LoadingCard useLogo delayed itemName="User Profiles" showRandomFacts />
      </Box>
    );
  }

  if (isError || userProfiles === undefined || userProfiles === null) {
    return (
      <Box padding="20px">
        <RequestErrorCard itemName="User Profiles" onRetry={refetch} />
      </Box>
    );
  }

  return (
    <Box
      minHeight="90vh"
      display="flex"
      padding="20px"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      bgcolor="background.default"
    >
      <FancyText variant="h4" sx={{ mb: 4 }}>
        Who's reading?
      </FancyText>
      <Grid spacing={4} justifyContent="center" width="100%" container>
        {userProfiles.map((userProfile) => (
          <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2 }} key={userProfile.id}>
            <UserProfileCard userProfile={userProfile} onClick={setProfile} />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
