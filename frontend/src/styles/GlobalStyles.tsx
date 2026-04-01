import { GlobalStyles } from "@mui/material";

export const KapitelShelfGlobalStyles: React.FC = () => {
  return (
    <GlobalStyles
      styles={{
        ...ScrollbarStyles,
      }}
    />
  );
};

export const ScrollbarStyles = {
  "*": {
    scrollbarWidth: "thin",
    scrollbarColor: "#b0b0b0 transparent",
  },

  "*::-webkit-scrollbar": {
    width: 8,
    height: 8,
  },

  "*::-webkit-scrollbar-track": {
    background: "transparent",
  },

  "*::-webkit-scrollbar-thumb": {
    backgroundColor: "#b0b0b0",
    borderRadius: 8,
  },

  "*::-webkit-scrollbar-thumb:hover": {
    backgroundColor: "#8c8c8c",
  },

  "*::-webkit-scrollbar-corner": {
    background: "transparent",
  },
};
