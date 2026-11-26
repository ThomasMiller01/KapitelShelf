import assert from "node:assert/strict";
import { test } from "node:test";

import { changePosition, createPosition, readLocation } from "./readerPosition.ts";

const measure = (state, section, pageCount) => changePosition(state, { type: "measure", section, pageCount });
const turn = (state, direction) => changePosition(state, { type: "turn", direction });

test("validates older numeric links and clamps them after measuring", () => {
  assert.equal(readLocation(new URLSearchParams("section=1.5&page=-2")), undefined);
  assert.deepEqual(readLocation(new URLSearchParams("page=0")), { section: 0, page: 0 });
  const initial = createPosition(3, readLocation(new URLSearchParams("section=8&page=9")));
  const settled = measure(initial, 2, 4);
  assert.equal(settled.section, 2);
  assert.equal(settled.page, 3);
  assert.equal(settled.lastMove, undefined);
  assert.strictEqual(turn(settled, 1), settled);
});

test("crosses section boundaries using the destination measurement", () => {
  const start = measure(createPosition(2, { section: 0, page: 0 }), 0, 2);
  assert.strictEqual(turn(start, -1), start);
  const intoNextSection = turn(turn(start, 1), 1);
  assert.strictEqual(measure(intoNextSection, 0, 2), intoNextSection);
  const next = measure(intoNextSection, 1, 3);
  assert.deepEqual(next.lastMove, { section: 1, page: 0 });
  const previous = measure(turn(next, -1), 0, 5);
  assert.deepEqual(previous.lastMove, { section: 0, page: 4 });
});

test("remeasuring and opening a link do not replace the deliberate save", () => {
  const original = measure(createPosition(2, { section: 0, page: 2 }), 0, 6);
  const move = turn(original, 1);
  const resized = measure(move, 0, 11);
  assert.equal(resized.page, 6);
  assert.strictEqual(resized.lastMove, move.lastMove);
  const reopened = changePosition(resized, { type: "open", location: { section: 1, page: 2 } });
  const settled = measure(reopened, 1, 8);
  assert.equal(settled.page, 2);
  assert.strictEqual(settled.lastMove, move.lastMove);
});
