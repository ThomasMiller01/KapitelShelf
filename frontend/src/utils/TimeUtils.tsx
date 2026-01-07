export const SECOND_MS = 1000;
export const MINUTE_MS = 60 * SECOND_MS;
export const HOUR_MS = 60 * MINUTE_MS;
export const DAY_MS = 24 * HOUR_MS;
export const WEEK_MS = 7 * DAY_MS;
export const MONTH_MS = 30 * DAY_MS;
export const YEAR_MS = 365 * DAY_MS;

type FormatTimeUntilMode =
  | "auto"
  | "calender"
  | "readable"
  | "date"
  | "time"
  | "datetime";

export const FormatTime = (dateUtc?: string | null): string => {
  if (!dateUtc) {
    return "-";
  }

  const localeDate = new Date(dateUtc);
  if (isNaN(localeDate.getTime())) {
    return "Invalid date";
  }

  return localeDate.toLocaleString();
};

export const FormatTimeUntil = (
  dateUtc?: string | null,
  allowPast?: boolean,
  mode: FormatTimeUntilMode = "auto"
): string => {
  if (!dateUtc) {
    return "-";
  }

  const localeDate = new Date(dateUtc);
  if (isNaN(localeDate.getTime())) {
    return "Invalid date";
  }

  const now = new Date();

  // calculate difference between
  const differenceInMs = localeDate.getTime() - now.getTime();
  const absDifferenceInMs = Math.abs(differenceInMs);

  const isPast = differenceInMs < 0;
  if (isPast && !allowPast) {
    return "-";
  }

  const preText = isPast ? "" : "in ";
  const postText = isPast ? " ago" : "";

  const seconds = Math.round(absDifferenceInMs / SECOND_MS);
  const minutes = Math.round(absDifferenceInMs / MINUTE_MS);
  const hours = Math.round(absDifferenceInMs / HOUR_MS);
  const days = Math.round(absDifferenceInMs / DAY_MS);
  const weeks = Math.round(absDifferenceInMs / WEEK_MS);
  const months = Math.round(absDifferenceInMs / MONTH_MS);

  const format = (value: number, unit: string): string =>
    `${preText}${value} ${unit}${value !== 1 ? "s" : ""}${postText}`;

  // date (yyyy-mm-dd)
  const date = localeDate.toISOString().slice(0, 10);

  switch (mode) {
    default:
    case "auto":
      if (absDifferenceInMs < MINUTE_MS) {
        return format(seconds, "second");
      } else if (absDifferenceInMs < HOUR_MS) {
        return format(minutes, "minute");
      } else if (absDifferenceInMs < DAY_MS) {
        return format(hours, "hour");
      } else if (absDifferenceInMs < WEEK_MS) {
        return format(days, "day");
      } else if (absDifferenceInMs < MONTH_MS) {
        return format(weeks, "week");
      } else if (absDifferenceInMs < YEAR_MS) {
        return format(months, "month");
      }
      return date;

    case "calender":
      if (absDifferenceInMs < MINUTE_MS) {
        return format(seconds, "second");
      } else if (absDifferenceInMs < HOUR_MS) {
        return format(minutes, "minute");
      } else if (absDifferenceInMs < DAY_MS) {
        return format(hours, "hour");
      } else if (absDifferenceInMs < WEEK_MS) {
        return format(days, "day");
      }
      return date;

    case "readable":
      if (absDifferenceInMs < MINUTE_MS) {
        return format(seconds, "second");
      } else if (absDifferenceInMs < HOUR_MS) {
        return format(minutes, "minute");
      } else if (absDifferenceInMs < DAY_MS) {
        return format(hours, "hour");
      } else if (absDifferenceInMs < WEEK_MS) {
        return format(days, "day");
      } else if (absDifferenceInMs < MONTH_MS) {
        return `${preText}about ${weeks} weeks${postText}`;
      } else if (absDifferenceInMs < YEAR_MS) {
        return `${preText}about ${months} months${postText}`;
      }
      return `${preText}over a year${postText}`;

    case "date":
      return date;

    case "time":
      return localeDate.toLocaleTimeString();

    case "datetime":
      return localeDate.toLocaleString();
  }
};
