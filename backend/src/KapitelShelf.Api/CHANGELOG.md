# Changelog

## [0.2.1](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.2.0...api@0.2.1) (2025-06-11)

### Features

- delete series ([#140](https://github.com/ThomasMiller01/KapitelShelf/issues/140)) ([3888f74](https://github.com/ThomasMiller01/KapitelShelf/commit/3888f7431190800276ce58413b3899a6cc11b5e8))
- edit books ([#147](https://github.com/ThomasMiller01/KapitelShelf/issues/147)) ([e1e6228](https://github.com/ThomasMiller01/KapitelShelf/commit/e1e62286f74bd35435fb00b49d2b3fb2202bc898))
- edit series ([#148](https://github.com/ThomasMiller01/KapitelShelf/issues/148)) ([4352cc1](https://github.com/ThomasMiller01/KapitelShelf/commit/4352cc1e60f039e52c66d62b0e2b56a74bbc351c))
- import book metadata from OpenLibrary ([#189](https://github.com/ThomasMiller01/KapitelShelf/issues/189)) ([5398492](https://github.com/ThomasMiller01/KapitelShelf/commit/5398492e307f1534224c58fd1a8242f9d5a1ca0c))
- import books in bulk from .csv ([#179](https://github.com/ThomasMiller01/KapitelShelf/issues/179)) ([eca6ac6](https://github.com/ThomasMiller01/KapitelShelf/commit/eca6ac6d910eb0d47da12b04f53c0a4008fce2c0))
- import metadata from amazon ([#196](https://github.com/ThomasMiller01/KapitelShelf/issues/196)) ([2c64ff2](https://github.com/ThomasMiller01/KapitelShelf/commit/2c64ff274392171fad965a9ce24da3a6e97cd0c7))
- import metadata from google ([#198](https://github.com/ThomasMiller01/KapitelShelf/issues/198)) ([4bf456e](https://github.com/ThomasMiller01/KapitelShelf/commit/4bf456e2c7c8f99e73d805d229ba290bd0ffcc8f))
- show volumes count for series on library page ([#150](https://github.com/ThomasMiller01/KapitelShelf/issues/150)) ([50bc2af](https://github.com/ThomasMiller01/KapitelShelf/commit/50bc2af6eb95561d2a275c524ed198a8c3549ddc))

### Bug Fixes

- ignore yourself during edit book duplicate check ([#151](https://github.com/ThomasMiller01/KapitelShelf/issues/151)) ([52453fa](https://github.com/ThomasMiller01/KapitelShelf/commit/52453fa3ef744b31f82e94fd624ed68dd1b5708b))
- import multiple files in sequence to prevent duplicate key errors ([#155](https://github.com/ThomasMiller01/KapitelShelf/issues/155)) ([a98e241](https://github.com/ThomasMiller01/KapitelShelf/commit/a98e2410842498324c81a576ec0e3c1a5eb8a3bb))
- upload file during single book import ([#200](https://github.com/ThomasMiller01/KapitelShelf/issues/200)) ([d2da4a5](https://github.com/ThomasMiller01/KapitelShelf/commit/d2da4a568c5bdf97ec03c6af5520ffac4473fc80))

## [0.2.0](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.1.1...api@0.2.0) (2025-05-09)

### ⚠ BREAKING CHANGES

- load cover images via Api and not reacts public directory (#110)

### Features

- import book from .fb2 file ([#126](https://github.com/ThomasMiller01/KapitelShelf/issues/126)) ([1551970](https://github.com/ThomasMiller01/KapitelShelf/commit/15519701928d80c4f2f9cd7f8e3f1e2cccd92e6f))
- import book from .pdf file ([#125](https://github.com/ThomasMiller01/KapitelShelf/issues/125)) ([aafb441](https://github.com/ThomasMiller01/KapitelShelf/commit/aafb4411530f4c40a841ab901c4d128eec4f9522))
- import book from doc/docx/odt/rtf ([#130](https://github.com/ThomasMiller01/KapitelShelf/issues/130)) ([c4ee296](https://github.com/ThomasMiller01/KapitelShelf/commit/c4ee2964bc0682209d32015488982f7aefc2050d))
- import book from file ([#124](https://github.com/ThomasMiller01/KapitelShelf/issues/124)) ([f92fe81](https://github.com/ThomasMiller01/KapitelShelf/commit/f92fe811d714b10d54971014bca2ecba2cfb0e1f))
- import book from text files ([#128](https://github.com/ThomasMiller01/KapitelShelf/issues/128)) ([7934f4e](https://github.com/ThomasMiller01/KapitelShelf/commit/7934f4e291385ed5ce19a7a7f0e199f2b448f657))
- load cover images via Api and not reacts public directory ([#110](https://github.com/ThomasMiller01/KapitelShelf/issues/110)) ([4eb38e6](https://github.com/ThomasMiller01/KapitelShelf/commit/4eb38e634f0a88a9ff41c8ad7b83c8aee0cf13ea))
- upload book files ([5628dd3](https://github.com/ThomasMiller01/KapitelShelf/commit/5628dd32c870533fbc53849ff4fdb23defa7a7c2))

## 0.1.1 (2025-05-04)

### Features

- add "create book" endpoint ([#54](https://github.com/ThomasMiller01/KapitelShelf/issues/54)) ([f727bd5](https://github.com/ThomasMiller01/KapitelShelf/commit/f727bd52e6679908f4b49ec3259909be55f26eb7))
- check for existing series, author, categories and tags when creating a book ([#72](https://github.com/ThomasMiller01/KapitelShelf/issues/72)) ([fec22bf](https://github.com/ThomasMiller01/KapitelShelf/commit/fec22bf3de1d947b5028c71ccac2c853b3d62c2a))
- support "Upload Cover" ([#76](https://github.com/ThomasMiller01/KapitelShelf/issues/76)) ([b886367](https://github.com/ThomasMiller01/KapitelShelf/commit/b88636777bad94acb48877d7d2417ad2e28fe9f7))

## 0.1.0 (2025-04-26)

- Initial Setup
