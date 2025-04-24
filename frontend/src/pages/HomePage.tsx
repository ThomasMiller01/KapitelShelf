import {
  Box,
  Button,
  Container,
  Stack,
  styled,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import KapitelShelfLogo from "/kapitelshelf.png";

import { useMobile } from "../hooks/isMobile";

const ProductNameText = styled(Typography)({
  fontFamily: "Playwrite AU SA",
  fontWeight: "200",
  lineHeight: "2",
  fontFeatureSettings: '"calt"',
});

const HomePage = (): ReactElement => {
  const navigate = useNavigate();
  const { isMobile } = useMobile();

  return (
    <Box
      sx={{
        mt: "12%",
        display: "flex",
      }}
    >
      <Container sx={{ textAlign: "center" }}>
        <Stack
          direction="row"
          justifyContent="center"
          alignItems="center"
          spacing={2}
        >
          <img
            src={KapitelShelfLogo}
            alt="KapitelShelf logo"
            style={{ width: isMobile ? "70px" : "100px" }}
          />
          <ProductNameText variant="h2">KapitelShelf</ProductNameText>
        </Stack>
        <Typography variant="h6" color="text.secondary" mt="15px">
          Welcome to your personal library. Manage your books, track your
          reading, and explore your collection.
        </Typography>
        <Box mt={4}>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate("/library")}
          >
            View My Books
          </Button>
        </Box>
      </Container>
    </Box>
  );
};

export default HomePage;
