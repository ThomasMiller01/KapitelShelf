import {
  Box,
  Grid,
  Stack,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";

import { ProfileImage } from "../../components/ProfileImage";
import type { ProfileImageTypeDTO } from "../../lib/api/KapitelShelf.Api/api";
import { ProfileImageCategories } from "../../utils/UserProfileUtils";

interface ProfileImageListProps {
  onClick: (profileImageType: ProfileImageTypeDTO) => void;
  profileColor: string;
}

export const ProfileImageList: React.FC<ProfileImageListProps> = (props) => (
  <Stack spacing={2}>
    {Object.entries(ProfileImageCategories).map(([category, images]) => (
      <Box>
        <Typography variant="subtitle1" textTransform="uppercase" gutterBottom>
          {category}
        </Typography>
        <ProfileImageCategoryList key={category} {...props} images={images} />
      </Box>
    ))}
  </Stack>
);

interface ProfileImageCategoryListProps {
  images: ProfileImageTypeDTO[];
  profileColor: string;
  onClick: (profileImageType: ProfileImageTypeDTO) => void;
}

const ProfileImageCategoryList: React.FC<ProfileImageCategoryListProps> = ({
  images,
  profileColor,
  onClick,
}) => {
  const theme = useTheme();
  const isSm = useMediaQuery(theme.breakpoints.down("md"));

  return (
    <Grid
      columnSpacing={{ xs: 3, md: 3 }}
      rowSpacing={{ xs: 1.3, md: 3 }}
      columns={10}
      justifyContent="start"
      width="100%"
      container
    >
      {images.map((profileImageType) => (
        <Grid
          size={{ xs: 5, sm: 3, md: 2.5, lg: 2 }}
          key={profileImageType}
          justifyItems="center"
        >
          <ProfileImage
            profileImageType={profileImageType}
            profileColor={profileColor}
            maxHeight={isSm ? 110 : 200}
            onClick={onClick}
          />
        </Grid>
      ))}
    </Grid>
  );
};
