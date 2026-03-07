import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";

const DEFAULT_SECTION = 0;

export const useReadBookPagination = () => {
  const [params, setParams] = useSearchParams();

  const section = useMemo<number>(() => {
    const value = Number(params.get("section"));
    return Number.isFinite(value) && value > 0 ? value : DEFAULT_SECTION;
  }, [params, 0]);

  const next = () =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      const currentSection = Number(next.get("section")) || DEFAULT_SECTION;
      next.set("section", String(currentSection + 1));
      return next;
    });

  const prev = () =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      const currentSection = Number(next.get("section")) || DEFAULT_SECTION;
      next.set("section", String(currentSection - 1));
      return next;
    });

  const set = (nextSection: number) =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      next.set("section", String(nextSection));
      return next;
    });

  return {
    section,
    next,
    prev,
    set,
  };
};
