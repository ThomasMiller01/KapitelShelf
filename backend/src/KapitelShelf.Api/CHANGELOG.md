# Changelog

## [0.3.4](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.3.3...api@0.3.4) (2026-01-14)


### Features

* Add a notification system ([#346](https://github.com/ThomasMiller01/KapitelShelf/issues/346)) ([2e4d8da](https://github.com/ThomasMiller01/KapitelShelf/commit/2e4d8da6351ab3333c52426f762b128318edbb33))
* Improve watchlist scraping strategy ([#347](https://github.com/ThomasMiller01/KapitelShelf/issues/347)) ([b2dbf5b](https://github.com/ThomasMiller01/KapitelShelf/commit/b2dbf5ba13635a69c237ca88a7fea2ce7b16ad85))
* Add autocomplete when editing books for series, author, category and tag ([#353](https://github.com/ThomasMiller01/KapitelShelf/issues/353)) ([307cb33](https://github.com/ThomasMiller01/KapitelShelf/commit/307cb33c373cf8c8bc5b8ec59be555f261b1f882))

## [0.3.3](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.3.2...api@0.3.3) (2025-10-12)


### Bug Fixes

* fixed adding book from watchlist to library ([#343](https://github.com/ThomasMiller01/KapitelShelf/issues/343)) ([cc6c4dd](https://github.com/ThomasMiller01/KapitelShelf/commit/cc6c4dd680b5f692f1862b671cde1b2d2fb5b393))

## [0.3.2](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.3.1...api@0.3.2) (2025-10-09)


### Features

* add released book from watchlist to library ([#333](https://github.com/ThomasMiller01/KapitelShelf/issues/333)) ([9d152cb](https://github.com/ThomasMiller01/KapitelShelf/commit/9d152cbd4a85568bf7cc41a33fda9428973630ee))
* add watchlist to automatically check for new volumes of series ([#329](https://github.com/ThomasMiller01/KapitelShelf/issues/329)) ([75d146c](https://github.com/ThomasMiller01/KapitelShelf/commit/75d146c39a0d243901597b4085716c7b514f0446))
* added experimental setting to use "rclone bisync" instead of "rclone sync" ([#316](https://github.com/ThomasMiller01/KapitelShelf/issues/316)) ([999afda](https://github.com/ThomasMiller01/KapitelShelf/commit/999afda61d385120d52f6903644e0bf76b009f2d))
* Android apk build ([#305](https://github.com/ThomasMiller01/KapitelShelf/issues/305)) ([cb5ea75](https://github.com/ThomasMiller01/KapitelShelf/commit/cb5ea75a4a02d2cd2b5e07f0c3a400de737aa020))
* import book from amazon asin ([#324](https://github.com/ThomasMiller01/KapitelShelf/issues/324)) ([d2b063a](https://github.com/ThomasMiller01/KapitelShelf/commit/d2b063ac31da9c80688485613b14cdd879bcf810))
* improve watchlist algorithm to extract new kindle volumes ([#337](https://github.com/ThomasMiller01/KapitelShelf/issues/337)) ([a7cc346](https://github.com/ThomasMiller01/KapitelShelf/commit/a7cc346d3bcbaf1baf21df197f0fe405442514f8))
* replace AutoMapper ([#335](https://github.com/ThomasMiller01/KapitelShelf/issues/335)) ([3759f19](https://github.com/ThomasMiller01/KapitelShelf/commit/3759f19e83e89e355489f8ad8f41225df3929365))
* Sync User Settings across different devices ([#304](https://github.com/ThomasMiller01/KapitelShelf/issues/304)) ([1ad38b2](https://github.com/ThomasMiller01/KapitelShelf/commit/1ad38b262132ec773167d7a68ab6e46bb4875a2f))


### Bug Fixes

* cover images were not loaded on the book details page ([#318](https://github.com/ThomasMiller01/KapitelShelf/issues/318)) ([2d3f48d](https://github.com/ThomasMiller01/KapitelShelf/commit/2d3f48d538d47b92575b4267e77dbd00ddb76cca))

## [0.3.1](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.3.0...api@0.3.1) (2025-09-08)


### Bug Fixes

* set domain for cloudstorage configuration ([#301](https://github.com/ThomasMiller01/KapitelShelf/issues/301)) ([14adff3](https://github.com/ThomasMiller01/KapitelShelf/commit/14adff34ef30fe32b6c3eaa6e54a01fc980bf579))

## [0.3.0](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.2.1...api@0.3.0) (2025-09-06)


### ⚠ BREAKING CHANGES

* remove deprecated `/series/summary` endpoint (#286)

### Features

* add "Chico" profile image ([#268](https://github.com/ThomasMiller01/KapitelShelf/issues/268)) ([47a7254](https://github.com/ThomasMiller01/KapitelShelf/commit/47a725469b330992e004b394b7ec6864763fd191))
* add "Ellie" profile avatar ([#290](https://github.com/ThomasMiller01/KapitelShelf/issues/290)) ([289fd42](https://github.com/ThomasMiller01/KapitelShelf/commit/289fd42fe6586b6be241a69f23a943b72530ce3f))
* add "Le Chonky Boy" profile image ([#284](https://github.com/ThomasMiller01/KapitelShelf/issues/284)) ([ef39365](https://github.com/ThomasMiller01/KapitelShelf/commit/ef3936505fe925173b61918a0c2ae72bd58132f3))
* add "Little Stinky" profile image ([#282](https://github.com/ThomasMiller01/KapitelShelf/issues/282)) ([7c5f04b](https://github.com/ThomasMiller01/KapitelShelf/commit/7c5f04bddef61289debaaf81e7ef46adcec95533))
* add basic support for OneDrive cloud storages ([#274](https://github.com/ThomasMiller01/KapitelShelf/issues/274)) ([b0e4004](https://github.com/ThomasMiller01/KapitelShelf/commit/b0e4004b5ccfda12e6383744ce91f372197fd91d))
* add profile image "Riley" ([#262](https://github.com/ThomasMiller01/KapitelShelf/issues/262)) ([e2304e8](https://github.com/ThomasMiller01/KapitelShelf/commit/e2304e8b313b9ba3f2ad351938e6534415bd11f4))
* add profile images "Muggy Malheur", "Cheery Chino", "Sunny Tome", "FR E SH A VOCA DO" and "Tailywink" ([#260](https://github.com/ThomasMiller01/KapitelShelf/issues/260)) ([acbb14b](https://github.com/ThomasMiller01/KapitelShelf/commit/acbb14bcb0e97837936c6bb842de6d10458dfd08))
* add support for background tasks ([#264](https://github.com/ThomasMiller01/KapitelShelf/issues/264)) ([1d681ec](https://github.com/ThomasMiller01/KapitelShelf/commit/1d681ec54458de65a41e82cd4aa4d5dada003445))
* change user profile image and color ([#258](https://github.com/ThomasMiller01/KapitelShelf/issues/258)) ([befd5fb](https://github.com/ThomasMiller01/KapitelShelf/commit/befd5fbee8c997bc343d4db1bf3a957e2b08f73a))
* display last visited books on home page ([#253](https://github.com/ThomasMiller01/KapitelShelf/issues/253)) ([34a697c](https://github.com/ThomasMiller01/KapitelShelf/commit/34a697cec3b0fd4c68c977da58bcb1b54eaf8c4b))
* download/remove/sync cloud storages ([#276](https://github.com/ThomasMiller01/KapitelShelf/issues/276)) ([07b9d6a](https://github.com/ThomasMiller01/KapitelShelf/commit/07b9d6a17e48feb8e509273381323490a9299c56))
* manage user via api ([#245](https://github.com/ThomasMiller01/KapitelShelf/issues/245)) ([321a789](https://github.com/ThomasMiller01/KapitelShelf/commit/321a78932189ed0c0e2c863f7b3e38304784f466))
* remove deprecated `/series/summary` endpoint ([#286](https://github.com/ThomasMiller01/KapitelShelf/issues/286)) ([e48d58a](https://github.com/ThomasMiller01/KapitelShelf/commit/e48d58acbc49cf824f23512bf7954c50cc1f62a5))
* scan cloud storages to import files ([#281](https://github.com/ThomasMiller01/KapitelShelf/issues/281)) ([9288c8d](https://github.com/ThomasMiller01/KapitelShelf/commit/9288c8d0bad20def63ed407dd4320b0fa47b15cb))


## [0.2.2](https://github.com/ThomasMiller01/KapitelShelf/compare/api@0.2.1...api@0.2.2) (2025-06-25)


### Features

* merge a series into another one ([#233](https://github.com/ThomasMiller01/KapitelShelf/issues/233)) ([3f8a315](https://github.com/ThomasMiller01/KapitelShelf/commit/3f8a315b4c5341879fde70b256af083d81355889))
* put migrator into api docker image to prevent mismatched api and migrator versions ([#227](https://github.com/ThomasMiller01/KapitelShelf/issues/227)) ([197e2e8](https://github.com/ThomasMiller01/KapitelShelf/commit/197e2e891dfc4cfd9be8965316715818479ff307))
* return https cover images for google metadata ([575ba70](https://github.com/ThomasMiller01/KapitelShelf/commit/575ba70b99fe8fdef26c9f0de648f64cebef5745))
* search-bar & search-results page ([#218](https://github.com/ThomasMiller01/KapitelShelf/issues/218)) ([2f2c500](https://github.com/ThomasMiller01/KapitelShelf/commit/2f2c500d7398dbb60ac15c75a8a85d7a81c62170))


### Bug Fixes

* amazon metadata source did not return any books ([#221](https://github.com/ThomasMiller01/KapitelShelf/issues/221)) ([fea0be9](https://github.com/ThomasMiller01/KapitelShelf/commit/fea0be9ccac9f089128f728286a42534ff658728))
* load metadata cover via proxy ([51604d4](https://github.com/ThomasMiller01/KapitelShelf/commit/51604d4d6d84266b67797b431e2b2641d2a3bc95))

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
