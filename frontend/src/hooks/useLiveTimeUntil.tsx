import { useEffect, useRef, useState } from "react";

import {
  DAY_MS,
  FormatTimeUntil,
  HOUR_MS,
  MINUTE_MS,
  MONTH_MS,
  SECOND_MS,
  WEEK_MS,
} from "../utils/TimeUtils";

interface useLiveTimeUntilProps {
  dateStr?: string | null;
  allowPast?: boolean;
}

export const useLiveTimeUntil = ({
  dateStr,
  allowPast = true,
}: useLiveTimeUntilProps): string => {
  const [display, setDisplay] = useState(() =>
    FormatTimeUntil(dateStr, allowPast)
  );
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    const schedule = (): void => {
      const now = new Date();
      const date = dateStr ? new Date(dateStr) : null;
      if (!date || isNaN(date.getTime())) {
        return;
      }

      const diffMs = date.getTime() - now.getTime();
      const absDiffMs = Math.abs(diffMs);

      if (diffMs <= 0 && !allowPast) {
        // dont update if it is in the past
        // but format one last time to allow "0 seconds"
        setDisplay(FormatTimeUntil(dateStr, allowPast));
        return;
      }

      // Decide interval and next update
      let nextUpdate = 0;
      if (absDiffMs < MINUTE_MS) {
        // < 1 min
        nextUpdate = SECOND_MS;
      } else if (absDiffMs < HOUR_MS) {
        // < 1 hr
        nextUpdate = MINUTE_MS;
      } else if (absDiffMs < DAY_MS) {
        // < 1 day
        nextUpdate = HOUR_MS;
      } else if (absDiffMs < WEEK_MS) {
        // < 1 week
        nextUpdate = DAY_MS;
      } else if (absDiffMs < MONTH_MS) {
        // < 1 month
        nextUpdate = WEEK_MS;
      } else {
        return;
      } // no update if > 1 month

      timerRef.current = setTimeout(() => {
        setDisplay(FormatTimeUntil(dateStr, allowPast));
        schedule(); // re-schedule
      }, nextUpdate);
    };

    setDisplay(FormatTimeUntil(dateStr, allowPast));
    schedule();

    return (): void => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, [dateStr, allowPast]);

  return display;
};
