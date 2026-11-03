import { Container } from "@mui/material";

import type { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";
import { Timeline } from "./Timeline";

interface BottomControlsProps {
  pageProgress: ReturnType<typeof useBookPageProgress>;
  setLocation: (section: number, page: number) => void;
  savedPage?: number;
  savedSection?: number;
}

export const BottomControls: React.FC<BottomControlsProps> = ({
  pageProgress,
  setLocation,
  savedPage,
  savedSection,
}) => (
  <Container
    sx={{
      width: "100%",
      padding: "0 !important",
      paddingTop: "10px !important",
      position: "relative",
      "&::before": {
        content: '""',
        position: "absolute",
        inset: 0,
        boxShadow:
          "0px -2px 4px -1px rgba(0, 0, 0, 0.2), 0px -4px 5px 0px rgba(0, 0, 0, 0.14), 0px -1px 10px 0px rgba(0, 0, 0, 0.12)",
        clipPath: "inset(-20px 0 100% 0)",
        pointerEvents: "none",
      },
      marginTop: "15px !important",
    }}
  >
    <Timeline
      pageProgress={pageProgress}
      savedPage={savedPage}
      savedSection={savedSection}
      onNavigate={setLocation}
    />
  </Container>
);
