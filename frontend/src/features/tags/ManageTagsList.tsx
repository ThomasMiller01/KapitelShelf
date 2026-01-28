import AddLinkIcon from "@mui/icons-material/AddLink";
import { GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import ConfirmDialog from "../../components/base/feedback/ConfirmDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ManageItemsTable } from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { TagDTO } from "../../lib/api/KapitelShelf.Api";
import { useDeleteTagsBulk } from "../../lib/requests/tags/useDeleteTagsBulk";
import { useMergeTagsBulk } from "../../lib/requests/tags/useMergeTagsBulk";
import { useTagsListQuery } from "../../lib/requests/tags/useTagsListQuery";
import { useUpdateTag } from "../../lib/requests/tags/useUpdateTag";
import { normalizeTag } from "../../utils/TagUtils";

export const columns: GridColDef<TagDTO>[] = [
  {
    field: "name",
    headerName: "Tag",
    minWidth: 300,
    sortable: true,
    editable: true,
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
];

export const ManageTagsList = () => {
  const { page, pageSize, sorting, filter, setItemsTableParams } =
    useItemsTableParams({
      defaultPageSize: 15,
    });

  const { data, isLoading, isError, refetch } = useTagsListQuery({
    page,
    pageSize,
    sorting,
    filter,
  });

  const { mutate: deleteTags } = useDeleteTagsBulk();
  const { mutate: updateTags } = useUpdateTag();
  const { mutate: mergeTags } = useMergeTagsBulk();

  const [mergeTargetTag, setMergeTargetTag] = useState<TagDTO | undefined>(
    undefined,
  );
  const [mergeSourceTagIds, setMergeSourceTagIds] = useState<string[]>([]);
  const [mergeTagsDialogOpen, setMergeTagsDialogOpen] = useState(false);
  const mergeTagsOpen = (selectedItemIds: string[]) => {
    // first element is target, all other source
    const targetTagId = selectedItemIds[0];
    const targetTag = data?.items?.find((x) => x.id == targetTagId);
    setMergeTargetTag(targetTag);
    setMergeSourceTagIds(selectedItemIds.slice(1, selectedItemIds.length));

    setMergeTagsDialogOpen(true);
  };

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Tags" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="tags" onRetry={refetch} />;
  }

  return (
    <>
      <ManageItemsTable
        settingKey="manage-tags"
        // data
        items={data?.items ?? []}
        isLoading={isLoading}
        totalItems={data?.totalCount ?? 0}
        // layout
        columns={columns}
        itemName="tag"
        // pagination & sorting
        page={page}
        pageSize={pageSize}
        sorting={sorting}
        filter={filter}
        setItemsTableParams={setItemsTableParams}
        // actions
        deleteAction={deleteTags}
        additionalActions={[
          {
            label: "Merge Tags",
            action: mergeTagsOpen,
            icon: <AddLinkIcon />,
            disabled: (selected) => selected.length <= 1,
          },
        ]}
        // editing
        onRowEdit={(updatedTag, originalTag) => {
          const updatedJson = JSON.stringify(normalizeTag(updatedTag));
          const originalJson = JSON.stringify(normalizeTag(originalTag));
          if (updatedJson === originalJson) {
            // if no changes, return
            return;
          }

          updateTags({ ...updatedTag, totalBooks: null });
        }}
      />
      <ConfirmDialog
        open={mergeTagsDialogOpen}
        onCancel={() => setMergeTagsDialogOpen(false)}
        onConfirm={() => {
          if (mergeTargetTag?.id === undefined) {
            return;
          }

          mergeTags({
            targetTagId: mergeTargetTag.id,
            sourceTagsIds: mergeSourceTagIds,
          });
          setMergeTagsDialogOpen(false);
        }}
        title="Merge Tags"
        description={`This will merge ${mergeSourceTagIds.length} tags into '${mergeTargetTag?.name}'.`}
        confirmText="Merge"
        confirmColor="warning"
      />
    </>
  );
};
