import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { AuthorDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useUpdateAuthor = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (author: AuthorDTO) => {
      if (author.id === undefined) {
        return null;
      }

      await clients.authors.authorsAuthorIdPut(author.id, author);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating author",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["authors"] }),
  });
};
