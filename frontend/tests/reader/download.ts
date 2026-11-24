import axios from "axios";

import { loadBookFile } from "../../src/features/reader/utils/BookFile";
import { BooksApi, Configuration } from "../../src/lib/api/KapitelShelf.Api";

export const checkBookDownload = async (): Promise<void> => {
  const original = "a".repeat(64);
  const current = "b".repeat(64);
  const metadata = { fileName: "old.txt", mimeType: "text/plain", sha256: original };
  const refresh = { fileName: "new.txt", mimeType: "text/plain", sha256: current };
  const endpoint = "https://reader.invalid/books/fixture";
  const downloadedFile = new Blob(["Fixture contents"], { type: "text/plain" });

  for (const [headers, expected, requestCount] of [
    [[original], original, 1],
    [[current, current], current, 3],
    [[current, "c".repeat(64)], undefined, 3],
    [[undefined], undefined, 1],
  ] as const) {
    const calls: string[] = [];
    let downloads = 0;
    const http = axios.create({
      adapter: async (config) => {
        const url = config.url ?? "";
        calls.push(url);
        const isDownload = url === `${endpoint}/file?includeChecksum=true`;
        if (!isDownload && url !== endpoint) {
          throw new Error(`Unexpected request: ${url}`);
        }
        if (isDownload && (config.responseType !== "blob" || !config.signal)) {
          throw new Error("The download did not request a cancellable blob response");
        }
        const hash = isDownload ? headers[downloads++] : undefined;
        return {
          config,
          status: 200,
          statusText: "OK",
          data: isDownload ? downloadedFile : { location: { type: 1, fileInfo: refresh } },
          headers: hash ? { "x-book-file-sha256": hash } : {},
        };
      },
    });
    const client = new BooksApi(new Configuration({ basePath: "https://reader.invalid" }), undefined, http);
    const controller = new AbortController();
    const result = await loadBookFile(client, "fixture", metadata, controller.signal);
    if (result.sha256 !== expected || calls.length !== requestCount || result.file.name !== (requestCount === 3 ? "new.txt" : "old.txt")) {
      throw new Error(`Unexpected checksum, filename, or request count for ${headers.join(", ")}`);
    }
    controller.abort();
    await loadBookFile(client, "fixture", metadata, controller.signal).then(
      () => { throw new Error("A cancelled download completed"); },
      (error: unknown) => { if (error !== controller.signal.reason) { throw error; } },
    );
    if (calls.length !== requestCount) {
      throw new Error("A cancelled download sent a request");
    }
  }
};
