module.exports = {
  header: "# Changelog\n",
  tagPrefix: "api@",
  releaseCommitMessageFormat: "chore(release-api): v{{currentTag}}",
  path: ".",
  packageFiles: "KapitelShelf.Api.csproj",
  bumpFiles: [
    { filename: "./KapitelShelf.Api.csproj", type: "csproj" },
    {
      filename: "../../../README.md",
      updater: {
        readVersion: (contents) =>
          contents.match(/api-v(\d+\.\d+\.\d+)/)?.[1] ?? "0.0.0",
        writeVersion: (contents, version) =>
          contents.replace(/api-v\d+\.\d+\.\d+/g, `api-v${version}`),
      },
    },
  ],
};
