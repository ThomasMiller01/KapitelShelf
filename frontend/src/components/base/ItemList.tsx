import AddIcon from "@mui/icons-material/Add";
import CloseIcon from "@mui/icons-material/Close";
import SaveIcon from "@mui/icons-material/Save";
import {
  Button,
  Chip,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
} from "@mui/material";
import { type ReactElement, useEffect, useState } from "react";

type ChipVariant = React.ComponentProps<typeof Chip>["variant"];

interface ItemListProps {
  itemName: string;
  initial?: string[];
  onChange: (items: string[]) => void;
  width?: string;
  variant?: ChipVariant;
}

const ItemList = ({
  itemName,
  initial = [],
  onChange,
  width = "25ch",
  variant = "filled",
}: ItemListProps): ReactElement => {
  const [items, setItems] = useState<string[]>(initial);
  useEffect(() => {
    onChange(items);
  }, [items, onChange]);

  const [showAddTextfield, setShowAddTextfield] = useState(false);
  const [addTextfield, setAddTextfield] = useState("");
  const onSave = (): void => {
    if (addTextfield !== "") {
      // dont allow duplicates
      setItems((items) => [
        ...items.filter((x) => x !== addTextfield),
        addTextfield,
      ]);
    }

    onClose();
  };
  const onKeyDown = (key: string): void => {
    if (key === "Enter") {
      onSave();
    }
  };
  const onClose = (): void => {
    setAddTextfield("");
    setShowAddTextfield(false);
  };

  return (
    <Stack
      direction="row"
      spacing={1}
      mb={1.5}
      flexWrap="wrap"
      alignItems="center"
    >
      {items.map((item) => (
        <Chip
          key={item}
          label={item}
          onDelete={() => setItems((items) => items.filter((x) => x !== item))}
          color="primary"
          variant={variant}
          sx={{ mb: "8px !important" }}
        />
      ))}
      {!showAddTextfield && (
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          size="small"
          sx={{ mb: "8px !important" }}
          onClick={() => setShowAddTextfield(true)}
        >
          Add {itemName}
        </Button>
      )}
      {showAddTextfield && (
        <Stack
          direction="row"
          spacing={1}
          mt="15px !important"
          alignItems="center"
        >
          <TextField
            label={itemName}
            size="small"
            variant="outlined"
            value={addTextfield}
            autoFocus
            onChange={(event) => setAddTextfield(event.target.value)}
            onKeyDown={(event) => onKeyDown(event.key)}
            sx={{ width }}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={onSave} edge="end">
                      <SaveIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              },
            }}
          />
          <IconButton onClick={onClose}>
            <CloseIcon />
          </IconButton>
        </Stack>
      )}
    </Stack>
  );
};

export default ItemList;
