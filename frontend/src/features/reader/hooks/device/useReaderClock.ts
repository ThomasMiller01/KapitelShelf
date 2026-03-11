import { useEffect, useState } from "react";

const formatTime = (): string =>
  new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

export const useReaderClock = (): string => {
  const [currentTime, setCurrentTime] = useState(formatTime);

  useEffect(() => {
    const interval = setInterval(() => setCurrentTime(formatTime()), 10000);
    return () => clearInterval(interval);
  }, []);

  return currentTime;
};
