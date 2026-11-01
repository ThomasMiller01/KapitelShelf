const conventionalCommits = require("conventional-changelog-conventionalcommits");
const { pendingCommits } = require("./release-lib.cjs");

module.exports = async (options) => {
  const preset = await conventionalCommits(options);
  const transform = preset.writerOpts.transform;
  
  let components;
  preset.writerOpts.transform = (commit, context) => {
    components ||= new Map(pendingCommits(context.gitSemverTags[0]).map((entry) => [entry.hash, entry.components]));
    
    const labels = components.get(commit.hash);
    if (!labels) {
      return;
    } 

    if (!commit.type) {
      console.warn(`Review unclassified change ${commit.hash.slice(0, 7)}: ${commit.header}`);
    }

    const result = transform(commit, context);
    if (result) {
        result.scope = labels.join(", ");
    }     
    return result;
  };

  preset.writerOpts.commitPartial = preset.writerOpts.commitPartial.replace("**{{scope}}:**", "[{{scope}}]");
  preset.writerOpts.mainTemplate = preset.writerOpts.mainTemplate.replace("**{{commit.scope}}:**", "[{{commit.scope}}]");
  return preset;
};
