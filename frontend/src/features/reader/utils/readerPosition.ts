import { mapPage } from "./pageProgress.ts";

export interface ReaderLocation {
  section: number;
  page: number;
}

interface Position {
  sectionCount: number;
  section: number;
  page: number | "last";
  pageCount: number | null;
  saveAfterMeasure: boolean;
  lastMove?: ReaderLocation;
}

type Action =
  | { type: "open" | "jump"; location: ReaderLocation }
  | { type: "turn"; direction: -1 | 1 }
  | { type: "measure"; section: number; pageCount: number };

const clamp = (value: number, max: number): number =>
  Number.isSafeInteger(value) ? Math.max(0, Math.min(value, max)) : 0;

export const readLocation = (params: URLSearchParams): ReaderLocation | undefined => {
  const readIndex = (name: string): number | undefined => {
    const value = params.get(name);
    return value !== null && /^\d+$/.test(value) && Number.isSafeInteger(Number(value)) ? Number(value) : undefined;
  };
  const section = readIndex("section");
  const page = readIndex("page");
  return section === undefined && page === undefined ? undefined : { section: section ?? 0, page: page ?? 0 };
};

export const createPosition = (sectionCount: number, location: ReaderLocation): Position => ({
  sectionCount,
  section: clamp(location.section, sectionCount - 1),
  page: clamp(location.page, Number.MAX_SAFE_INTEGER),
  pageCount: null,
  saveAfterMeasure: false,
});

export const changePosition = (state: Position, action: Action): Position => {
  if (action.type === "measure") {
    const { pageCount } = action;
    if (action.section !== state.section || pageCount < 1 || pageCount === state.pageCount) {
      return state;
    }
    let page = state.page === "last" ? pageCount - 1 : clamp(state.page, pageCount - 1);
    if (state.page !== "last" && state.pageCount !== null) {
      // shortcut: preserve relative position on reflow, use text anchors if exact restore is needed.
      page = mapPage(state.page, state.pageCount, pageCount);
    }
    return {
      ...state, page, pageCount, saveAfterMeasure: false,
      lastMove: state.saveAfterMeasure ? { section: state.section, page } : state.lastMove,
    };
  }

  let { section } = state;
  let page: Position["page"];
  if (action.type === "turn") {
    if (state.pageCount === null || state.page === "last") {
      return state;
    }
    page = state.page + action.direction;
    if (page < 0 || page === state.pageCount) {
      section += action.direction;
      if (section < 0 || section >= state.sectionCount) {
        return state;
      }
      page = action.direction === -1 ? "last" : 0;
    }
  } else {
    section = clamp(action.location.section, state.sectionCount - 1);
    page = clamp(action.location.page, Number.MAX_SAFE_INTEGER);
  }

  const pageCount = section === state.section ? state.pageCount : null;
  if (pageCount !== null && page !== "last") {
    page = clamp(page, pageCount - 1);
  }
  if (section === state.section && page === state.page) {
    return state;
  }

  const save = action.type !== "open";
  return {
    ...state, section, page, pageCount,
    saveAfterMeasure: save && pageCount === null,
    lastMove: save && pageCount !== null && page !== "last" ? { section, page } : state.lastMove,
  };
};
