import CancelIcon from "@mui/icons-material/Cancel";
import DeleteIcon from "@mui/icons-material/Delete";
import LaunchIcon from "@mui/icons-material/Launch";
import SearchIcon from "@mui/icons-material/Search";
import ViewColumnIcon from "@mui/icons-material/ViewColumn";
import {
  Box,
  Button,
  IconButton,
  Stack,
  styled,
  TextField,
} from "@mui/material";
import InputAdornment from "@mui/material/InputAdornment";
import {
  ColumnsPanelTrigger,
  DataGrid,
  GRID_CHECKBOX_SELECTION_COL_DEF,
  GridColDef,
  GridRenderCellParams,
  GridToolbarProps,
  QuickFilter,
  QuickFilterClear,
  QuickFilterControl,
  Toolbar,
  ToolbarButton,
} from "@mui/x-data-grid";
import { ReactNode, useMemo, useState } from "react";
import { NavLink } from "react-router-dom";
import {
  setItemsTableParamsProps,
  SortingModel,
} from "../hooks/url/useItemsTableParams";
import { useSetting } from "../hooks/useSetting";
import { normalizeBook } from "../utils/BookUtils";
import { toTitleCase } from "../utils/TextUtils";
import { IconButtonWithTooltip } from "./base/IconButtonWithTooltip";
import ConfirmDialog from "./base/feedback/ConfirmDialog";

const StyledQuickFilter = styled(QuickFilter)({
  marginLeft: "auto",
});

interface AdditionalAction {
  label: string;
  icon?: ReactNode;
  action: (selectedItemIds: string[]) => void;
  disabled?: (selectedItemIds: string[]) => boolean;
}

interface Actions {
  deleteAction?: (selectedItemIds: string[]) => void;
  additionalActions?: AdditionalAction[];
}

interface ManageItemsTableProps extends Actions {
  settingKey: string;

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
  filter?: string | null;
  setItemsTableParams?: (params: setItemsTableParamsProps) => void;

  // editing
  onRowEdit?: (updatedRow: any, originalRow: any) => void;
}

