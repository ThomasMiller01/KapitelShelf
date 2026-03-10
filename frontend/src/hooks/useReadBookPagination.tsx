import { useCallback, useEffect, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";

const DEFAULT_SECTION = 0;
const DEFAULT_PAGE = 0;

const readSectionParam = (params: URLSearchParams): number => {
  const value = Number(params.get("section"));
  return Number.isFinite(value) && value > 0 ? value : DEFAULT_SECTION;
};

const readPageParam = (params: URLSearchParams): number => {
  const value = Number(params.get("page"));
  return Number.isFinite(value) && value > 0 ? value : DEFAULT_PAGE;
};

export const useReadBookPagination = () => {
  const [params, setParams] = useSearchParams();

  const [section, setSectionState] = useState<number>(() =>
    readSectionParam(params),
  );
  const [page, setPageState] = useState<number>(() => readPageParam(params));

  const sectionRef = useRef(section);
  const pageRef = useRef(page);

  const commitPagination = useCallback(
    (nextSection: number, nextPage: number) => {
      sectionRef.current = nextSection;
      pageRef.current = nextPage;
      setSectionState(nextSection);
      setPageState(nextPage);

      setParams((prev) => {
        const next = new URLSearchParams(prev);
        next.set("section", String(nextSection));
        next.set("page", String(nextPage));
        return next;
      }, { replace: true });
    },
    [setParams],
  );

  useEffect(() => {
    const urlSection = readSectionParam(params);
    const urlPage = readPageParam(params);

    if (urlSection === sectionRef.current && urlPage === pageRef.current) {
      return;
    }

    sectionRef.current = urlSection;
    pageRef.current = urlPage;
    setSectionState(urlSection);
    setPageState(urlPage);
  }, [params]);

  const nextSection = () => {
    commitPagination(sectionRef.current + 1, DEFAULT_PAGE);
  };

  const prevSection = () => {
    // Don't reset page here, content navigatedBackRef will set it to last page.
    commitPagination(sectionRef.current - 1, pageRef.current);
  };

  const setSection = (nextSection: number) => {
    commitPagination(nextSection, DEFAULT_PAGE);
  };

  const setPage = (nextPage: number) => {
    commitPagination(sectionRef.current, nextPage);
  };

  return {
    section,
    page,
    nextSection,
    prevSection,
    setSection,
    setPage,
  };
};
