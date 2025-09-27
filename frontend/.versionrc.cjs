module.exports = {
  header: "# Changelog\n",
  tagPrefix: "frontend@",
  releaseCommitMessageFormat: "chore(release-frontend): v{{currentTag}}",
  path: ".",
  bumpFiles: [
    { filename: "./package.json", type: "json" },
    { filename: "./package-lock.json", type: "json" },
    {
      filename: "../README.md",
      updater: {
        readVersion: (contents) =>
          contents.match(/frontend-v(\d+\.\d+\.\d+)/)?.[1] ?? "0.0.0",
        writeVersion: (contents, version) =>
          contents.replace(/frontend-v\d+\.\d+\.\d+/g, `frontend-v${version}`),
      },
    },
  ],
};
