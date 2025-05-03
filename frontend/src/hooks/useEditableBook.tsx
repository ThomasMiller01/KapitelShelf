import type { Dayjs } from "dayjs";
import { useCallback, useState } from "react";

import type {
  BookDTO,
  CategoryDTO,
  TagDTO,
} from "../lib/api/KapitelShelf.Api/api";

interface EditableBookResult {
  book: BookDTO;
  handleTextChange: <K extends keyof BookDTO>(
    key: K
  ) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
  handleNumberChange: (
    key: "pageNumber" | "seriesNumber"
  ) => (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleReleaseDateChange: (date: Dayjs | null) => void;
  handleAuthorChange: (author: string) => void;
  handleSeriesChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  setCategories: (items: string[]) => void;
  setTags: (items: string[]) => void;
}

export const useEditableBook = (initial?: BookDTO): EditableBookResult => {
  const [book, setBook] = useState<BookDTO>({
    title: initial?.title ?? null,
    description: initial?.description ?? null,
    releaseDate: initial?.releaseDate ?? null,
    pageNumber: initial?.pageNumber ?? null,
    series: initial?.series ?? undefined,
    seriesNumber: initial?.seriesNumber ?? null,
    author: initial?.author ?? undefined,
    categories: initial?.categories ?? [],
    tags: initial?.tags ?? [],
    cover: initial?.cover ?? undefined,
    location: initial?.location ?? undefined,
  });

  const handleTextChange =
    <K extends keyof BookDTO>(key: K) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>): void => {
      const raw = e.target.value;
      setBook((v) => ({
        ...v,
        [key]: raw as BookDTO[K],
      }));
    };

  const handleNumberChange =
    (key: "pageNumber" | "seriesNumber") =>
    (e: React.ChangeEvent<HTMLInputElement>): void => {
      const num = Number(e.target.value);
      setBook(
        (v) =>
          ({
            ...v,
            [key]: isNaN(num) ? null : num,
          } as Partial<BookDTO>)
      );
    };

  const handleReleaseDateChange = useCallback((date: Dayjs | null) => {
    let dateIso = null;
    try {
      dateIso = date?.toISOString();
    } catch {
      return;
    }

    setBook((v) => ({
      ...v,
      releaseDate: dateIso,
    }));
  }, []);

  const handleAuthorChange = useCallback((author: string) => {
    if (!author) {
      setBook((v) => ({ ...v, author: undefined }));
      return;
    }

    let firstName: string;
    let lastName: string;

    const parts = author.split(" ");
    if (parts.length === 1) {
      firstName = parts[0];
      lastName = "";
    } else {
      // everything except last goes into firstName
      firstName = parts.slice(0, -1).join(" ");
      lastName = parts[parts.length - 1];
    }

    setBook((v) => ({
      ...v,
      author: {
        firstName,
        lastName: lastName || null,
      },
    }));
  }, []);

  const handleSeriesChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const series = e.target.value;
      if (series === "") {
        setBook((v) => ({
          ...v,
          series: undefined,
        }));
        return;
      }

      setBook((v) => ({
        ...v,
        series: {
          name: series,
        },
      }));
    },
    []
  );

  const setCategories = useCallback((items: string[]) => {
    const categories: CategoryDTO[] = items.map((name) => ({ name }));
    setBook((v) => ({ ...v, categories }));
  }, []);

  const setTags = useCallback((items: string[]) => {
    const tags: TagDTO[] = items.map((name) => ({ name }));
    setBook((v) => ({ ...v, tags }));
  }, []);

  return {
    book,
    handleTextChange,
    handleNumberChange,
    handleReleaseDateChange,
    handleAuthorChange,
    handleSeriesChange,
    setCategories,
    setTags,
  };
};
