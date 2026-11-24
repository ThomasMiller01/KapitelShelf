# Book Reader Rewrite Plan

**Status** · Stage 1 parser baselines and stage 2 attachment identity are in place. Guarded bookmark storage is next.

**Baseline** · `c6ec92f` on `411-pbi-rewrite-bookreader`. The current user experience is described in the [Book Reader guide](references/book-reader.md).

## Scope and confirmed decisions

Replace the reader frontend, reading-position persistence, and Android reader lifecycle in stages, with one cutover through the existing reader route. Follow KapitelShelf’s feature structure, shared MUI components, React Query API hooks, generated client, and backend controller and EF Core conventions. Use the existing test tooling. The current implementation and its documented behavior are the starting point for regression cases.

- Preserve EPUB, FB2, TXT, and text-extractable PDF support. Keep the existing opening and error flows, responsive page layout, toolbar and controls, page turns by button, keyboard, tap and swipe, nested table of contents, timeline and saved marker, estimated progress, reader themes and fonts, time and available battery status, and Android orientation lock and status-bar behavior.
- For an unchanged file, resume on the page containing the first visible passage from the saved position, including after viewport, font, device, or orientation changes. On an illustration-only page, anchor to the visible packaged illustration. Bind anchors to the attachment's existing SHA-256. Use the numeric fallback if the file changes or the anchor cannot be resolved.
- Preserve existing saved data. Where section splitting changes, translate legacy section indexes to the corresponding source content. For old clients and links, use the closest corresponding legacy section and the legacy page when known. Otherwise use the start of that section.
- Interpret existing `section` and `page` links using legacy section numbering. Include a versioned content anchor in new links so they survive layout changes. Keep numeric coordinates as fallbacks. The anchor must contain no book text.
- Opening or restoring a location, including opening a deep link and leaving immediately, preserves the current bookmark. A deliberate page turn or jump can replace it, including when moving backward. An older tab closing later cannot overwrite newer progress from another device.
- If competing saves cannot be ordered reliably, keep the server-confirmed bookmark and reject the conflicting save. Keep the current page visible, show one notice, and clear the rejected pending save. The next open resumes from the server bookmark unless the reader has made a fresh deliberate move since the device learned the current revision.
- Let old apps open the book and use numeric fallback coordinates. Once a new reader has saved a revisioned bookmark for a book and profile, reject saves from clients that send no revision for that record.
- Keep the latest unsynced position locally for that book and profile across browser and app restarts. Retry on reconnect under the same conflict rules.
- If local storage and server sync both fail, keep reading with the pending position in memory. Warn once that closing may lose the position. Retry while open and clear the warning when either save succeeds.
- If a device has never received a server revision, automatically sync its offline progress only when the server can confirm there is no saved bookmark. When the server already has a bookmark, use the conflict behavior and accept a replacement after a fresh deliberate move.
- When saved progress cannot be loaded, allow reading from the explicit link, then the local position, then the start of the book. Indicate that syncing is unavailable. A late response must leave the visible page in place, and the opening position must never overwrite the saved bookmark.
- Retain safe book formatting, packaged images, and internal links. Opening or paginating a book must not automatically request remote resources. External links require an explicit click.
- Keep the label of an unresolved table-of-contents entry and allow its working children to expand and navigate. The unresolved entry itself does not navigate.
- Gate the cutover on manual checks in desktop Chromium, Firefox, and the Android app, using all four formats. Verify navigation, layout changes, and resume in each environment, plus backgrounding, reopening, status bar, and orientation lock on Android.

## Implementation stages

1. **Characterize the existing reader.** Capture representative fixtures and expected behavior for all four formats. Record how the old parsers number sections so saved indexes can be translated before changing the split logic. Reproduce the code-audit regressions involving duplicate loads, invalid navigation, boundary transitions, and stale saves.
2. **Add compatible storage.** Extend the existing reading endpoint and EF model with a nullable, versioned source anchor and a server-issued progress revision separate from `LastReadAt`. Check the revision and update the bookmark atomically. Retain `currentSection` and `currentPage` and all existing rows. Resolve legacy positions in the frontend and upgrade them on a subsequent deliberate save. Update the mapper and backend tests, then regenerate Swagger and the TypeScript client.
3. **Replace parsing and rendering.** Use one typed adapter per format and preserve source identities before chunking. Keep book loading cancellable, reject stale results, and release parser and object URL resources. Split markup without breaking nodes or losing supported content. At the import boundary allow safe markup and verified packaged resources. Use the shadow root for styling isolation. Continue skipping textless PDF pages and show the existing no-text message for wholly image-only PDFs.
4. **Replace navigation and persistence.** Use one reader state owner for validated navigation and measured pagination. Treat animation as presentation so it cannot block a page turn or commit progress. Route every input through the same bounded navigation actions. Resolve the initial location using the confirmed precedence rules. Measure content anchors after layout and preserve them when repaginating. Persist the latest deliberate save locally before syncing it, coalesce updates, and retry transient failures. Handle app backgrounding and page exit. Tie all asynchronous work to the active book and profile.
5. **Replace the lifecycle and cut over.** Keep current setting keys and validate stored values. Tie native orientation and status bar updates to reader entry, exit, and app resume. Apply the additive database migration and deploy the compatible backend before exposing the new reader. Connect the reader to the existing route in one release and remove the superseded frontend after the release checks pass. Update the Book Reader guide for confirmed user-facing changes.

