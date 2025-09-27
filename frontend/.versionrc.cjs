module.exports = {
  header: "# Changelog\n",
  tagPrefix: "frontend@",
  path: ".",
  bumpFiles: [
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
