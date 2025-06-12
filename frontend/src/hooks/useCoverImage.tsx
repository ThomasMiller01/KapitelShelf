import { useEffect, useState } from "react";

import bookCover from "../assets/books/nocover.png";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl, RenameFile, UrlToFile } from "../utils/FileUtils";

const FALLBACK_IMAGE_NAME = "cover.png";

interface useCoverImageProps {
  initial: BookDTO | undefined;
  fallback?: string | undefined;
}

interface useCoverImageReturn {
  coverImage: string | undefined;
  coverFile: File | undefined;
  updateCoverFromUrl: (url: string) => void;
  updateCoverFromFile: (file: File) => void;
  onLoadingError?: () => void;
}

export const useCoverImage = ({
  initial,
  fallback = bookCover,
}: useCoverImageProps): useCoverImageReturn => {
  const [image, setImage] = useState<string | undefined>(undefined);
  const [imageFile, setImageFile] = useState<File | undefined>(undefined);

  // set initial image url
  useEffect(() => {
    const imageUrl = CoverUrl(initial) ?? fallback;
    const imageName = initial?.cover?.fileName ?? FALLBACK_IMAGE_NAME;

    // download image file and set it as current image file
    UrlToFile(imageUrl).then((file) => {
      const renamedFile = RenameFile(file, imageName);
      setImageFile(renamedFile);
    });
  }, [initial, fallback]);

  // update image url, when image file changes
  useEffect(() => {
    if (imageFile === undefined) {
      setImage(undefined);
      return;
    }

    const url = URL.createObjectURL(imageFile);
    setImage(url);
    return (): void => URL.revokeObjectURL(url);
  }, [imageFile]);

  const updateCoverFromUrl = (url: string): void => {
    UrlToFile(url).then((file) => {
      const renamedFile = RenameFile(file, file.name ?? FALLBACK_IMAGE_NAME);
      setImageFile(renamedFile);
    });
  };

  const updateCoverFromFile = (file: File): void => {
    setImageFile(file);
  };

  const onLoadingError = (): void => {
    updateCoverFromUrl(fallback);
  };

  return {
    coverImage: image,
    coverFile: imageFile,
    updateCoverFromUrl,
    updateCoverFromFile,
    onLoadingError,
  };
};
