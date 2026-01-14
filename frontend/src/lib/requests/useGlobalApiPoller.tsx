import { useNotificationStatsPoller } from "./notifications/useNotificationStatsPoller";

export const useGlobalApiPoller = () => {
  useNotificationStatsPoller();
};
