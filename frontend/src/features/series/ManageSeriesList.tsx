import AddLinkIcon from "@mui/icons-material/AddLink";
import { GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import ConfirmDialog from "../../components/base/feedback/ConfirmDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import {
  LinkColumn,
  ManageItemsTable,
} from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { SeriesDTO } from "../../lib/api/KapitelShelf.Api";
import { useDeleteSeriesBulk } from "../../lib/requests/series/useDeleteSeriesBulk";
import { useMergeSeriesBulk } from "../../lib/requests/series/useMergeSeriesBulk";
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
  const { page, pageSize, sorting, filter, setItemsTableParams } =
    useItemsTableParams({
      defaultPageSize: 15,
    });

  const { data, isLoading, isError, refetch } = useSeriesListSimpleQuery({
    page,
    pageSize,
    sorting,
    filter,
  });

  const { mutate: deleteSeries } = useDeleteSeriesBulk();
  const { mutate: mergeSeries } = useMergeSeriesBulk();

  const [mergeTargetSeries, setMergeTargetSeries] = useState<
    SeriesDTO | undefined
  >(undefined);
  const [mergeSourceSeriesIds, setMergeSourceSeriesIds] = useState<string[]>(
    [],
  );
  const [mergeSeriesDialogOpen, setMergeSeriesDialogOpen] = useState(false);
  const mergeSeriesOpen = (selectedItemIds: string[]) => {
    // first element is target, all other source
    const targetSeriesId = selectedItemIds[0];
    const targetSeries = data?.items?.find((x) => x.id == targetSeriesId);
    setMergeTargetSeries(targetSeries);
    setMergeSourceSeriesIds(selectedItemIds.slice(1, selectedItemIds.length));

    setMergeSeriesDialogOpen(true);
  };

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  return (
    <>
      <ManageItemsTable
        // data
        items={data?.items ?? []}
        isLoading={isLoading}
        totalItems={data?.totalCount ?? 0}
        // layout
        columns={columns}
        itemName="serie"
        // pagination & sorting
        page={page}
        pageSize={pageSize}
        sorting={sorting}
        filter={filter}
        setItemsTableParams={setItemsTableParams}
        // actions
        deleteAction={deleteSeries}
        additionalActions={[
          {
            label: "Merge Series",
            action: mergeSeriesOpen,
            icon: <AddLinkIcon />,
            disabled: (selected) => selected.length <= 1,
          },
        ]}
      />
      <ConfirmDialog
        open={mergeSeriesDialogOpen}
        onCancel={() => setMergeSeriesDialogOpen(false)}
        onConfirm={() => {
          if (mergeTargetSeries?.id === undefined) {
            return;
          }

          mergeSeries({
            targetSeriesId: mergeTargetSeries.id,
            sourceSeriesIds: mergeSourceSeriesIds,
          });
          setMergeSeriesDialogOpen(false);
        }}
        title="Merge Series"
        description={`This will merge all books from ${mergeSourceSeriesIds.length} series into '${mergeTargetSeries?.name}'.`}
        confirmText="Merge"
        confirmColor="warning"
      />
    </>
  );
};
