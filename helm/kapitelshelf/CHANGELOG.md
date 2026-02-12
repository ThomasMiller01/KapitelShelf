# Changelog

## [0.3.4](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.3.3...helm@0.3.4) (2026-02-12)


### Features

* bump frontend and api images ([01c5cff](https://github.com/ThomasMiller01/KapitelShelf/commit/01c5cff0f48214ecff15545a736e4eb532e6d595))

## [0.3.3](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.3.2...helm@0.3.3) (2026-01-14)


### Features

* bump frontend and api images ([9eaa7ca](https://github.com/ThomasMiller01/KapitelShelf/commit/9eaa7caac6846f754298180ed6740243766b35a5))

## [0.3.2](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.3.1...helm@0.3.2) (2025-10-09)


### Features

* add frontend, api and helm version to readme ([#326](https://github.com/ThomasMiller01/KapitelShelf/issues/326)) ([6c3abd1](https://github.com/ThomasMiller01/KapitelShelf/commit/6c3abd110bdf333f60fc3e2e9ed3a07e6b0740e6))

## [0.3.1](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.3.0...helm@0.3.1) (2025-09-08)

### Bug Fixes

* set domain for cloudstorage configuration ([#301](https://github.com/ThomasMiller01/KapitelShelf/issues/301)) ([14adff3](https://github.com/ThomasMiller01/KapitelShelf/commit/14adff34ef30fe32b6c3eaa6e54a01fc980bf579))

## [0.3.0](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.2.2...helm@0.3.0) (2025-09-06)


### ⚠ BREAKING CHANGES

* bump frontend and api versions in values ([5545669](https://github.com/ThomasMiller01/KapitelShelf/commit/55456693013ae488a12288105cb925f6efc602c8))

## [0.2.2](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.2.1...helm@0.2.2) (2025-06-25)


### Features

* bump frontend and api images ([352c305](https://github.com/ThomasMiller01/KapitelShelf/commit/352c305de4266adb75ba96530eebe5caf980ff04))
* put migrator into api docker image to prevent mismatched api and migrator versions ([#227](https://github.com/ThomasMiller01/KapitelShelf/issues/227)) ([197e2e8](https://github.com/ThomasMiller01/KapitelShelf/commit/197e2e891dfc4cfd9be8965316715818479ff307))

## [0.2.1](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.2.0...helm@0.2.1) (2025-06-11)

### Features

* new kapitelshelf release ([#110](https://github.com/ThomasMiller01/KapitelShelf/issues/110)) ([7661a84](https://github.com/ThomasMiller01/KapitelShelf/commit/7661a847d2b5c512dac4ed39270b7b13ec48c5e4))

## [0.2.0](https://github.com/ThomasMiller01/KapitelShelf/compare/helm@0.1.5...helm@0.2.0) (2025-05-09)


### ⚠ BREAKING CHANGES

* load cover images via Api and not reacts public directory (#110)

### Features

* load cover images via Api and not reacts public directory ([#110](https://github.com/ThomasMiller01/KapitelShelf/issues/110)) ([4eb38e6](https://github.com/ThomasMiller01/KapitelShelf/commit/4eb38e634f0a88a9ff41c8ad7b83c8aee0cf13ea))


### Bug Fixes

* delete migrator job after install/upgrade to prevent helm updating errors ([#103](https://github.com/ThomasMiller01/KapitelShelf/issues/103)) ([b1d3424](https://github.com/ThomasMiller01/KapitelShelf/commit/b1d3424b31ec4e9e0f60768fd5361122dd15490d))

## 0.1.5 (2025-05-04)


### Bug Fixes

* add port to external-database host ([ff4ea01](https://github.com/ThomasMiller01/KapitelShelf/commit/ff4ea018c316b3c662ba323ad457c3e32ae292ba))

## 0.1.4 (2025-05-04)


### Bug Fixes

* inconsistent PostgreSQL naming ([#88](https://github.com/ThomasMiller01/KapitelShelf/issues/88)) ([064d167](https://github.com/ThomasMiller01/KapitelShelf/commit/064d167afbe959c4fc25b1d85562fe35d6436af8))

## 0.1.3 (2025-05-04)


### Features

* support "Upload Cover" ([#76](https://github.com/ThomasMiller01/KapitelShelf/issues/76)) ([b886367](https://github.com/ThomasMiller01/KapitelShelf/commit/b88636777bad94acb48877d7d2417ad2e28fe9f7))


## 0.1.2 (2025-04-29)


### Bug Fixes

* use lowercase chart name for name-template ([#39](https://github.com/ThomasMiller01/KapitelShelf/issues/39)) ([2c286f8](https://github.com/ThomasMiller01/KapitelShelf/commit/2c286f8555b95959ebecd3baeea521de9d379da7))

## 0.1.1 (2025-04-29)

### Documentation

* Fix helm repo url in docs

## 0.1.0 (2025-04-28)


### Features

* Initial Setup
