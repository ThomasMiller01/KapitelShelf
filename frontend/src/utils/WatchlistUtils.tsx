import type { SeriesDTO } from "../lib/api/KapitelShelf.Api";
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
