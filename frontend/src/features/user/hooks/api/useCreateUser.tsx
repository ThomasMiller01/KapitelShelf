import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { CreateUserDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useCreateUser = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (createUser: CreateUserDTO) =>
      clients.users.usersPost(createUser),
    meta: {
      notify: {
        enabled: true,
        operation: "Adding profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
