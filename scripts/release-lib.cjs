const { execFileSync } = require("node:child_process");
const YAML = require("yaml");

const components = [
  { name: "Frontend", path: "frontend", legacyTag: "frontend@0.3.4" },
  { name: "Backend", path: "backend", legacyTag: "api@0.3.5" },
  { name: "Helm", path: "helm", legacyTag: "helm@0.3.5" },
];

function git(...args) {
  return execFileSync("git", args, { encoding: "utf8", maxBuffer: 16 * 1024 * 1024 }).trim();
}

function pendingCommits(base) {
  const commits = new Map();
  for (const component of components) {
    const from = base || component.legacyTag;
    
    // Fail on missing history instead of accidentally regenerating every release.
    git("rev-parse", "--verify", `${from}^{commit}`);

    const hashes = git("log", "--no-merges", "--format=%H", `${from}..HEAD`, "--", component.path);
    for (const hash of hashes.split("\n").filter(Boolean)) {
      if (!commits.has(hash)) {
        commits.set(hash, { hash, components: [] });
      }

      commits.get(hash).components.push(component.name);
    }
  }
  return [...commits.values()];
}

function extractNotes(changelog, version) {
  const sections = changelog.replace(/\r\n/g, "\n").split(/(?=^## )/m);
  const section = sections.find((text) => text.startsWith(`## [${version}]`) ||
    text.startsWith(`## ${version} (`) || text.startsWith(`## ${version}\n`));
  if (!section) throw new Error(`CHANGELOG.md has no unified release entry for ${version}`);
  return section.trim() + "\n";
}

function helmImageTagNodes(contents) {
  const document = YAML.parseDocument(contents);
  if (document.errors.length) throw new Error(`Invalid Helm values: ${document.errors[0].message}`);
  return ["frontend", "api"].map((component) => {
    const node = document.getIn([component, "image", "tag"], true);
    if (!YAML.isScalar(node) || !node.range || node.anchor ||
        [YAML.Scalar.BLOCK_FOLDED, YAML.Scalar.BLOCK_LITERAL].includes(node.type)) {
      throw new Error(`Helm ${component}.image.tag must be a plain or quoted, unanchored scalar`);
    }
    return node;
  });
}

function readHelmImageTags(contents) {
  return helmImageTagNodes(contents).map((node) => String(node.value));
}

function updateHelmImageTags(contents, version) {
  // Replace only the two scalar values, retaining comments, formatting, and unrelated settings.
  const nodes = helmImageTagNodes(contents).sort((a, b) => b.range[0] - a.range[0]);
  for (const node of nodes) {
    const value = node.type === YAML.Scalar.QUOTE_SINGLE ? `'${version}'`
      : node.type === YAML.Scalar.PLAIN ? version : JSON.stringify(version);
    contents = contents.slice(0, node.range[0]) + value + contents.slice(node.range[1]);
  }
  return contents;
}

function textUpdater(pattern, writeVersion) {
  return {
    readVersion: (contents) => {
      const match = contents.match(pattern);
      if (!match) throw new Error(`Cannot find version matching ${pattern}`);
      return match[1];
    },
    writeVersion,
  };
}

module.exports = { git, pendingCommits, extractNotes, readHelmImageTags, updateHelmImageTags, textUpdater };
