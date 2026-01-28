import { BookDTO } from "../lib/api/KapitelShelf.Api";

export const normalizeBook = (b: BookDTO) => {
  const authorFirst = b.author?.firstName?.trim() ?? "";
  const authorLast = b.author?.lastName?.trim() ?? "";
  const authorName = `${authorFirst} ${authorLast}`.trim();

  const seriesName = b.series?.name?.trim() ?? "";

  const categories = (b.categories ?? [])
    .map((x) => x.name?.trim() ?? "")
    .filter((x) => x.length > 0)
    .sort();

  const tags = (b.tags ?? [])
    .map((x) => x.name?.trim() ?? "")
    .filter((x) => x.length > 0)
    .sort();

  const releaseDateIso =
    b.releaseDate === undefined || b.releaseDate === null
      ? null
      : (() => {
          const d = new Date(b.releaseDate);
          return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(
            2,
            "0",
          )}-${String(d.getUTCDate()).padStart(2, "0")}`;
        })();

  return {
    title: (b.title ?? "").trim(),
    description: (b.description ?? "").trim(),
    pageNumber: b.pageNumber ?? null,
    seriesNumber: b.seriesNumber ?? null,
    authorName,
    seriesName,
    releaseDateIso,
    categories,
    tags,
  };
};
