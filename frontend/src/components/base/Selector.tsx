import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import {
  Badge,
  Box,
  Checkbox,
  Divider,
  FormControlLabel,
  Menu,
  MenuItem,
  Tooltip,
  Typography,
} from "@mui/material";
import type { ReactElement } from "react";
import { useEffect, useState } from "react";

import { IconButtonWithTooltip } from "./IconButtonWithTooltip";

interface SelectorProps {
  icon: ReactElement;
  subIcon?: ReactElement;
  tooltip?: string;
  options: string[];
  selected?: string[];
  onChange?: (selected: string[]) => void;
  onSelect?: (selected: string) => void;
  onUnselect?: (unselected: string) => void;
}

export const Selector: React.FC<SelectorProps> = ({
  icon,
  subIcon,
  tooltip,
  options,
  selected: initialSelected = [],
  onChange,
  onSelect,
  onUnselect,
}) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const [selected, setSelected] = useState<string[]>(initialSelected);
  useEffect(() => {
    onChange?.(selected);
  }, [selected, onChange]);

  useEffect(() => {
    setSelected(initialSelected);
  }, [initialSelected]);

  const open = Boolean(anchorEl);

  const handleOpen = (e: React.MouseEvent<HTMLElement>): void => {
    setAnchorEl(e.currentTarget);
  };
  const handleClose = (): void => setAnchorEl(null);

  const allSelected = selected.length === options.length && options.length > 0;
  const isIndeterminate = selected.length > 0 && !allSelected;

  const handleSelectAll = (): void => {
    if (allSelected) {
      setSelected([]);
      options.forEach((value) => {
        onUnselect?.(value);
      });
    } else {
      setSelected(options);
      options.forEach((value) => {
        onSelect?.(value);
      });
    }
  };

  const handleToggle = (value: string): void => {
    if (selected.includes(value)) {
      setSelected(selected.filter((v) => v !== value));
      onUnselect?.(value);
    } else {
      setSelected([...selected, value]);
      onSelect?.(value);
    }
  };

  return (
    <>
      <IconButtonWithTooltip tooltip={tooltip} onClick={handleOpen}>
        <Badge badgeContent={allSelected ? "" : <NotAllShownBadge />}>
          <Badge
            badgeContent={subIcon}
            overlap="circular"
            anchorOrigin={{
              vertical: "bottom",
              horizontal: "right",
            }}
          >
            {icon}
          </Badge>
        </Badge>
      </IconButtonWithTooltip>
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        slotProps={{
          list: {
            dense: true,
          },
        }}
      >
        <MenuItem disableRipple>
          <FormControlLabel
            control={
              <Checkbox
                checked={allSelected}
                indeterminate={isIndeterminate}
                onChange={handleSelectAll}
                size="small"
                sx={{ padding: "6px", mr: 1 }}
              />
            }
            label="Select All"
            sx={{ width: "100%" }}
          />
        </MenuItem>
        <Divider />
        {options.length === 0 && (
          <MenuItem disabled>
            <Typography color="text.secondary" variant="body1">
              No options
            </Typography>
          </MenuItem>
        )}
        {options.map((option) => (
          <MenuItem
            key={option}
            disableRipple
            onClick={() => handleToggle(option)}
          >
            <Checkbox
              checked={selected.includes(option)}
              sx={{ padding: "6px", mr: 1 }}
              size="small"
            />
            <Box sx={{ fontSize: "1rem" }}>{option}</Box>
          </MenuItem>
        ))}
      </Menu>
    </>
  );
};

const NotAllShownBadge = (): ReactElement => (
  <Tooltip
    title="Some tasks are hidden."
    slotProps={{
      tooltip: {
        sx: {
          color: "rgb(244, 199, 199)",
          backgroundColor: "#753939",
        },
      },
    }}
  >
    <VisibilityOffIcon fontSize="small" color="error" />
  </Tooltip>
);
