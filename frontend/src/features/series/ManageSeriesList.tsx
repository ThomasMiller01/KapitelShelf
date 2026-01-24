import { GridColDef } from "@mui/x-data-grid";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import {
  LinkColumn,
  ManageItemsTable,
} from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { SeriesDTO } from "../../lib/api/KapitelShelf.Api";
import { useDeleteSeriesBulk } from "../../lib/requests/series/useDeleteSeriesBulk";
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
    width: 120,
    sortable: true,
    valueGetter: (_, row) => FormatTime(row.createdAt, "date"),
  },
  LinkColumn((params) => `/library/series/${params.row.id}`),
];

export const ManageSeriesList = () => {
  const { page, pageSize, setItemsTableParams } = useItemsTableParams({
    defaultPageSize: 15,
  });

  const { data, isLoading, isError, refetch } = useSeriesListSimpleQuery({
    page,
    pageSize,
  });

  const { mutate: deleteSeries } = useDeleteSeriesBulk();

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  return (
    <ManageItemsTable
      // data
      items={data?.items ?? []}
      isLoading={isLoading}
      totalItems={data?.totalCount ?? 0}
      // layout
      columns={columns}
      itemName="serie"
      // pagination
      page={page}
      pageSize={pageSize}
      setItemsTableParams={setItemsTableParams}
      // actions
      deleteAction={deleteSeries}
    />
  );
};
