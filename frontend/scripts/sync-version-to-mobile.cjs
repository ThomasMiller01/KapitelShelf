const fs = require("fs");
const path = require("path");

// get version from package.json
const pkg = require("../package.json");
const versionName = pkg.version;

// compute versionCode for Android (e.g. 1.2.3 → 10203)
const versionCode = versionName
  .split(".")
  .reduce((acc, num, i) => acc + Number(num) * Math.pow(100, 2 - i), 0);

console.log(
  `⏳ Syncing versionName "${versionName}" and versionCode "${versionCode}"`
);

// ✅ Update Android build.gradle
const gradlePath = path.resolve(__dirname, "../android/app/build.gradle");
let gradleContent = fs.readFileSync(gradlePath, "utf-8");

gradleContent = gradleContent
  .replace(/versionCode \d+/, `versionCode ${versionCode}`)
  .replace(/versionName "[^"]+"/, `versionName "${versionName}"`);

fs.writeFileSync(gradlePath, gradleContent);
console.log("✅ Android build.gradle updated");
