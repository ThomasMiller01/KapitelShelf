import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useDeleteUser = () => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useMutation({
    mutationFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      await clients.users.usersUserIdDelete(profile.id);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