### Location and pagination contracts

- Use a versioned locator containing the existing file SHA-256, a zero-based source unit, and a text offset or packaged-image index. Source units are the EPUB spine entry, FB2 source section, original PDF page, or whole TXT file. Assign positions on the normalized source before splitting so chunk sizes and viewport measurements cannot redefine them. Save the first visible content position and locate the column containing it after measuring. Remeasure on size, font, and image layout changes. Keep browser measurements and animation outside the navigation reducer.
- Keep `section` and `page` zero-based on the existing route. Add a versioned `location` query value with the anchor in new URLs and include legacy numeric coordinates. Use an explicit recognized anchor first, then explicit numeric coordinates, then the eligible saved position, then book start. Validate and clamp numeric coordinates after the destination has been measured. Replacing reader URL state must preserve unrelated query parameters and avoid a history entry per page turn.
- Reuse `BookDTO.location.fileInfo.sha256`. Store new book uploads under a unique revision directory inside the book directory, preserving the original filename. Commit the new attachment metadata after the file is written and use existing orphan-file cleanup for superseded attachments. This prevents same-name uploads from replacing bytes behind an existing file record.
- Return the SHA-256 with the reader's file response and expose the header through the existing CORS policy. For legacy storage paths, verify the checksum against the opened stream and repair a stale checksum only if that attachment is still current. On a metadata or checksum mismatch, refetch the attachment and download once. Use numeric coordinates if the hash cannot be verified. Key parsed content by attachment fingerprint.

### Saving and compatibility contracts

- Keep `PUT /books/{bookId}/reading` and its bodyless open behavior. `LastReadAt` still tracks reading activity for Home. Add nullable `contentAnchor` and `progressRevision` to the stored and returned bookmark. Preserve existing numeric positions and treat pre-existing rows conservatively as saved during migration. Newly created open-only rows have no progress revision.
- A new save sends the numeric fallback, a nullable anchor, the expected revision, and a write ID. Atomically compare the expected revision and update by book and profile. Return a new revision on acceptance, return the same result for the last accepted write ID if its response was lost, and return `409` with the current bookmark on conflict. Reject invalid input and stale-file anchors before changing progress. Keep the last accepted guarded write ID so legacy writes can be accepted only before the first guarded save. Bodyless opens continue working throughout rollout.
- Use browser storage scoped by server, profile, and book for the last confirmed position and the latest pending deliberate save. Use IndexedDB transactions for pending-record updates so tabs cannot erase each other's newer unsynced work. Reconcile before replaying a pending save. Until the current server state is known, allow reading and retain the local move. A save without a known base revision may proceed only against a confirmed empty bookmark. Once reading content is visible, a late response updates sync state and never moves the viewport.
- Persist each settled deliberate move locally, debounce foreground sync by 500 ms, serialize writes, and coalesce newer positions. Retry transient failures twice, then retry pending work on reconnect or reentry. Flush on page hide or Android background when possible. Keep the local pending record until acknowledged, superseded, or rejected as a conflict. A reopened reader can recover the saved position from local storage even when opening the book itself still requires access to its file.

## Verification

