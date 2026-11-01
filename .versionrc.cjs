const { readHelmImageTags, updateHelmImageTags, textUpdater } = require("./scripts/release-lib.cjs");

module.exports = {
  header: "# Changelog\n",
  tagPrefix: "v",
  releaseCommitMessageFormat: "chore(release): v{{currentTag}}",
  preset: { name: require.resolve("./scripts/changelog-preset.cjs") },
  skip: { tag: true },
  commitAll: true,
  scripts: {
    prerelease: "node scripts/release.cjs check",
    postchangelog: "npm run gen:helm:docs",
    precommit: "git add helm/kapitelshelf/README.md",
  },
  packageFiles: [{ filename: "package.json", type: "json" }],
  bumpFiles: [
    { filename: "package.json", type: "json" },
    { filename: "package-lock.json", type: "json" },
    { filename: "frontend/package.json", type: "json" },
    { filename: "frontend/package-lock.json", type: "json" },
    { filename: "backend/src/KapitelShelf.Api/KapitelShelf.Api.csproj", type: "csproj" },
    {
      filename: "helm/kapitelshelf/Chart.yaml",
      updater: textUpdater(/^version: (\S+)/m, (contents, version) => contents
        .replace(/^version: .+$/m, `version: ${version}`)
        .replace(/^appVersion: .+$/m, `appVersion: "${version}"`)),
    },
    {
      filename: "helm/kapitelshelf/values.yaml",
      updater: {
        readVersion: (contents) => readHelmImageTags(contents)[0],
        writeVersion: updateHelmImageTags,
      },
    },
  ],
};
