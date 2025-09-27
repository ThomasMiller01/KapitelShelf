module.exports = {
  header: "# Changelog\n",
  tagPrefix: "helm@",
  releaseCommitMessageFormat: "chore(release-helm): v{{currentTag}}",
  path: ".",
  packageFiles: "Chart.yaml",
  bumpFiles: [
    { filename: "./Chart.yaml", type: "yaml" },
    {
      filename: "../../README.md",
      updater: {
        readVersion: (contents) =>
          contents.match(/helm-v(\d+\.\d+\.\d+)/)?.[1] ?? "0.0.0",
        writeVersion: (contents, version) =>
          contents.replace(/helm-v\d+\.\d+\.\d+/g, `helm-v${version}`),
      },
    },
  ],
  scripts: {
    postchangelog: "cd ../../ && npm run gen:helm:docs",
    precommit: "git add README.md",
  },
};
