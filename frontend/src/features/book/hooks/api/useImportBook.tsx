import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

export const useImportBook = () => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useMutation({
    mutationFn: async (file: File) => {
      const { data } = await clients.books.booksImportPost(profile?.id, file);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing Book",
      },
    },
  });
};
