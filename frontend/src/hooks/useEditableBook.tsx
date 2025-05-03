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

  const setCategories = useCallback((items: string[]) => {
    const dto: CategoryDTO[] = items.map((name) => ({ name }));
    setBook((v) => ({ ...v, categories: dto }));
  }, []);

  const setTags = useCallback((items: string[]) => {
    const dto: TagDTO[] = items.map((name) => ({ name }));
    setBook((v) => ({ ...v, tags: dto }));
  }, []);

  return {
    book,
    handleTextChange,
    handleNumberChange,
    handleReleaseDateChange,
    setCategories,
    setTags,
  };
};
