import { SortDirection } from "@mui/material";
import {
  BookSortByDTO,
  SeriesSortByDTO,
  SortDirectionDTO,
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

export const ToSortDirectionDTO = (value: SortDirection | null | undefined) => {
  switch (value) {
    case "desc":
      return SortDirectionDTO.NUMBER_1;

    case "asc":
    default:
      return SortDirectionDTO.NUMBER_0;
  }
};
