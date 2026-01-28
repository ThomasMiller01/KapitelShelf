import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { IsMobileApp } from "../../../utils/MobileUtils";

export const useOneDriveStartOAuthFlow = () => {
  const { clients } = useApi();

  const redirectUrl = IsMobileApp()
    ? "kapitelshelf://auth/callback"
    : window.location.href;

  return useMutation({
    mutationFn: async () => {
      const { data } = await clients.onedrive.cloudstorageOnedriveOauthGet(
        redirectUrl
      );
      window.location.href = data;
    },
  });
};
