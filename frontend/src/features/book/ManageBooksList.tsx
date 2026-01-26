import { GridColDef } from "@mui/x-data-grid";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import {
  LinkColumn,
  ManageItemsTable,
} from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { BookDTO } from "../../lib/api/KapitelShelf.Api";
import { useBooksList } from "../../lib/requests/books/useBooksList";
import { useDeleteBooks } from "../../lib/requests/books/useDeleteBooks";
import { LocationTypeToString } from "../../utils/LocationUtils";
import { FormatTime } from "../../utils/TimeUtils";

const formatList = (
  values: Array<{ name?: string | null }> | null | undefined,
  max: number,
): string => {
  const items = (values ?? [])
    .map((x) => x.name?.trim())
    .filter((x): x is string => Boolean(x));

  if (items.length === 0) {
    return "-";
  }

  const head = items.slice(0, max).join(", ");
  return items.length > max ? `${head} +${items.length - max}` : head;
};

export const columns: GridColDef<BookDTO>[] = [
  {
    field: "title",
    headerName: "Title",
    flex: 1,
    minWidth: 260,
    sortable: true,
    editable: true,
    valueGetter: (_, row) => row.title ?? null,
    valueFormatter: (value) => value ?? "-",
  },
  {
    field: "author",
    headerName: "Author",
    width: 150,
    sortable: true,
    editable: true,
    valueGetter: (_, row) => {
      {
        const first = row.author?.firstName?.trim() ?? "";
        const last = row.author?.lastName?.trim() ?? "";
        const full = `${first} ${last}`.trim();

        return full.length > 0 ? full : null;
      }
    },
    valueFormatter: (value) => value ?? "-",
    valueSetter: (value, row) => {
      if (value === null) {
        return {
          ...row,
          author: undefined,
        };
      }

      const parts = value?.split(" ").filter(Boolean);
      return {
        ...row,
        author: {
          firstName: parts.slice(0, -1).join(" "),
          lastName: parts[parts.length - 1] ?? "",
        },
      };
    },
    // TODO edit with autocomplete
  },
  {
    field: "series",
    headerName: "Series",
    width: 200,
    sortable: true,
    editable: true,
    valueGetter: (_, row) => row.series?.name ?? null,
    valueFormatter: (value) => value ?? "-",
    valueSetter: (value, row) => {
      return {
        ...row,
        series:
          value.length > 0
            ? ({ ...row.series, name: value } as any)
            : undefined,
      };
    },
    // TODO edit with autocomplete
  },
  {
    field: "seriesNumber",
    headerName: "Volume",
    type: "number",
    width: 80,
    sortable: true,
    editable: true,
    align: "center",
    valueFormatter: (value) => (value === 0 ? "-" : value ?? "-"),
  },
  {
    field: "pageNumber",
    headerName: "Pages",
    type: "number",
    width: 80,
    sortable: true,
    editable: true,
    align: "right",
    headerAlign: "right",
    valueGetter: (_, row) => row.pageNumber ?? null,
    valueFormatter: (value) => value ?? "-",
  },
  {
    field: "releaseDate",
    headerName: "Release",
    align: "center",
    width: 110,
    type: "date",
    sortable: true,
    editable: true,
    valueGetter: (_, row) => {
      if (!row.releaseDate) {
        return null;
      }

      const d = new Date(row.releaseDate);
      return Number.isNaN(d.getTime()) ? null : d;
    },
    valueFormatter: (value) => FormatTime(value, "date"),
  },
  {
    field: "categories",
    headerName: "Categories",
    width: 220,
    sortable: false,
    editable: true,
    valueGetter: (_, row) => row.categories ?? null,
    valueFormatter: (value) => formatList(value, 3),
    // TODO edit category
  },
  {
    field: "tags",
    headerName: "Tags",
    width: 220,
    sortable: false,
    editable: true,
    valueGetter: (_, row) => row.tags ?? null,
    valueFormatter: (value) => formatList(value, 3),
    // TODO edit tags
  },
  {
    field: "location",
    headerName: "Location",
    width: 150,
    sortable: false,
    editable: false,
    valueGetter: (_, row) =>
      row.location?.type === undefined
        ? "-"
        : LocationTypeToString[row.location.type],
  },
  LinkColumn((params) => `/library/books/${params.row.id}`),
];

export const ManageBooksList = () => {
  const { page, pageSize, sorting, filter, setItemsTableParams } =
    useItemsTableParams({
      defaultPageSize: 15,
    });

  const { data, isLoading, isError, refetch } = useBooksList({
    page,
    pageSize,
    sorting,
    filter,
  });

  const { mutate: deleteBooks } = useDeleteBooks();

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Books" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="books" onRetry={refetch} />;
  }

  return (
    <ManageItemsTable
      settingKey="manage-books"
      // data
      items={data?.items ?? []}
      isLoading={isLoading}
      totalItems={data?.totalCount ?? 0}
      // layout
      columns={columns}
      itemName="book"
      // pagination & sorting
      page={page}
      pageSize={pageSize}
      sorting={sorting}
      filter={filter}
      setItemsTableParams={setItemsTableParams}
      // actions
      deleteAction={deleteBooks}
      // editing
      // editing
      onRowEdit={(updatedBook, _) => console.log("updated", updatedBook)}
    />
  );
};
