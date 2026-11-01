const { git } = require("./release-lib.cjs");

try {
  switch (process.argv[2]) {
    case "check":
      if (git("status", "--porcelain")) {
        throw new Error("Commit or stash your changes before preparing a release.");
      } 
      if (git("rev-parse", "--is-shallow-repository") === "true") {
        throw new Error("Fetch the full Git history and tags before preparing a release.");
      } 
      break;

    default:
      throw new Error("Expected check");
  }
} catch (error) {
  console.error(error.message);
  process.exitCode = 1;
}