export const ManageItemsTable: React.FC<ManageItemsTableProps> = ({
  settingKey,

  // data
  items,
  isLoading,
  totalItems,

  // layout
  columns: baseColumns,
  itemName,

  // pagination & sorting
  page = 1,
  pageSize = items.length,
  sorting,
  filter,
  setItemsTableParams,

  // actions
  deleteAction,
  additionalActions,

  // editing
  onRowEdit,
}) => {
  const columns = useMemo(
    () => [
      {
        ...GRID_CHECKBOX_SELECTION_COL_DEF,
        hideable: true,
      },
      ...baseColumns,
    ],
    [baseColumns],
  );

  const [visibleColumns, setVisibleColumns] = useSetting(
    `${settingKey}-columns`,
    columns.map((x) => x.field),
  );

  const [selected, setSelected] = useState<string[]>([]);

  const CustomToolbar = ({ ...props }) => (
    <ManageItemsToolbar
      selected={selected}
      itemName={itemName}
      deleteAction={deleteAction}
      additionalActions={additionalActions}
      {...props}
    />
  );

  return (
    <Box>
      <DataGrid
        // data
        rows={items}
        columns={columns}
        rowCount={totalItems}
        loading={isLoading}
        // styling
        density="compact"
        // columns visibility
        columnVisibilityModel={Object.fromEntries(
          columns.map((column) => [
            column.field,
            visibleColumns.includes(column.field),
          ]),
        )}
        disableRowSelectionExcludeModel
        onColumnVisibilityModelChange={(visibilityModel) => {
          const allFields = columns.map((c) => c.field);

          const newVisibleColumns =
            Object.keys(visibilityModel).length === 0
              ? allFields
              : allFields.filter((field) => visibilityModel[field] !== false);

          setVisibleColumns(newVisibleColumns);
        }}
        // selection
        checkboxSelection
        onRowSelectionModelChange={(newRowSelectionModel) =>
          setSelected(
            Array.from(newRowSelectionModel.ids).map((x) => String(x)),
          )
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
        // filter
        disableColumnFilter
        filterMode="server"
        filterModel={{
          items: [],
          quickFilterValues: filter?.split(" "),
        }}
        onFilterModelChange={(model, _) =>
          setItemsTableParams?.({
            nextFilter: model.quickFilterValues?.join(" "),

            // reset pagination when trying to filter
            nextPage: 1,
          })
        }
        // editing
        editMode="row"
        processRowUpdate={(updatedRow, originalRow) => {
          // only call onRowEdit, if updated and original differ
          const updatedJson = JSON.stringify(normalizeBook(updatedRow));
          const originalJson = JSON.stringify(normalizeBook(originalRow));
          if (updatedJson !== originalJson) {
            onRowEdit?.(updatedRow, originalRow);
          }

          return updatedRow;
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
          paginationRowsPerPage: `${toTitleCase(itemName) ?? "Row"}s per page`,
        }}
        // Toolbar
        slots={{ toolbar: CustomToolbar }}
        showToolbar
        slotProps={{
          toolbar: {
            showQuickFilter: filter !== undefined,
          },
        }}
      />
    </Box>
  );
};

interface ManageItemsToolbarProps extends GridToolbarProps, Actions {
  selected: string[];
  itemName?: string;
}

const ManageItemsToolbar: React.FC<ManageItemsToolbarProps> = ({
  selected,
  itemName,
  deleteAction,
  additionalActions,
  showQuickFilter,
}) => {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  return (
    <Toolbar style={{ minHeight: "auto" }}>
      <Stack
        direction={{ xs: "column", md: "row" }}
        spacing={{ xs: 1.5, md: 1 }}
        justifyContent="space-between"
        alignItems="start"
        sx={{ width: "100%" }}
      >
        <Stack direction="row" spacing={{ xs: 1, md: 2 }} alignItems="center">
          {/* Columns */}
          <ColumnsPanelTrigger render={<ToolbarButton />}>
            <ViewColumnIcon fontSize="small" />
          </ColumnsPanelTrigger>

          {/* Filter */}
          {showQuickFilter && (
            <StyledQuickFilter expanded>
              <QuickFilterControl
                render={({ ref, ...other }) => (
                  <TextField
                    {...other}
                    sx={{ width: 240 }}
                    inputRef={ref}
                    placeholder="Search..."
                    variant="standard"
                    slotProps={{
                      input: {
                        startAdornment: (
                          <InputAdornment position="start">
                            <SearchIcon fontSize="small" />
                          </InputAdornment>
                        ),
                        endAdornment: other.value ? (
                          <InputAdornment position="end">
                            <QuickFilterClear
                              edge="end"
                              size="small"
                              material={{ sx: { marginRight: -0.75 } }}
                            >
                              <CancelIcon fontSize="small" />
                            </QuickFilterClear>
                          </InputAdornment>
                        ) : null,
                        ...other.slotProps?.input,
                      },
                      ...other.slotProps,
                    }}
                  />
                )}
              />
            </StyledQuickFilter>
          )}
        </Stack>

        <Stack
          direction="row"
          spacing={1}
          alignItems="center"
          justifyContent="end"
          sx={{ width: "100%" }}
        >
          {additionalActions?.map(({ label, action, icon, disabled }) => {
            const isDisabled = disabled?.(selected);
            return (
              <Button
                key={label}
                size="small"
                variant="contained"
                color="secondary"
                startIcon={icon}
                disabled={isDisabled}
                onClick={() => action(selected)}
              >
                {label}
              </Button>
            );
          })}

          {/* Delete */}
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
              <ConfirmDialog
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
    </Toolbar>
  );
};

export const LinkColumn = (
  to: (params: GridRenderCellParams) => string,
): GridColDef<any> => {
  return {
    field: "Link",
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
