import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";

export type SortingDirection = "asc" | "desc";

export interface SortingModel {
  field: string | null;
  sort: SortingDirection | null;
}

interface useItemsTableParamsProps {
  defaultPage?: number;
  defaultPageSize?: number;
}

export interface setItemsTableParamsProps {
  nextPage?: number;
  nextPageSize?: number;
  nextSorting?: SortingModel;
  nextFilter?: string;
}

export const useItemsTableParams = ({
  defaultPage = 1,
  defaultPageSize = 24,
}: useItemsTableParamsProps) => {
  const [params, setParams] = useSearchParams();

  const page = useMemo<number>(() => {
    const value = Number(params.get("page"));
    return Number.isFinite(value) && value > 0 ? value : defaultPage;
  }, [params, defaultPage]);

  const pageSize = useMemo<number>(() => {
    const value = Number(params.get("pageSize"));
    return Number.isFinite(value) && value > 0 ? value : defaultPageSize;
  }, [params, defaultPageSize]);

  const sorting = useMemo<SortingModel>(() => {
    const raw = params.get("sort");
    if (!raw) {
      return { field: null, sort: null };
    }

    const [fieldRaw, directionRaw] = raw.split(":");
    const parsedField = fieldRaw?.trim() ?? "";

    const parsedSort: SortingDirection | null =
      directionRaw === "asc" || directionRaw === "desc" ? directionRaw : null;

    if (!parsedField || !parsedSort) {
      return { field: null, sort: null };
    }

    return { field: parsedField, sort: parsedSort };
  }, [params]);

  const filter = useMemo<string | null>(() => params.get("filter"), [params]);

  const setItemsTableParams = ({
    nextPage,
    nextPageSize,
    nextSorting,
    nextFilter,
  }: setItemsTableParamsProps) => {
    setParams((prev) => {
      const next = new URLSearchParams(prev);

      // pagination
      if (nextPage !== undefined) {
        next.set("page", String(nextPage));
      }
      if (nextPageSize !== undefined) {
        next.set("pageSize", String(nextPageSize));
      }

      // sorting
      if (nextSorting !== undefined) {
        const trimmedField = nextSorting.field?.trim() ?? "";
        if (!trimmedField || !nextSorting.sort) {
          next.delete("sort");
        } else {
          next.set("sort", `${trimmedField}:${nextSorting.sort}`);
        }
      }

      // filter
      if (nextFilter !== undefined) {
        const trimmedField = nextFilter.trim();
        if (!trimmedField) {
          next.delete("filter");
        } else {
          next.set("filter", trimmedField);
        }
      }

      return next;
    });
  };

  return {
    page,
    pageSize,
    sorting,
    filter,
    setItemsTableParams,
  };
};
