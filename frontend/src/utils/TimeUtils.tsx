export const SECOND_MS = 1000;
export const MINUTE_MS = 60 * SECOND_MS;
export const HOUR_MS = 60 * MINUTE_MS;
export const DAY_MS = 24 * HOUR_MS;
export const WEEK_MS = 7 * DAY_MS;
export const MONTH_MS = 30 * DAY_MS;

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
  allowPast?: boolean
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

  // less than 1 minute -> show in seconds
  if (absDifferenceInMs < MINUTE_MS) {
    const secs = Math.round(absDifferenceInMs / SECOND_MS);
    return `${preText}${secs} second${secs !== 1 ? "s" : ""}${postText}`;
  }

  // less than 1 hour -> show in minutes
  if (absDifferenceInMs < HOUR_MS) {
    const mins = Math.round(absDifferenceInMs / MINUTE_MS);
    return `${preText}${mins} minute${mins !== 1 ? "s" : ""}${postText}`;
  }

  // less than 1 day -> show in hours
  if (absDifferenceInMs < DAY_MS) {
    const hours = Math.round(absDifferenceInMs / HOUR_MS);
    return `${preText}${hours} hour${hours !== 1 ? "s" : ""}${postText}`;
  }

  // less than 1 week -> show in days
  if (absDifferenceInMs < WEEK_MS) {
    const days = Math.round(absDifferenceInMs / DAY_MS);
    return `${preText}${days} day${days !== 1 ? "s" : ""}${postText}`;
  }

  // less than 1 month -> show in weeks
  if (absDifferenceInMs < MONTH_MS) {
    const weeks = Math.round(absDifferenceInMs / WEEK_MS);
    return `${preText}${weeks} week${weeks !== 1 ? "s" : ""}${postText}`;
  }

  // Otherwise, just show the date (yyyy-mm-dd)
  return localeDate.toISOString().slice(0, 10);
};
