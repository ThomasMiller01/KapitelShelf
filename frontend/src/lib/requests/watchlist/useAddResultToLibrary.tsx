import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { BookDTO } from "../../api/KapitelShelf.Api";

export const useAddResultToLibrary = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (book: BookDTO) => {
      if (book.id === undefined) {
        return;
      }

      const { data } =
        await clients.watchlist.watchlistResultResultIdLibraryPut(book.id);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Adding book to library",
        showSuccess: true,
        showLoading: true,
      },
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["watchlist", profile?.id] });
    },
  });
};
