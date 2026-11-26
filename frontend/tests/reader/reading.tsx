import { CssBaseline, ThemeProvider } from "@mui/material";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import axios, { type AxiosResponse } from "axios";
import { SnackbarProvider } from "notistack";
import type { ReactElement } from "react";
import { flushSync } from "react-dom";
import { createRoot } from "react-dom/client";
import { MemoryRouter, useLocation } from "react-router-dom";

import BookReader from "../../src/features/reader/components/BookReader";
import { ReaderThemeProvider } from "../../src/features/reader/components/ThemeProvider";
import { ApiProvider } from "../../src/shared/contexts/ApiProvider";
import { UserProfileContext } from "../../src/shared/contexts/UserProfileContext";
import { theme } from "../../src/styles/Theme";

// eslint-disable-next-line react-refresh/only-export-components -- This probe only runs on the fixture page.
const Search = (): ReactElement => <output hidden>{useLocation().search}</output>;
const pause = (ms = 25): Promise<void> => new Promise((resolve) => window.setTimeout(resolve, ms));

const until = async (check: () => boolean, description: string): Promise<void> => {
  for (let i = 0; i < 120; i++) {
    if (check()) {
      return;
    }
    await pause();
  }
  throw new Error(description);
};

export const checkReading = async (): Promise<void> => {
  const previousAdapter = axios.defaults.adapter;
  const host = document.createElement("div");
  host.style.cssText = "position:fixed;inset:0;visibility:hidden;display:flex";
  document.body.append(host);
  const root = createRoot(host);
  const query = new QueryClient();
  const file = new Blob([`FIRST CHAPTER\n\n${"One long sentence goes on and on in the first chapter. ".repeat(330)}\n\nLAST CHAPTER\n\nThe final sentence.`], { type: "text/plain" });
  const book = { id: "fixture", location: { type: 1 as const, fileInfo: { fileName: "fixture.txt" } } };
  const noop = (): void => undefined;
  const profile = { profile: { id: "profile" }, setProfile: noop, clearProfile: noop, syncProfile: noop };
  let stored = { currentSection: 0, currentPage: 0 };
  let saved = 0;
  axios.defaults.adapter = async (config): Promise<AxiosResponse> => {
    if (typeof config.data === "string" && config.data !== "null") {
      stored = JSON.parse(config.data) as typeof stored;
      saved++;
    }
    return { config, status: 200, statusText: "OK", data: config.url?.endsWith("/file") ? file : { ...stored }, headers: {} };
  };

  const show = (search: string): void => flushSync(() => root.render(
    <QueryClientProvider client={query}>
      <ApiProvider>
        <UserProfileContext.Provider value={profile}>
          <SnackbarProvider>
            <ThemeProvider theme={theme}>
              <CssBaseline />
              <MemoryRouter key={search} initialEntries={[`/read${search}`]}>
                <ReaderThemeProvider><BookReader book={book} /><Search /></ReaderThemeProvider>
              </MemoryRouter>
            </ThemeProvider>
          </SnackbarProvider>
        </UserProfileContext.Provider>
      </ApiProvider>
    </QueryClientProvider>,
  ));
  const params = (): URLSearchParams => new URLSearchParams(host.querySelector("output")?.textContent ?? "");
  const arrow = (key: string): void => {
    window.dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true }));
  };
  const atEnd = (): boolean => host.querySelector<HTMLButtonElement>('button[aria-label="Next page"]')?.disabled === true;

  try {
    show("?section=1&page=0&keep=1");
    await until(() => atEnd() && host.textContent?.includes("100%") === true, "The deep link did not restore the final page");
    arrow("ArrowRight");
    await pause(550);
    if (params().get("section") !== "1" || params().get("keep") !== "1" || saved !== 0) {
      throw new Error("Opening a link or turning past the last page changed the bookmark");
    }

    const readingPage = host.querySelector<HTMLElement>('[role="region"]')!;
    const columns = host.querySelector<HTMLElement>("[data-reader-content]")!.parentElement!.parentElement!;
    // Synthetic pointers cannot acquire browser pointer capture.
    readingPage.setPointerCapture = noop;
    const pointer = (type: string, x: number): void => flushSync(() => {
      readingPage.dispatchEvent(new PointerEvent(type, {
        bubbles: true, pointerId: 1, pointerType: "touch", clientX: x,
        buttons: type === "pointerdown" || type === "pointermove" ? 1 : 0,
      }));
    });
    const { left } = columns.getBoundingClientRect();
    pointer("pointerdown", 100);
    pointer("pointermove", 130);
    await until(() => columns.getBoundingClientRect().left > left + 20, "The page did not follow the finger before release");
    if (columns.getBoundingClientRect().left > left + 30 || params().get("section") !== "1") {
      throw new Error("The page moved too far during a short drag");
    }
    pointer("pointerup", 130);
    await until(() => columns.getBoundingClientRect().left === left, "A short drag did not return to the current page");

    pointer("pointerdown", 100);
    pointer("pointermove", 190);
    await until(() => columns.getBoundingClientRect().left > left + 60, "The page did not follow a long drag");
    pointer("pointercancel", 190);
    await until(() => columns.getBoundingClientRect().left === left, "Canceling did not return to the current page");
    if (params().get("section") !== "1") {
      throw new Error("Canceling the gesture turned the page");
    }

    pointer("pointerdown", 100);
    pointer("pointermove", 190);
    pointer("pointerup", 190);
    await until(() => params().get("section") === "0" && Number(params().get("page")) > 0, "Backward swipe did not reach the preceding chapter's last page");
    await until(() => saved === 1, "Deliberate navigation did not save once");
    const savedPage = stored.currentPage;
    host.style.width = "390px";
    await until(() => params().get("page") !== String(savedPage), "Resize did not repaginate the book").catch(() => {
      const columns = host.querySelector<HTMLElement>("[data-reader-content]")?.parentElement?.parentElement;
      throw new Error(`Resize did not repaginate page ${savedPage}. Host ${host.offsetWidth}, columns ${columns?.offsetWidth}x${columns?.offsetHeight}.`);
    });
    await pause(550);
    if (saved !== 1 || stored.currentSection !== 0) {
      throw new Error("Resize wrote a bookmark");
    }
    host.style.width = "";
    show("");
    await until(() => params().get("section") === "0" && params().get("page") === String(savedPage), "Reopening did not restore the saved page");
    await pause(550);
    if (saved !== 1) {
      throw new Error("Reopening wrote a bookmark");
    }
  } finally {
    root.unmount();
    host.remove();
    query.clear();
    axios.defaults.adapter = previousAdapter;
  }
};
