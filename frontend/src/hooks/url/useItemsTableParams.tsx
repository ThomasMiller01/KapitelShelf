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

  const setItemsTableParams = ({
    nextPage,
    nextPageSize,
    nextSorting,
  }: setItemsTableParamsProps) => {
    setParams((prev) => {
      const next = new URLSearchParams(prev);

      // pagination
      if (nextPage) {
        next.set("page", String(nextPage));
      }
      if (nextPageSize) {
        next.set("pageSize", String(nextPageSize));
      }

      // sorting
      if (nextSorting) {
        const trimmedField = nextSorting.field?.trim() ?? "";
        if (!trimmedField || !nextSorting.sort) {
          next.delete("sort");
          return next;
        }

        next.set("sort", `${trimmedField}:${nextSorting.sort}`);
      }

      return next;
    });

    return Promise.resolve();
  };

  return {
    page,
    pageSize,
    sorting,
    setItemsTableParams,
  };
};
