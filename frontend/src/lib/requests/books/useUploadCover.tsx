import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface UploadCoverMutationProps {
  bookId: string;
  coverFile: File;
}

export const useUploadCover = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async ({ bookId, coverFile }: UploadCoverMutationProps) => {
      const { data } = await clients.books.booksBookIdCoverPost(
        bookId,
        coverFile
      );
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Uploading Cover",
      },
    },
  });
};
