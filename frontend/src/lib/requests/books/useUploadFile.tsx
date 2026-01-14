import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface UploadFileMutationProps {
  bookId: string;
  bookFile: File;
}

export const useUploadFile = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async ({ bookId, bookFile }: UploadFileMutationProps) => {
      const { data } = await clients.books.booksBookIdFilePost(
        bookId,
        bookFile
      );
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Uploading file",
        showLoading: true,
      },
    },
  });
};
