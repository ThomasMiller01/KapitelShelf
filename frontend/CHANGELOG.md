# Changelog

## [0.2.2](https://github.com/ThomasMiller01/KapitelShelf/compare/frontend@0.2.1...frontend@0.2.2) (2025-06-11)


### Bug Fixes

* frontend build error ([c3b8884](https://github.com/ThomasMiller01/KapitelShelf/commit/c3b88842d0f2b8f6c631c02a7ca68867c786255b))

## [0.2.1](https://github.com/ThomasMiller01/KapitelShelf/compare/frontend@0.2.0...frontend@0.2.1) (2025-06-11)

### Features

- delete book ([#139](https://github.com/ThomasMiller01/KapitelShelf/issues/139)) ([33e9f39](https://github.com/ThomasMiller01/KapitelShelf/commit/33e9f3977915c9bd6ce65497c8e96e86726ba191))
- delete series ([#140](https://github.com/ThomasMiller01/KapitelShelf/issues/140)) ([3888f74](https://github.com/ThomasMiller01/KapitelShelf/commit/3888f7431190800276ce58413b3899a6cc11b5e8))
- edit books ([#147](https://github.com/ThomasMiller01/KapitelShelf/issues/147)) ([e1e6228](https://github.com/ThomasMiller01/KapitelShelf/commit/e1e62286f74bd35435fb00b49d2b3fb2202bc898))
- edit series ([#148](https://github.com/ThomasMiller01/KapitelShelf/issues/148)) ([4352cc1](https://github.com/ThomasMiller01/KapitelShelf/commit/4352cc1e60f039e52c66d62b0e2b56a74bbc351c))
- import book metadata from OpenLibrary ([#189](https://github.com/ThomasMiller01/KapitelShelf/issues/189)) ([5398492](https://github.com/ThomasMiller01/KapitelShelf/commit/5398492e307f1534224c58fd1a8242f9d5a1ca0c))
- import books in bulk from .csv ([#179](https://github.com/ThomasMiller01/KapitelShelf/issues/179)) ([eca6ac6](https://github.com/ThomasMiller01/KapitelShelf/commit/eca6ac6d910eb0d47da12b04f53c0a4008fce2c0))
- import metadata from amazon ([#196](https://github.com/ThomasMiller01/KapitelShelf/issues/196)) ([2c64ff2](https://github.com/ThomasMiller01/KapitelShelf/commit/2c64ff274392171fad965a9ce24da3a6e97cd0c7))
- import metadata from google ([#198](https://github.com/ThomasMiller01/KapitelShelf/issues/198)) ([4bf456e](https://github.com/ThomasMiller01/KapitelShelf/commit/4bf456e2c7c8f99e73d805d229ba290bd0ffcc8f))
- show volumes count for series on library page ([#150](https://github.com/ThomasMiller01/KapitelShelf/issues/150)) ([50bc2af](https://github.com/ThomasMiller01/KapitelShelf/commit/50bc2af6eb95561d2a275c524ed198a8c3549ddc))

### Bug Fixes

- book card metadata box not full width ([#182](https://github.com/ThomasMiller01/KapitelShelf/issues/182)) ([77f8c0c](https://github.com/ThomasMiller01/KapitelShelf/commit/77f8c0c7c5fbe39ee6ea6046428d8426b86c8127))
- dont display a read/download button, if no book file is uploaded ([#184](https://github.com/ThomasMiller01/KapitelShelf/issues/184)) ([9c1d462](https://github.com/ThomasMiller01/KapitelShelf/commit/9c1d46289125829dda7109c43b1c49bc4df1ec9e))
- dont show volume number in metadata cover image ([#199](https://github.com/ThomasMiller01/KapitelShelf/issues/199)) ([62ac626](https://github.com/ThomasMiller01/KapitelShelf/commit/62ac626bb4c8cfbc0f1b74dc67ba39d47f987727))
- hide "pages" on book card, if page number is 0 or not set ([#173](https://github.com/ThomasMiller01/KapitelShelf/issues/173)) ([5f29808](https://github.com/ThomasMiller01/KapitelShelf/commit/5f298089df26bd4a4b2c9dec262b1eb0fb9f6a5e))
- import multiple files in sequence to prevent duplicate key errors ([#155](https://github.com/ThomasMiller01/KapitelShelf/issues/155)) ([a98e241](https://github.com/ThomasMiller01/KapitelShelf/commit/a98e2410842498324c81a576ec0e3c1a5eb8a3bb))
- set url for location type kindle, skoobe and onleihe when editing a book ([#202](https://github.com/ThomasMiller01/KapitelShelf/issues/202)) ([ef4372c](https://github.com/ThomasMiller01/KapitelShelf/commit/ef4372c998fe5840ba90a83c123aa1b828990f8c))

## [0.2.0](https://github.com/ThomasMiller01/KapitelShelf/compare/frontend@0.1.2...frontend@0.2.0) (2025-05-09)

### ⚠ BREAKING CHANGES

- load cover images via Api and not reacts public directory (#110)

### Features

- import book from .fb2 file ([#126](https://github.com/ThomasMiller01/KapitelShelf/issues/126)) ([1551970](https://github.com/ThomasMiller01/KapitelShelf/commit/15519701928d80c4f2f9cd7f8e3f1e2cccd92e6f))
- import book from .pdf file ([#125](https://github.com/ThomasMiller01/KapitelShelf/issues/125)) ([aafb441](https://github.com/ThomasMiller01/KapitelShelf/commit/aafb4411530f4c40a841ab901c4d128eec4f9522))
- import book from doc/docx/odt/rtf ([#130](https://github.com/ThomasMiller01/KapitelShelf/issues/130)) ([c4ee296](https://github.com/ThomasMiller01/KapitelShelf/commit/c4ee2964bc0682209d32015488982f7aefc2050d))
- import book from file ([#124](https://github.com/ThomasMiller01/KapitelShelf/issues/124)) ([f92fe81](https://github.com/ThomasMiller01/KapitelShelf/commit/f92fe811d714b10d54971014bca2ecba2cfb0e1f))
- import book from text files ([#128](https://github.com/ThomasMiller01/KapitelShelf/issues/128)) ([7934f4e](https://github.com/ThomasMiller01/KapitelShelf/commit/7934f4e291385ed5ce19a7a7f0e199f2b448f657))
- import multiples files at once ([#134](https://github.com/ThomasMiller01/KapitelShelf/issues/134)) ([3637c4d](https://github.com/ThomasMiller01/KapitelShelf/commit/3637c4dd7860d370201e499af7f5174683a21025))
- improved general visibility of volume number on book cards ([#104](https://github.com/ThomasMiller01/KapitelShelf/issues/104)) ([c24b750](https://github.com/ThomasMiller01/KapitelShelf/commit/c24b750de3659499e899411da8999a83d05126c1))
- improved sizing of book/series cards on different view widths ([#105](https://github.com/ThomasMiller01/KapitelShelf/issues/105)) ([95c4b4d](https://github.com/ThomasMiller01/KapitelShelf/commit/95c4b4df5b7afea046e54b2482d5546f0a7a04b1))
- load cover images via Api and not reacts public directory ([#110](https://github.com/ThomasMiller01/KapitelShelf/issues/110)) ([4eb38e6](https://github.com/ThomasMiller01/KapitelShelf/commit/4eb38e634f0a88a9ff41c8ad7b83c8aee0cf13ea))
- Notification to navigate to book after creation ([#102](https://github.com/ThomasMiller01/KapitelShelf/issues/102)) ([dd90d73](https://github.com/ThomasMiller01/KapitelShelf/commit/dd90d732cd3dbd9a2904d6ae522ea15280935f28))
- upload book files ([5628dd3](https://github.com/ThomasMiller01/KapitelShelf/commit/5628dd32c870533fbc53849ff4fdb23defa7a7c2))

### Bug Fixes

- corrected "Category" label on "Create Book" page ([#113](https://github.com/ThomasMiller01/KapitelShelf/issues/113)) ([b2747aa](https://github.com/ThomasMiller01/KapitelShelf/commit/b2747aa9ac21ed13a0c11824f667109b8db0a153))

## 0.1.2 (2025-05-04)

### Features

- "Create Book" view ([#67](https://github.com/ThomasMiller01/KapitelShelf/issues/67)) ([6d80f84](https://github.com/ThomasMiller01/KapitelShelf/commit/6d80f8460fff46eb77f1290269d4b408a3a95133))
- "Not Implemented" notification ([#60](https://github.com/ThomasMiller01/KapitelShelf/issues/60)) ([5fc176c](https://github.com/ThomasMiller01/KapitelShelf/commit/5fc176cff9b2d5bc5aac961d8b284a8bd4d3aa97))
- add notifications via notistack ([#45](https://github.com/ThomasMiller01/KapitelShelf/issues/45)) ([11b10fe](https://github.com/ThomasMiller01/KapitelShelf/commit/11b10fe02566fc6a6b7804cca2816cecff35582b))
- support "Upload Cover" ([#76](https://github.com/ThomasMiller01/KapitelShelf/issues/76)) ([b886367](https://github.com/ThomasMiller01/KapitelShelf/commit/b88636777bad94acb48877d7d2417ad2e28fe9f7))
- validation for "Create Book" page ([#74](https://github.com/ThomasMiller01/KapitelShelf/issues/74)) ([ebd0144](https://github.com/ThomasMiller01/KapitelShelf/commit/ebd0144341a98c53ef3a062e2ebc6d2758ac5547))

## 0.1.1 (2025-04-28)

### Features

- Load environment variables at runtime ([#33](https://github.com/ThomasMiller01/KapitelShelf/issues/33)) ([d4dc387](https://github.com/ThomasMiller01/KapitelShelf/commit/d4dc387497a3fee5de735120a9607539a81aaa03))

## 0.1.0 (2025-04-26)

### Features

- Initial Setup
