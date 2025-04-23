import { Box, Button, Container, Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import { useToggleTheme } from "../context/ThemeContext";

const HomePage = (): ReactElement => {
  const navigate = useNavigate();
  const toggleTheme = useToggleTheme();

  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: "background.default",
        color: "text.primary",
        display: "flex",
        alignItems: "center",
      }}
    >
      <Container maxWidth="md" sx={{ textAlign: "center" }}>
        <Typography variant="h2" gutterBottom>
          📚 KapitelShelf
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Welcome to your personal library. Manage your books, track your
          reading, and explore your collection.
        </Typography>
        <Box mt={4}>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate("/books")}
          >
            View My Books
          </Button>
        </Box>
        <Box mt={4}>
          <Button variant="contained" size="large" onClick={toggleTheme}>
            Toggle Theme
          </Button>
        </Box>
      </Container>
    </Box>
  );
};

export default HomePage;
