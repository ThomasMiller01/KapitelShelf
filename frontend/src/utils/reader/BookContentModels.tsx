export interface BookContent {
  metadata: BookContentMetadata;
  navigation: BookContentNavigation;
  sections: BookSection[];
}

export interface BookContentMetadata {
  title?: string;
  subtitle?: string;
  description?: string;
}

export interface BookContentNavigation {
  tableOfContents: BookTocItem[];
  pageCount?: number;
}

export interface BookTocItem {
  id: string;
  label: string;
  sectionIndex?: number;
  href?: string;
  children: BookTocItem[];
}

export interface BookSection {
  id: string;
  index: number;
  sourceSectionIndex?: number;
  title?: string;
  href?: string;
  contentType: BookSectionContentType;
  content: string;
  text?: string;
  sizeBytes?: number;
  locator?: BookSectionLocator;
}

export type BookSectionContentType =
  | "html"
  | "xhtml"
  | "text"
  | "pdf-page"
  | "binary";

export interface BookSectionLocator {
  locationHint?: string;
  progression?: number;
}
