import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";
import { UserDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useEditUser = () => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useMutation({
    mutationFn: async (user: UserDTO) => {
      if (profile?.id === undefined) {
        return null;
      }

      await clients.users.usersUserIdPut(profile.id, user);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
