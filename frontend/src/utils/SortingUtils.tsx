import { SortDirection } from "@mui/material";
import {
  AuthorSortByDTO,
  BookSortByDTO,
  CategorySortByDTO,
  SeriesSortByDTO,
  SortDirectionDTO,
  TagSortByDTO,
} from "../lib/api/KapitelShelf.Api";

export const ToBookSortByDTO = (value: string | null | undefined) => {
  switch (value) {
    case "title":
      return BookSortByDTO.NUMBER_1;

    case "author":
      return BookSortByDTO.NUMBER_2;

    case "series":
      return BookSortByDTO.NUMBER_3;

    case "seriesNumber":
      return BookSortByDTO.NUMBER_4;

    case "pageNumber":
      return BookSortByDTO.NUMBER_5;

    case "releaseDate":
      return BookSortByDTO.NUMBER_6;

    case "default":
    default:
      return BookSortByDTO.NUMBER_0;
  }
};

export const ToSeriesSortByDTO = (value: string | null | undefined) => {
  switch (value) {
    case "name":
      return SeriesSortByDTO.NUMBER_1;

    case "totalBooks":
      return SeriesSortByDTO.NUMBER_2;

    case "updatedAt":
      return SeriesSortByDTO.NUMBER_3;

    case "createdAt":
      return SeriesSortByDTO.NUMBER_4;

    case "default":
    default:
      return SeriesSortByDTO.NUMBER_0;
  }
};

export const ToAuthorsSortByDTO = (value: string | null | undefined) => {
  switch (value) {
    case "firstName":
      return AuthorSortByDTO.NUMBER_1;

    case "lastName":
      return AuthorSortByDTO.NUMBER_2;

    case "totalBooks":
      return AuthorSortByDTO.NUMBER_3;

    case "default":
    default:
      return SeriesSortByDTO.NUMBER_0;
  }
};

export const ToCategoriesSortByDTO = (value: string | null | undefined) => {
  switch (value) {
    case "name":
      return CategorySortByDTO.NUMBER_1;

    case "totalBooks":
      return CategorySortByDTO.NUMBER_2;

    case "default":
    default:
      return CategorySortByDTO.NUMBER_0;
  }
};

export const ToTagsSortByDTO = (value: string | null | undefined) => {
  switch (value) {
    case "name":
      return TagSortByDTO.NUMBER_1;

    case "totalBooks":
      return TagSortByDTO.NUMBER_2;

    case "default":
    default:
      return TagSortByDTO.NUMBER_0;
  }
};

export const ToSortDirectionDTO = (value: SortDirection | null | undefined) => {
  switch (value) {
    case "asc":
      return SortDirectionDTO.NUMBER_0;

    case "desc":
    default:
      return SortDirectionDTO.NUMBER_1;
  }
};
