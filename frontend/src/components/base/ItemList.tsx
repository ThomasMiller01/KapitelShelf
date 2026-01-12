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
import { useEffect, useState } from "react";
import { AutoComplete } from "./AutoComplete";

type ChipVariant = React.ComponentProps<typeof Chip>["variant"];

interface ItemListProps {
  itemName: string;
  items?: string[];
  onChange: (items: string[]) => void;
  width?: string;
  variant?: ChipVariant;
  useAutocomplete?: boolean;
  autocompleteFetchSuggestions?: (value: string) => Promise<string[]>;
}

const ItemList: React.FC<ItemListProps> = ({
  itemName,
  items: initial = [],
  onChange,
  width = "25ch",
  variant = "filled",
  useAutocomplete = false,
  autocompleteFetchSuggestions = undefined,
}) => {
  const [items, setItems] = useState<string[]>(initial);
  useEffect(() => {
    onChange(items);
  }, [items, onChange]);

  // keep items in sync with initial
  useEffect(() => {
    if (JSON.stringify(items) !== JSON.stringify(initial)) {
      setItems(initial);
    }
  }, [initial]);

  const [showAddTextfield, setShowAddTextfield] = useState(false);
  const [addTextfield, setAddTextfield] = useState("");
  const onSave = (value?: string): void => {
    const next: string = value ?? addTextfield;
    if (next !== "") {
      // dont allow duplicates
      setItems((items) => [...items.filter((x) => x !== next), next]);
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
        <Stack direction="row" spacing={1} alignItems="center">
          {useAutocomplete && autocompleteFetchSuggestions ? (
            <ItemListAutoComplete
              label={itemName}
              value={addTextfield}
              onChange={setAddTextfield}
              onKeyDown={onKeyDown}
              onSave={onSave}
              width={width}
              fetchSuggestions={autocompleteFetchSuggestions}
            />
          ) : (
            <ItemListTextField
              label={itemName}
              value={addTextfield}
              onChange={setAddTextfield}
              onKeyDown={onKeyDown}
              onSave={onSave}
              width={width}
            />
          )}
          <IconButton onClick={onClose}>
            <CloseIcon />
          </IconButton>
        </Stack>
      )}
    </Stack>
  );
};

interface ItemListTextFieldProps {
  label: string;
  value: string;
  onChange: (value: React.SetStateAction<string>) => void;
  onKeyDown: (key: string) => void;
  width: string;
  onSave: () => void;
}

const ItemListTextField: React.FC<ItemListTextFieldProps> = ({
  label,
  value,
  onChange,
  onKeyDown,
  width,
  onSave,
}) => {
  return (
    <TextField
      label={label}
      size="small"
      variant="outlined"
      value={value}
      autoFocus
      onChange={(event) => onChange(event.target.value)}
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
  );
};

interface ItemListAutoCompleteProps extends ItemListTextFieldProps {
  fetchSuggestions: (value: string) => Promise<string[]>;
}

const ItemListAutoComplete: React.FC<ItemListAutoCompleteProps> = ({
  label,
  value,
  onKeyDown,
  width,
  onSave,
  fetchSuggestions,
}) => {
  return (
    <AutoComplete
      label={label}
      size="small"
      variant="outlined"
      value={value}
      autoFocus
      onChange={onSave}
      onKeyDown={(event) => onKeyDown(event.key)}
      sx={{ width }}
      fetchSuggestions={fetchSuggestions}
    />
  );
};

export default ItemList;
