import AutocompleteMUI, {
  AutocompleteProps as AutocompletePropsMUI,
} from "@mui/material/Autocomplete";
import TextField, {
  TextFieldPropsSizeOverrides,
  TextFieldVariants,
} from "@mui/material/TextField";
import { debounce } from "@mui/material/utils";
import { useQuery } from "@tanstack/react-query";
import { ReactNode, useEffect, useMemo, useState } from "react";

export type AutoCompleteProps = Omit<
  AutocompletePropsMUI<string, false, false, true>,
  | "options"
  | "loading"
  | "onChange"
  | "onInputChange"
  | "inputValue"
  | "renderInput"
  | "getOptionLabel"
  | "filterOptions"
> & {
  label?: string;
  onChange: (value: string | null) => void;
  fetchSuggestions: (value: string) => Promise<string[]>;
  variant?: TextFieldVariants;
  error?: boolean;
  helperText?: ReactNode;
  size?: TextFieldPropsSizeOverrides;
};

export const AutoComplete: React.FC<AutoCompleteProps> = ({
  label,
  value,
  onChange,
  fetchSuggestions,
  variant = "filled",
  error = false,
  helperText = undefined,
  size,
  ...props
}) => {
  const [inputValue, setInputValue] = useState<string>(value ?? "");
  const [debouncedQuery, setDebouncedQuery] = useState<string>("");

  const triggerDebounce = useMemo(() => {
    return debounce((q: string): void => {
      setDebouncedQuery(q);
    }, 400);
  }, []);

  // update input value when external value changes
  useEffect(() => {
    setInputValue(value ?? "");
  }, [value]);

  // trigger debounce on every input change
  useEffect(() => {
    triggerDebounce(inputValue.trim());

    return () => {
      triggerDebounce.clear();
    };
  }, [inputValue, triggerDebounce]);

  const suggestionsQuery = useQuery({
    queryKey: ["autocomplete", label, debouncedQuery],
    queryFn: async (): Promise<string[]> => {
      return await fetchSuggestions(debouncedQuery);
    },
    staleTime: 60_000,
    gcTime: 5 * 60_000,
  });

  const fetchedOptions: readonly string[] = suggestionsQuery.data ?? [];

  const options: readonly string[] = value
    ? [value, ...fetchedOptions.filter((x) => x !== value)]
    : fetchedOptions;

  return (
    <AutocompleteMUI
      {...props}
      freeSolo
      selectOnFocus
      clearOnBlur
      handleHomeEndKeys
      options={options}
      onChange={(_, selected) => {
        if (selected?.startsWith("Add ")) {
          selected = selected.slice(4, selected.length);
        }

        onChange?.(selected);
      }}
      filterOptions={(opts, state) => {
        const typed: string = state.inputValue.trim();
        if (!typed) {
          return opts;
        }

        const exists: boolean = opts.some(
          (x) => x.toLowerCase() === typed.toLowerCase(),
        );
        if (exists) {
          return opts;
        }

        return [...opts, `Add ${typed}`];
      }}
      loading={suggestionsQuery.isFetching}
      value={value}
      inputValue={inputValue}
      onInputChange={(_, newInputValue) => setInputValue(newInputValue)}
      noOptionsText="No results"
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          variant={variant}
          error={error}
          helperText={helperText}
          size={size}
          slotProps={{
            input: {
              ...params.InputProps,
              endAdornment: params.InputProps.endAdornment,
            },
          }}
        />
      )}
    />
  );
};
