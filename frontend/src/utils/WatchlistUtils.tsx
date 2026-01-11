import type {
  BookDTO,
  SeriesDTO,
  WatchlistDTO,
} from "../lib/api/KapitelShelf.Api";
import { LocationTypeDTO } from "../lib/api/KapitelShelf.Api";

const LocationsSupportSeriesWatchlist: number[] = [LocationTypeDTO.NUMBER_2];

export const SeriesSupportsWatchlist = (
  series: SeriesDTO | null | undefined
): boolean => {
  if (series?.lastVolume?.location?.type === undefined) {
    return false;
  }

  return LocationsSupportSeriesWatchlist.includes(
    series.lastVolume.location.type
  );
};

// get release date from the first item
export const GetFirstReleaseDate = (watchlist: WatchlistDTO): Date | null => {
  const first: BookDTO | undefined = watchlist.items?.[0];
  if (!first?.releaseDate) {
    return null;
  }

  const date = new Date(first.releaseDate);
  return Number.isNaN(date.getTime()) ? null : date;
};

export const SplitByReleaseWindow = (
  data: WatchlistDTO[]
): {
  arrivingSoon: WatchlistDTO[];
  comingUp: WatchlistDTO[];
  later: WatchlistDTO[];
} => {
  const now = new Date();

  const weekFromNow = new Date(now);
  weekFromNow.setDate(now.getDate() + 7);

  const monthFromNow = new Date(now);
  monthFromNow.setDate(now.getDate() + 30);

  return {
    arrivingSoon: data.filter((w) => {
      const date = GetFirstReleaseDate(w);
      return !!date && date <= weekFromNow;
    }),

    comingUp: data.filter((w) => {
      const date = GetFirstReleaseDate(w);
      return !!date && date > weekFromNow && date <= monthFromNow;
    }),

    later: data.filter((w) => {
      const date = GetFirstReleaseDate(w);
      return !date || date > monthFromNow;
    }),
  };
};
