import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";

export const usePaginationParams = (defaultPage = 1, defaultPageSize = 24) => {
  const [params, setParams] = useSearchParams();

  const page = useMemo<number>(() => {
    const value = Number(params.get("page"));
    return Number.isFinite(value) && value > 0 ? value : defaultPage;
  }, [params, defaultPage]);

  const pageSize = useMemo<number>(() => {
    const value = Number(params.get("pageSize"));
    return Number.isFinite(value) && value > 0 ? value : defaultPageSize;
  }, [params, defaultPageSize]);

  const setPagination = (nextPage: number, nextPageSize: number) => {
    setParams((prev) => {
      prev.set("page", String(nextPage));
      prev.set("pageSize", String(nextPageSize));
      return prev;
    });
  };

  return { page, pageSize, setPagination };
};
