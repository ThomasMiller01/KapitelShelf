import assert from "node:assert/strict";
import { test } from "node:test";

import { mapPage } from "./pageProgress.ts";

test("maps section pages to a stable estimated range and back", () => {
  assert.equal(mapPage(0, 20, 10), 0);
  assert.equal(mapPage(19, 20, 10), 9);
  assert.equal(mapPage(9, 10, 20), 19);
  assert.equal(mapPage(5, 10, 20), 11);
});

test("handles one-page sections and out-of-range locations", () => {
  assert.equal(mapPage(0, 1, 10), 0);
  assert.equal(mapPage(5, 10, 1), 0);
  assert.equal(mapPage(-1, 10, 20), 0);
  assert.equal(mapPage(20, 10, 20), 19);
});
