import { styled } from "@mui/material";
import { MaterialDesignContent, SnackbarProvider } from "notistack";
import type { ReactElement, ReactNode } from "react";

import { useMobile } from "../../../hooks/useMobile";

const BaseNotification = styled(MaterialDesignContent)(() => ({
  padding: "0px 10px",
}));

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider = ({
  children,
}: NotificationProviderProps): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <SnackbarProvider
      maxSnack={5}
      dense={isMobile}
      hideIconVariant={isMobile}
      Components={{
        default: BaseNotification,
        success: BaseNotification,
        error: BaseNotification,
        warning: BaseNotification,
        info: BaseNotification,
      }}
    >
      {children}
    </SnackbarProvider>
  );
};
