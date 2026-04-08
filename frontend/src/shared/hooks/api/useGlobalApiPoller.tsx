import { useNotificationStatsPoller } from "../../../features/notifications/hooks/api/useNotificationStatsPoller";

export const useGlobalApiPoller = () => {
  useNotificationStatsPoller();
};
