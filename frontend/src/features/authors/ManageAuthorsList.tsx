import { GridColDef } from "@mui/x-data-grid";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ManageItemsTable } from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { AuthorDTO } from "../../lib/api/KapitelShelf.Api";
import { useAuthorsListQuery } from "../../lib/requests/authors/useAuthorsListQuery";
import { normalizeSeries } from "../../utils/SeriesUtils";

export const columns: GridColDef<AuthorDTO>[] = [
  {
    field: "firstName",
    headerName: "First Name",
    minWidth: 180,
    sortable: true,
    editable: true,
    valueGetter: (_, row) => row.firstName ?? "-",
  },
  {
    field: "lastName",
    headerName: "Last name",
    minWidth: 180,
    sortable: true,
    editable: true,
    valueGetter: (_, row) => row.lastName ?? "-",
  },
  {
    field: "totalBooks",
    headerName: "Books",
    type: "number",
    width: 120,
    sortable: true,
    align: "right",
    headerAlign: "right",
    valueGetter: (_, row) => row.totalBooks ?? 0,
  },
];

export const ManageAuthorsList = () => {
  const { page, pageSize, sorting, filter, setItemsTableParams } =
    useItemsTableParams({
      defaultPageSize: 15,
    });

  const { data, isLoading, isError, refetch } = useAuthorsListQuery({
    page,
    pageSize,
    sorting,
    filter,
  });

  // const { mutate: deleteSeries } = useDeleteSeriesBulk();
  // const { mutate: updateSeries } = useUpdateSeries();

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Authors" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="authors" onRetry={refetch} />;
  }

  return (
    <>
      <ManageItemsTable
        settingKey="manage-authors"
        // data
        items={data?.items ?? []}
        isLoading={isLoading}
        totalItems={data?.totalCount ?? 0}
        // layout
        columns={columns}
        itemName="author"
        // pagination & sorting
        page={page}
        pageSize={pageSize}
        sorting={sorting}
        filter={filter}
        setItemsTableParams={setItemsTableParams}
        // actions
        // deleteAction={deleteSeries}
        // editing
        onRowEdit={(updatedSeries, originalSeries) => {
          const updatedJson = JSON.stringify(normalizeSeries(updatedSeries));
          const originalJson = JSON.stringify(normalizeSeries(originalSeries));
          if (updatedJson === originalJson) {
            // if no changes, return
            return;
          }

          // updateSeries({
          //   ...updatedSeries,
          //   lastVolume: null,
          //   totalBooks: null,
          // });
        }}
      />
    </>
  );
};
