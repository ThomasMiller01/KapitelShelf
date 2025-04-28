module.exports = {
  header: "# Changelog\n",
  tagPrefix: "helm@",
  path: ".",
  packageFiles: "Chart.yaml",
  bumpFiles: "Chart.yaml",
  scripts: {
    postchangelog: "cd ../../ && npm run gen:helm:docs",
    precommit: "git add README.md",
  },
};
