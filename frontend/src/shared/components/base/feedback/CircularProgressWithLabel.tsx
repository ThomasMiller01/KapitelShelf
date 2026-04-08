import type { CircularProgressProps } from "@mui/material";
import { Box, CircularProgress, Typography } from "@mui/material";

interface CircularProgressWithLabelProps extends CircularProgressProps {
  progress: number | null | undefined;
}

export const CircularProgressWithLabel: React.FC<
  CircularProgressWithLabelProps
> = ({ progress, ...props }) => {
  if (progress === null || progress === undefined) {
    return <></>;
  }

  return (
    <Box sx={{ position: "relative", display: "inline-flex" }}>
      <CircularProgress variant="determinate" {...props} value={progress} />
      <Box
        sx={{
          top: 0,
          left: 0,
          bottom: 0,
          right: 0,
          position: "absolute",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <Typography
          variant="caption"
          component="div"
          sx={{ color: "text.secondary" }}
        >
          {progress}%
        </Typography>
      </Box>
    </Box>
  );
};
