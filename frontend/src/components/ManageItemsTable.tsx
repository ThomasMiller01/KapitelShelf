import LaunchIcon from "@mui/icons-material/Launch";
import { IconButton } from "@mui/material";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import { NavLink } from "react-router-dom";

interface ManageItemsTableProps {
  // data
  items: any[];
  totalItems: number;
  isLoading: boolean;

  // layout
  columns: GridColDef<any>[];

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
  page,
  pageSize,
  setPagination,
}) => {
  return (
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
    />
  );
};

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
