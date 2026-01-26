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
    valueGetter: (_, row) => row.title ?? "-",
  },
  {
    field: "author",
    headerName: "Author",
    width: 150,
    sortable: true,
    valueGetter: (_, row) => {
      {
        const first = row.author?.firstName?.trim() ?? "";
        const last = row.author?.lastName?.trim() ?? "";
        const full = `${first} ${last}`.trim();

        return full.length > 0 ? full : "-";
      }
    },
  },
  {
    field: "series",
    headerName: "Series",
    width: 200,
    sortable: true,
    valueGetter: (_, row) => row.series?.name ?? "-",
  },
  {
    field: "seriesNumber",
    headerName: "Volume",
    width: 80,
    sortable: true,
    align: "center",
  },
  {
    field: "pageNumber",
    headerName: "Pages",
    type: "number",
    width: 80,
    sortable: true,
    align: "right",
    headerAlign: "right",
    valueGetter: (_, row) => row.pageNumber ?? "-",
  },
  {
    field: "releaseDate",
    headerName: "Release",
    align: "center",
    width: 100,
    sortable: true,
    valueGetter: (_, row) => FormatTime(row.releaseDate, "date"),
  },
  {
    field: "categories",
    headerName: "Categories",
    width: 220,
    sortable: false,
    valueGetter: (_, row) => formatList(row.categories ?? null, 3),
  },
  {
    field: "tags",
    headerName: "Tags",
    width: 220,
    sortable: false,
    valueGetter: (_, row) => formatList(row.tags ?? null, 3),
  },
  {
    field: "location",
    headerName: "Location",
    width: 150,
    sortable: false,
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
    />
  );
};
