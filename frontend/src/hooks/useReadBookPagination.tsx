import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";

const DEFAULT_SECTION = 0;
const DEFAULT_PAGE = 0;

export const useReadBookPagination = () => {
  const [params, setParams] = useSearchParams();

  const section = useMemo<number>(() => {
    const value = Number(params.get("section"));
    return Number.isFinite(value) && value > 0 ? value : DEFAULT_SECTION;
  }, [params]);

  const page = useMemo<number>(() => {
    const value = Number(params.get("page"));
    return Number.isFinite(value) && value > 0 ? value : DEFAULT_PAGE;
  }, [params]);

  const nextSection = () =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      const currentSection = Number(next.get("section")) || DEFAULT_SECTION;
      next.set("section", String(currentSection + 1));
      next.set("page", "0");
      return next;
    });

  const prevSection = () =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      const currentSection = Number(next.get("section")) || DEFAULT_SECTION;
      next.set("section", String(currentSection - 1));
      // Don't reset page here, content navigatedBackRef will set it to last page
      return next;
    });

  const setSection = (nextSection: number) =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      next.set("section", String(nextSection));
      next.set("page", "0");
      return next;
    });

  const setPage = (nextPage: number) =>
    setParams((prev) => {
      const next = new URLSearchParams(prev);
      next.set("page", String(nextPage));
      return next;
    });

  return {
    section,
    page,
    nextSection,
    prevSection,
    setSection,
    setPage,
  };
};
