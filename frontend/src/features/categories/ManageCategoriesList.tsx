import AddLinkIcon from "@mui/icons-material/AddLink";
import { GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import ConfirmDialog from "../../components/base/feedback/ConfirmDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ManageItemsTable } from "../../components/ManageItemsTable";
import { useItemsTableParams } from "../../hooks/url/useItemsTableParams";
import { CategoryDTO } from "../../lib/api/KapitelShelf.Api";
import { useCategoriesListQuery } from "../../lib/requests/categories/useCategoriesListQuery";
import { useDeleteCategoriesBulk } from "../../lib/requests/categories/useDeleteCategoriesBulk";
import { useMergeCategoriesBulk } from "../../lib/requests/categories/useMergeCategoriesBulk";
import { useUpdateCategory } from "../../lib/requests/categories/useUpdateCategory";
import { normalizeCategory } from "../../utils/CategoryUtils";

export const columns: GridColDef<CategoryDTO>[] = [
  {
    field: "name",
    headerName: "Category",
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

export const ManageCategoriesList = () => {
  const { page, pageSize, sorting, filter, setItemsTableParams } =
    useItemsTableParams({
      defaultPageSize: 15,
    });

  const { data, isLoading, isError, refetch } = useCategoriesListQuery({
    page,
    pageSize,
    sorting,
    filter,
  });

  const { mutate: deleteCategories } = useDeleteCategoriesBulk();
  const { mutate: updateCategory } = useUpdateCategory();
  const { mutate: mergeCategories } = useMergeCategoriesBulk();

  const [mergeTargetCategory, setMergeTargetCategory] = useState<
    CategoryDTO | undefined
  >(undefined);
  const [mergeSourceCategoryIds, setMergeSourceCategoryIds] = useState<
    string[]
  >([]);
  const [mergeCategoriesDialogOpen, setMergeCategoriesDialogOpen] =
    useState(false);
  const mergeCategoriesOpen = (selectedItemIds: string[]) => {
    // first element is target, all other source
    const targetCategoryId = selectedItemIds[0];
    const targetCategory = data?.items?.find((x) => x.id == targetCategoryId);
    setMergeTargetCategory(targetCategory);
    setMergeSourceCategoryIds(selectedItemIds.slice(1, selectedItemIds.length));

    setMergeCategoriesDialogOpen(true);
  };

  if (isLoading) {
    return (
      <LoadingCard useLogo delayed itemName="Categories" showRandomFacts />
    );
  }

  if (isError) {
    return <RequestErrorCard itemName="categories" onRetry={refetch} />;
  }

  return (
    <>
      <ManageItemsTable
        settingKey="manage-categories"
        // data
        items={data?.items ?? []}
        isLoading={isLoading}
        totalItems={data?.totalCount ?? 0}
        // layout
        columns={columns}
        itemName="category"
        // pagination & sorting
        page={page}
        pageSize={pageSize}
        sorting={sorting}
        filter={filter}
        setItemsTableParams={setItemsTableParams}
        // actions
        deleteAction={deleteCategories}
        additionalActions={[
          {
            label: "Merge Categories",
            action: mergeCategoriesOpen,
            icon: <AddLinkIcon />,
            disabled: (selected) => selected.length <= 1,
          },
        ]}
        // editing
        onRowEdit={(updatedCategory, originalCategory) => {
          const updatedJson = JSON.stringify(
            normalizeCategory(updatedCategory),
          );
          const originalJson = JSON.stringify(
            normalizeCategory(originalCategory),
          );
          if (updatedJson === originalJson) {
            // if no changes, return
            return;
          }

          updateCategory({ ...updatedCategory, totalBooks: null });
        }}
      />
      <ConfirmDialog
        open={mergeCategoriesDialogOpen}
        onCancel={() => setMergeCategoriesDialogOpen(false)}
        onConfirm={() => {
          if (mergeTargetCategory?.id === undefined) {
            return;
          }

          mergeCategories({
            targetCategoryId: mergeTargetCategory.id,
            sourceCategoryIds: mergeSourceCategoryIds,
          });
          setMergeCategoriesDialogOpen(false);
        }}
        title="Merge Categories"
        description={`This will merge ${mergeSourceCategoryIds.length} categories into '${mergeTargetCategory?.name}'.`}
        confirmText="Merge"
        confirmColor="warning"
      />
    </>
  );
};
