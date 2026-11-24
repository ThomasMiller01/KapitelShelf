import axios, { type AxiosResponse } from "axios";
import type { ReactElement } from "react";
import { flushSync } from "react-dom";
import { createRoot } from "react-dom/client";

import { useReadBook } from "../../src/features/reader/hooks/useReadBook";
import { ApiProvider } from "../../src/shared/contexts/ApiProvider";

// eslint-disable-next-line react-refresh/only-export-components -- This probe only runs on the fixture page.
const Probe = ({ name }: { name: string }): ReactElement => {
  const result = useReadBook({ id: name, location: { type: 1, fileInfo: { fileName: `${name}.txt` } } });
  return <span>{result.content?.metadata.title ?? result.error ?? "loading"}</span>;
};

const nextTick = (): Promise<void> => new Promise((resolve) => window.setTimeout(resolve, 0));

export const checkReaderLoading = async (): Promise<void> => {
  const originalAdapter = axios.defaults.adapter;
  const originalText = File.prototype.text;
  const readFiles: string[] = [];
  let firstAborted = false;
  let requestCount = 0;
  let finishFirst = (): void => undefined;
  const firstDownload = new Promise<void>((resolve) => { finishFirst = resolve; });
  const host = document.createElement("div");
  host.hidden = true;
  document.body.append(host);
  const root = createRoot(host);

  File.prototype.text = function (this: File): Promise<string> {
    readFiles.push(this.name);
    return originalText.call(this);
  };
  axios.defaults.adapter = async (config): Promise<AxiosResponse> => {
    requestCount += 1;
    if (config.url?.includes("/first/file?")) {
      await firstDownload;
      firstAborted = config.signal?.aborted === true;
    }
    return {
      config,
      status: 200,
      statusText: "OK",
      data: new Blob(["A complete sentence for the reader."], { type: "text/plain" }),
      headers: {},
    };
  };

  const show = (name: string): void => flushSync(() => root.render(<ApiProvider><Probe name={name} /></ApiProvider>));
  try {
    show("first");
    for (let attempt = 0; requestCount === 0 && attempt < 100; attempt++) {
      await nextTick();
    }
    show("second");
    for (let attempt = 0; host.textContent === "loading" && attempt < 100; attempt++) {
      await nextTick();
    }
    finishFirst();
    await nextTick();
    show("second");
    await nextTick();

    if (host.textContent !== "second" || !firstAborted || requestCount !== 2
      || readFiles.length !== 1 || readFiles[0] !== "second.txt") {
      throw new Error("A replaced reader load was not cancelled or the visible book was parsed more than once");
    }
  } finally {
    finishFirst();
    root.unmount();
    host.remove();
    axios.defaults.adapter = originalAdapter;
    File.prototype.text = originalText;
  }
};
