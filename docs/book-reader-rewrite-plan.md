# Book Reader Simplification

**Status** · Implemented and verified on `411-pbi-rewrite-bookreader`.

The reader should keep the experience described in the [Book Reader guide](references/book-reader.md). The existing file download and per-profile section/page API are sufficient. Earlier bookmark revision and attachment checksum work is superseded.

## Implementation

1. Restore the backend and generated client contract from the pre-rewrite baseline (`c6ec92f`). Remove the branch-only bookmark migration from source after checking release history. Leave databases and existing stored attachments alone.
2. Keep the cancellable loader and typed parsers. Download through the existing client and remove checksum negotiation. Preserve legacy parser section numbering so stored bookmarks and links keep working.
3. Give section, page, and measurement updates to one reader hook. Route keyboard, tap, swipe, timeline, and table-of-contents actions through it. Render one measured section and let the visible page follow a horizontal drag until release. Keep existing reader settings and mobile controls.
4. Restore valid URL coordinates first, then the profile bookmark, then the beginning. Clamp when the target is measured. Save deliberate navigation through the existing API, with a brief debounce and best-effort exit handling. Leave bookmarks alone when opening links or repaginating. Page numbers are approximate across layouts, and different devices share the existing server save behavior.
5. Sanitize book markup before display while keeping readable content, packaged images, and links. Preserve the PDF text-extraction limitation and the existing web and Android experience.

## Open reader follow-ups

- [ ] Finish the swipe animation after release. The page should follow the finger during the drag, then continue smoothly from the release position until the destination page settles. Check forward and backward swipes within chapters and across chapter boundaries on Android. Verify that short and canceled drags return to the current page without saving a new bookmark.
- [ ] Center the text block on Android. The supplied portrait screenshot shows a wider left margin than right margin on page 516. Check the page padding and column offsets, then verify that the text block has even side margins on the first page and after several page turns. Check portrait and landscape and preserve paragraph alignment.

## Verification and resume point

- The backend builds. All 668 backend tests pass. The regenerated OpenAPI contract and client match the pre-rewrite baseline apart from the API assembly version in their headers. Release refs checked during implementation did not contain the removed migration. No database downgrade was performed.
- The frontend production build, lint on the changed reader source and fixtures, and all five `npm run test:reader` tests pass.
- Headless Chromium passed the [reader fixtures](../frontend/tests/reader/legacy.html) at `/tests/reader/legacy.html`. They exercise legacy section and TOC mapping in EPUB, FB2, TXT, and PDF, packaged images, textless PDF, load cancellation, safe markup, live drag, swipe release and cancellation, chapter boundaries, URL coordinates, resize, bookmark writes, and reopening.
- Native Android verification remains open. Check edge taps, swipes, appearance, and orientation lock on a device. Page numbers can still refer to different text when switching layouts, and saving during browser or app exit is best effort.
