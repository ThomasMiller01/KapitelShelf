import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { UserDTO } from "../../api/KapitelShelf.Api";

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
