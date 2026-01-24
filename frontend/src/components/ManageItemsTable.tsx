import DeleteIcon from "@mui/icons-material/Delete";
import LaunchIcon from "@mui/icons-material/Launch";
import { Box, IconButton, Stack, Typography } from "@mui/material";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { useState } from "react";
import { NavLink } from "react-router-dom";
import {
  setItemsTableParamsProps,
  SortingModel,
} from "../hooks/url/useItemsTableParams";
import { toTitleCase } from "../utils/TextUtils";
import { IconButtonWithTooltip } from "./base/IconButtonWithTooltip";
import DeleteDialog from "./base/feedback/DeleteDialog";

interface Actions {
  deleteAction?: (selectedItemIds: string[]) => void;
}

interface ManageItemsTableProps extends Actions {
  // data
  items: any[];
  isLoading: boolean;
  totalItems?: number;

  // layout
  columns: GridColDef<any>[];
  itemName?: string;

  // pagination & sorting
  page?: number;
  pageSize?: number;
  sorting?: SortingModel;
  setItemsTableParams?: (params: setItemsTableParamsProps) => void;
}

export const ManageItemsTable: React.FC<ManageItemsTableProps> = ({
  // data
  items,
  isLoading,
  totalItems,

  // layout
  columns,
  itemName,

  // pagination & sorting
  page = 1,
  pageSize = items.length,
  sorting,
  setItemsTableParams,

  // actions
  deleteAction,
}) => {
  const [selected, setSelected] = useState<string[]>([]);

  return (
    <Box>
      <ManageItemsActionBar
        selected={selected}
        itemName={itemName}
        deleteAction={deleteAction}
      />
      <DataGrid
        // data
        rows={items}
        columns={columns}
        rowCount={totalItems}
        loading={isLoading}
        // misc
        disableColumnFilter
        // styling
        density="compact"
        // selection
        checkboxSelection
        onRowSelectionModelChange={(newRowSelectionModel) =>
          setSelected(newRowSelectionModel.map((x) => String(x)))
        }
        disableRowSelectionOnClick
        // pagination
        pagination
        paginationMode="server"
        pageSizeOptions={[15, 25, 50]}
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(model) =>
          setItemsTableParams?.({
            nextPage: model.page + 1,
            nextPageSize: model.pageSize,
          })
        }
        // sorting
        disableColumnSorting={!sorting}
        sortingMode="server"
        sortModel={
          sorting && sorting.field && sorting.sort
            ? [{ field: sorting.field, sort: sorting.sort }]
            : []
        }
        onSortModelChange={(sortingModel) => {
          const first = sortingModel[0];

          setItemsTableParams?.({
            nextSorting: {
              field: first?.field ?? null,
              sort:
                first?.sort === "asc" || first?.sort === "desc"
                  ? first.sort
                  : null,
            },

            // reset pagination when trying to sort
            nextPage: 1,
          });
        }}
        // language
        localeText={{
          noRowsLabel: `No ${itemName ?? "row"}s`,
          noResultsOverlayLabel: `No ${itemName ?? "result"}s found`,
          footerRowSelected: (count) =>
            count !== 1
              ? `${count.toLocaleString()} ${itemName ?? "row"}s selected`
              : `${count.toLocaleString()} ${itemName ?? "row"} selected`,
          footerTotalRows: `Total ${toTitleCase(itemName) ?? "Row"}s:`,
          checkboxSelectionSelectAllRows: `Select all ${itemName ?? "row"}s`,
          checkboxSelectionUnselectAllRows: `Unselect all ${
            itemName ?? "row"
          }s`,
          checkboxSelectionSelectRow: `Select ${itemName ?? "row"}`,
          checkboxSelectionUnselectRow: `Unselect ${itemName ?? "row"}`,
          MuiTablePagination: {
            labelRowsPerPage: `${toTitleCase(itemName) ?? "Row"}s per page`,
          },
        }}
      />
    </Box>
  );
};

interface ManageItemsActionBarProps extends Actions {
  selected: string[];
  itemName?: string;
}

const ManageItemsActionBar: React.FC<ManageItemsActionBarProps> = ({
  selected,
  itemName,
  deleteAction,
}) => {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  return (
    <Box
      sx={{
        mb: 1.5,
        px: 2,
        py: 1,
        borderRadius: 1,
        backgroundColor: (theme) => theme.palette.background.paper,
        border: "1px solid",
        borderColor: "divider",
      }}
    >
      <Stack
        direction="row"
        spacing={2}
        justifyContent="space-between"
        alignItems="center"
      >
        <Typography>Manage your {toTitleCase(itemName) ?? "Row"}s</Typography>
        <Stack
          direction="row"
          spacing={2}
          justifyContent="end"
          alignItems="center"
        >
          {deleteAction && (
            <>
              <IconButtonWithTooltip
                tooltip={`Delete ${selected.length} selected ${
                  itemName ?? "row"
                }${selected.length > 1 ? "s" : ""}`}
                disabled={selected.length === 0}
                color="error"
                onClick={() => setDeleteDialogOpen(true)}
              >
                <DeleteIcon />
              </IconButtonWithTooltip>
              <DeleteDialog
                open={deleteDialogOpen}
                onCancel={() => setDeleteDialogOpen(false)}
                onConfirm={() => {
                  deleteAction(selected);
                  setDeleteDialogOpen(false);
                }}
              />
            </>
          )}
        </Stack>
      </Stack>
    </Box>
  );
};

export const LinkColumn = (
  to: (params: GridRenderCellParams) => string,
): GridColDef<any> => {
  return {
    field: "link",
    headerName: "",
    width: 64,
    sortable: false,
    filterable: false,
    disableColumnMenu: true,
    align: "center",
    renderCell: (params) => {
      const toValue = to(params);
      return (
        <IconButton
          component={NavLink}
          to={toValue}
          size="small"
          color="inherit"
        >
          <LaunchIcon fontSize="small" />
        </IconButton>
      );
    },
  };
};
