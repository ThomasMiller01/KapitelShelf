import AddIcon from "@mui/icons-material/Add";
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

interface ItemListProps {
  itemName: string;
  initial?: string[];
  onChange: (items: string[]) => void;
  width?: string;
}

const ItemList = ({
  itemName,
  initial = [],
  onChange,
  width = "25ch",
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
      setAddTextfield("");
    }

    setShowAddTextfield(false);
  };
  const onKeyDown = (key: string): void => {
    if (key === "Enter") {
      onSave();
    }
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
          sx={{ my: "4px !important" }}
        />
      ))}
      {!showAddTextfield && (
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          size="small"
          onClick={() => setShowAddTextfield(true)}
        >
          Add {itemName}
        </Button>
      )}
      {showAddTextfield && (
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
      )}
    </Stack>
  );
};

export default ItemList;
