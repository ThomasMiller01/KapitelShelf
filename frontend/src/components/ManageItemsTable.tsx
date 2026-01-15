import LaunchIcon from "@mui/icons-material/Launch";
import { IconButton } from "@mui/material";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { NavLink } from "react-router-dom";
import { toTitleCase } from "../utils/TextUtils";

interface ManageItemsTableProps {
  // data
  items: any[];
  totalItems: number;
  isLoading: boolean;

  // layout
  columns: GridColDef<any>[];
  itemName?: string;

  // pagination
  page: number;
  pageSize: number;
  setPagination: (nextPage: number, nextPageSize: number) => void;
}

export const ManageItemsTable: React.FC<ManageItemsTableProps> = ({
  items,
  totalItems,
  isLoading,
  columns,
  itemName,
  page,
  pageSize,
  setPagination,
}) => (
  <DataGrid
    rows={items}
    columns={columns}
    rowCount={totalItems}
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
    onPaginationModelChange={(model) =>
      setPagination(model.page + 1, model.pageSize)
    }
    localeText={{
      noRowsLabel: `No ${itemName ?? "row"}s`,
      noResultsOverlayLabel: `No ${itemName ?? "result"}s found`,
      footerRowSelected: (count) =>
        count !== 1
          ? `${count.toLocaleString()} ${itemName ?? "row"}s selected`
          : `${count.toLocaleString()} ${itemName ?? "row"} selected`,
      footerTotalRows: `Total ${toTitleCase(itemName) ?? "Row"}s:`,
      checkboxSelectionSelectAllRows: `Select all ${itemName ?? "row"}s`,
      checkboxSelectionUnselectAllRows: `Unselect all ${itemName ?? "row"}s`,
      checkboxSelectionSelectRow: `Select ${itemName ?? "row"}`,
      checkboxSelectionUnselectRow: `Unselect ${itemName ?? "row"}`,
      MuiTablePagination: {
        labelRowsPerPage: `${toTitleCase(itemName) ?? "Row"}s per page`,
      },
    }}
  />
);

export const LinkColumn = (
  to: (params: GridRenderCellParams) => string
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
