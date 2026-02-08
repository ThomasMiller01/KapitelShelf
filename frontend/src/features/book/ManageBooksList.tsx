import { Chip, Rating, Stack } from "@mui/material";
import { GridColDef } from "@mui/x-data-grid";
import { AutoComplete } from "../../components/base/AutoComplete";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import {
  EditAutoCompleteSX,
  EditDatePickerCell,
  LinkColumn,
  ManageItemsTable,
} from "../../components/ManageItemsTable";
import { ApiClients, useApi } from "../../contexts/ApiProvider";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { BookDTO, CategoryDTO, TagDTO } from "../../lib/api/KapitelShelf.Api";
import { useBooksList } from "../../lib/requests/books/useBooksList";
import { useDeleteBooks } from "../../lib/requests/books/useDeleteBooks";
import { useUpdateBook } from "../../lib/requests/books/useUpdateBook";
import { normalizeBook } from "../../utils/BookUtils";
import { LocationTypeToString } from "../../utils/LocationUtils";
import { FormatTime } from "../../utils/TimeUtils";

const columns = (clients: ApiClients): GridColDef<BookDTO>[] => [
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
      if (parts.length === 1) {
        return {
          ...row,
          author: {
            firstName: parts[0],
            lastName: "",
          },
        };
      }

      return {
        ...row,
        author: {
          firstName: parts.slice(0, -1).join(" "),
          lastName: parts[parts.length - 1] ?? "",
        },
      };
    },
    renderEditCell: ({ id, field, value, api }) => (
      <AutoComplete
        variant="standard"
        size="small"
        fullWidth
        value={value}
        onChange={(newValue) =>
          api.setEditCellValue({ id, field, value: newValue })
        }
        fetchSuggestions={async (value) => {
          const { data } = await clients.authors.authorsAutocompleteGet(value);
          return data;
        }}
        sx={EditAutoCompleteSX}
      />
    ),
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
          value?.length > 0
            ? ({ ...row.series, name: value } as any)
            : undefined,
      };
    },
    renderEditCell: ({ id, field, value, api }) => (
      <AutoComplete
        variant="standard"
        size="small"
        fullWidth
        value={value}
        onChange={(newValue) =>
          api.setEditCellValue({ id, field, value: newValue })
        }
        fetchSuggestions={async (value) => {
          const { data } = await clients.series.seriesAutocompleteGet(value);
          return data;
        }}
        sx={EditAutoCompleteSX}
      />
    ),
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
      return new Date(
        Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate()),
      );
    },
    valueFormatter: (value) => FormatTime(value, "date"),
    renderEditCell: (params) => <EditDatePickerCell {...params} />,
  },
  {
    field: "rating",
    headerName: "Rating",
    type: "number",
    width: 120,
    sortable: true,
    align: "right",
    headerAlign: "right",
    renderCell: (params) => {
      if (params.value == null) {
        return null;
      }

      return (
        <Rating
          value={params.value / 2}
          max={5}
          precision={0.5}
          readOnly
          size="small"
        />
      );
    },
  },
  {
    field: "categories",
    headerName: "Categories",
    width: 210,
    sortable: false,
    editable: false,
    valueGetter: (_, row) => row.categories ?? null,
    renderCell: (params) => {
      const categories = params.value ?? [];
      if (categories.length === 0) {
        return <span>-</span>;
      }

      return (
        <Stack
          direction="row"
          spacing={1}
          mb={1.5}
          flexWrap="wrap"
          alignItems="center"
        >
          {categories.slice(0, 1).map((category: CategoryDTO) => (
            <Chip
              key={category.id}
              label={category.name}
              color="primary"
              size="small"
              sx={{ my: "4px !important" }}
            />
          ))}
          {categories.length > 1 && (
            <Chip
              label={`+${categories.length - 1}`}
              size="small"
              variant="outlined"
              sx={{ my: "4px !important" }}
            />
          )}
        </Stack>
      );
    },
  },
  {
    field: "tags",
    headerName: "Tags",
    width: 210,
    sortable: false,
    editable: false,
    valueGetter: (_, row) => row.tags ?? null,
    renderCell: (params) => {
      const tags = params.value ?? [];
      if (tags.length === 0) {
        return <span>-</span>;
      }

      return (
        <Stack
          direction="row"
          spacing={1}
          flexWrap="wrap"
          alignItems="center"
          sx={{ py: "2px" }}
        >
          {tags
            .filter((t: TagDTO) => Boolean(t && (t.name ?? "").trim()))
            .map((tag: TagDTO) => {
              return (
                <Chip
                  key={tag.id}
                  label={tag.name}
                  variant="outlined"
                  size="small"
                  sx={{ my: "4px !important" }}
                />
              );
            })}
        </Stack>
      );
    },
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
  const { clients } = useApi();

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
  const { mutate: updateBook } = useUpdateBook();

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
      columns={columns(clients)}
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
      onRowEdit={(updatedBook, originalBook) => {
        const updatedJson = JSON.stringify(normalizeBook(updatedBook));
        const originalJson = JSON.stringify(normalizeBook(originalBook));
        if (updatedJson === originalJson) {
          // if no changes, return
          return;
        }

        updateBook(updatedBook);
      }}
    />
  );
};