- Use the Node 22 test runner for location precedence and fallback, legacy conversion, navigation boundaries, state transitions, progress mapping, and save ordering. Add a `test:reader` script and run it in the frontend quality gate. Extend NUnit coverage for migration compatibility, checksum handling, endpoint validation, and profile isolation. Run frontend lint and build, backend build and tests, Swagger and client generation, and the Android build.
- Use the browser and device matrix to check nested TOCs, image and long-content layout, empty and malformed files, textless PDFs, keyboard focus, swipe cancellation, rapid section changes, resize, font changes, and reduced motion. Check that imported content renders without executing active content or requesting remote resources.
- Start from an existing saved row and verify that it resumes near the original location. Save a new anchor and verify it across a different layout. Confirm that a deep-link-only visit preserves the bookmark and an older tab cannot replace a newer save.
- Test files whose section numbering changes with both old and new links. Check offline navigation followed by restart and reconnect, both with and without an intervening save from another device. Fail the initial progress request and verify that reading continues and a late response neither jumps nor saves the opening page.
- Verify old-client opens and writes before the first revisioned save, then verify that old-client opens still work and unversioned writes are rejected. Test first-time offline sync with and without an existing bookmark. Confirm that an offline conflict shows one notice, leaves the current page in place, and restores the server bookmark on the next open. Open the same anchored link under different layouts, including an illustration-only page.
- Replace a file using the same filename and verify the checksum changes, a stale download cannot be labeled with the old checksum, and old anchors resolve through their numeric fallback. Test failed local storage and failed network together, a lost save response followed by retry, and an unresolved TOC parent with working children.

## Resume here

### Stage 1 baseline

Start the frontend dev server and open `/tests/reader/legacy.html` to check and download the five synthetic [fixture files](../frontend/tests/reader/fixtures.ts). The fixture page records the legacy section numbers and table-of-contents destinations. Chromium passed all five parser checks. The Node reader tests, targeted lint, and frontend production build also passed. The frontend quality gate now runs `npm run test:reader` for pure reader tests.

| Format | Legacy section numbering to preserve |
| --- | --- |
| EPUB | Follow spine order. Split large source entries into consecutive chunks. A TOC target uses the first chunk of its source entry, even when a fragment is present. The fixture has two chunks for the first entry and a packaged illustration at legacy section 2. |
| FB2 | Use Foliate's converted body order. Top-level sections in the first body precede additional bodies such as notes. Nested TOC entries point into their containing source section. |
| TXT | Normalize newlines, group paragraphs using the legacy heading rules, then chunk long groups before the shared markup split. The fixture numbers its untitled preface, first chapter, continuation, and final chapter as 0 through 3. |
| PDF | Discard textless pages before assigning legacy indexes. Preserve original page identity separately for stable anchors. In the fixture, legacy section 1 corresponds to original page 3. An entirely textless document produces a single notice section. |

The legacy splitter packs rendered markup within a 50,000-character budget. It drops empty chunks and numbers the remainder in order. A displayed page index is layout-dependent and carries no stable source offset.

### Reader regression reproductions

- **Duplicate parsing.** Open a valid fixture with a breakpoint at `ParseBook` in `useReadBook.tsx`. After its first invocation, `isLoading` changes to `false` and the parsing effect runs again with the same file.
- **Out-of-bounds navigation.** On the last page of the final section, press `ArrowRight`. The keyboard listener bypasses the disabled page button and advances the section past the end. Also try `?section=1.5`, which passes the numeric query parser without identifying a section.
- **Boundary transitions.** In a narrow viewport, swipe backward from the first page of a section into a multi-page predecessor. Repeat during a resize or while alternating between next and previous. The acceptance result is the predecessor's last real page with working controls after the transition. This interaction still needs a device run.
- **Stale progress.** Use two tabs with the same profile and book. Open the first tab at an earlier page, advance and leave the second tab, then leave the first tab without deliberately navigating. The old reader sends that tab's position during cleanup and the server accepts it. Check the requests and reopen the book. This scenario still needs a run against a live API.

### Stage 2 attachment identity

- New uploads use revision-specific paths and retain their original download names. The same-name replacement test reproduced the old overwrite and now verifies that each revision's checksum matches its bytes.
- The rewritten reader can request `GET /books/{bookId}/file?includeChecksum=true` and read `X-Book-File-Sha256`. The endpoint verifies legacy streams and conditionally repairs their metadata. The existing reader will start using the checksum when the new loader is connected.
- The backend build, 19 storage and controller tests, Swagger and client generation, frontend build, and Node reader tests passed. The three integration cases for conditional database repair compiled, but Docker was unavailable, so their PostgreSQL fixture could not start.

### Next work

Finish stage 2 with the additive bookmark migration, current-revision checks, and legacy-client compatibility. Rerun the PostgreSQL checksum repair tests when Docker is available. Carry forward the fixture expectations for parser changes and wire the checksum download into the new reader loader. The full browser and Android matrix remains a cutover check. Update this section with completed stages, verification results, and the next unfinished stage as implementation proceeds.
