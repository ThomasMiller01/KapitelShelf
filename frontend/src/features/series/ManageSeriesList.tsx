import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { SeriesDTO } from "../../lib/api/KapitelShelf.Api";
import { useSeriesListSimpleQuery } from "../../lib/requests/series/useSeriesListSimpleQuery";
import { FormatTime } from "../../utils/TimeUtils";

export const columns: GridColDef<SeriesDTO>[] = [
  {
    field: "name",
    headerName: "Series",
    flex: 1,
    minWidth: 220,
    sortable: true,
    valueGetter: (_, row) => row.name ?? "-",
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
  {
    field: "updatedAt",
    headerName: "Updated",
    width: 180,
    sortable: true,
    valueGetter: (_, row) => FormatTime(row.createdAt),
  },
  {
    field: "createdAt",
    headerName: "Created",
    width: 180,
    sortable: true,
    valueGetter: (_, row) => FormatTime(row.createdAt),
  },
];

export const ManageSeriesList = () => {
  const [pageSize, setPageSize] = useState(15);
  const [page, setPage] = useState(1);

  const { data, isLoading, isError, refetch } = useSeriesListSimpleQuery({
    page,
    pageSize,
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  return (
    <DataGrid
      rows={data?.items ?? []}
      columns={columns}
      rowCount={data?.totalCount}
      loading={isLoading}
      disableRowSelectionOnClick
      disableColumnSorting
      disableColumnFilter
      checkboxSelection
      density="compact"
      pagination
      paginationMode="server"
      pageSizeOptions={[15, 25, 50]}
      paginationModel={{ page: page - 1, pageSize }}
      onPaginationModelChange={(model) => {
        setPage(model.page + 1);
        setPageSize(model.pageSize);
      }}
    />
  );
};
