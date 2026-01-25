import { SortDirection } from "@mui/material";
import { BookSortByDTO, SortDirectionDTO } from "../lib/api/KapitelShelf.Api";

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

export const ToSortDirectionDTO = (value: SortDirection | null | undefined) => {
  switch (value) {
    case "desc":
      return SortDirectionDTO.NUMBER_1;

    case "asc":
    default:
      return SortDirectionDTO.NUMBER_0;
  }
};
