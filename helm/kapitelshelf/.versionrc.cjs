module.exports = {
  header: "# Changelog\n",
  tagPrefix: "helm@",
  path: ".",
  packageFiles: "Chart.yaml",
  bumpFiles: "Chart.yaml",
  script: {
    prerelease: "git add README.md",
  },
};
