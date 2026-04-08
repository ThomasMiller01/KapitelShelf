import AddLinkIcon from "@mui/icons-material/AddLink";
import { GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import ConfirmDialog from "../../../shared/components/base/feedback/ConfirmDialog";
import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { ManageItemsTable } from "../../../shared/components/ManageItemsTable";
import { useItemsTableParams } from "../../../shared/hooks/useItemsTableParams";
import { AuthorDTO } from "../../../lib/api/KapitelShelf.Api/index.ts";
import { useAuthorsListQuery } from "../hooks/api/useAuthorsListQuery";
import { useDeleteAuthorsBulk } from "../hooks/api/useDeleteAuthorsBulk";
import { useMergeAuthorsBulk } from "../hooks/api/useMergeAuthorsBulk";
import { useUpdateAuthor } from "../hooks/api/useUpdateAuthor";
import { normalizeAuthor } from "../utils/AuthorUtils";

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

  const { mutate: deleteAuthors } = useDeleteAuthorsBulk();
  const { mutate: updateAuthor } = useUpdateAuthor();
  const { mutate: mergeAuthors } = useMergeAuthorsBulk();

  const [mergeTargetAuthor, setMergeTargetAuthor] = useState<
    AuthorDTO | undefined
  >(undefined);
  const [mergeSourceAuthorsIds, setMergeSourceAuthorsIds] = useState<string[]>(
    [],
  );
  const [mergeAuthorsDialogOpen, setMergeAuthorsDialogOpen] = useState(false);
  const mergeAuthorsOpen = (selectedItemIds: string[]) => {
    // first element is target, all other source
    const targetAuthorId = selectedItemIds[0];
    const targetAuthor = data?.items?.find((x) => x.id == targetAuthorId);
    setMergeTargetAuthor(targetAuthor);
    setMergeSourceAuthorsIds(selectedItemIds.slice(1, selectedItemIds.length));

    setMergeAuthorsDialogOpen(true);
  };

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
        deleteAction={deleteAuthors}
        additionalActions={[
          {
            label: "Merge Authors",
            action: mergeAuthorsOpen,
            icon: <AddLinkIcon />,
            disabled: (selected) => selected.length <= 1,
          },
        ]}
        // editing
        onRowEdit={(updatedAuthor, originalAuthor) => {
          const updatedJson = JSON.stringify(normalizeAuthor(updatedAuthor));
          const originalJson = JSON.stringify(normalizeAuthor(originalAuthor));
          if (updatedJson === originalJson) {
            // if no changes, return
            return;
          }

          updateAuthor({ ...updatedAuthor, totalBooks: null });
        }}
      />
      <ConfirmDialog
        open={mergeAuthorsDialogOpen}
        onCancel={() => setMergeAuthorsDialogOpen(false)}
        onConfirm={() => {
          if (mergeTargetAuthor?.id === undefined) {
            return;
          }

          mergeAuthors({
            targetAuthorId: mergeTargetAuthor.id,
            sourceAuthorsIds: mergeSourceAuthorsIds,
          });
          setMergeAuthorsDialogOpen(false);
        }}
        title="Merge Authors"
        description={`This will merge ${mergeSourceAuthorsIds.length} authors into '${mergeTargetAuthor?.firstName} ${mergeTargetAuthor?.lastName}'.`}
        confirmText="Merge"
        confirmColor="warning"
      />
    </>
  );
};
