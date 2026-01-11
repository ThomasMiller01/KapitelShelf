import FilterListIcon from "@mui/icons-material/FilterList";
import { type ReactElement, useEffect, useState } from "react";

import { useSetting } from "../../hooks/useSetting";
import { Selector } from "./Selector";

interface HiddenFilterProps<TItem> {
  items: TItem[];
  extractValue: (item: TItem) => string | null | undefined;
  onFilteredChange?: (filteredItems: TItem[]) => void;
  onHiddenOptionsChange?: (hiddenOptions: string[]) => void;
  settingsKey: string;
  defaultHiddenOptions?: string[];
  subIcon?: ReactElement;
  tooltip?: string;
  textColor?: (option: string) => string;
}

export const HiddenFilter = <TItem,>({
  items,
  extractValue,
  onFilteredChange,
  onHiddenOptionsChange,
  settingsKey,
  defaultHiddenOptions = [],
  subIcon,
  tooltip = "Filter elements",
  textColor = undefined,
}: HiddenFilterProps<TItem>): ReactElement => {
  const [filteredItems, setFilteredItems] = useState<TItem[]>(items);

  const [hiddenOptions, setHiddenOptions] = useSetting<string[]>(
    settingsKey,
    defaultHiddenOptions
  );

  // extract options from items
  const [options, setOptions] = useState<string[]>([]);
  useEffect(() => {
    if (items === undefined) {
      return;
    }

    // extract options from items
    const allOptions = items.map((x) => extractValue(x));

    // only allow strings
    const stringOptions = allOptions.filter((x): x is string => Boolean(x));

    // get the unique options and sort
    const uniqueOptions = Array.from(new Set(stringOptions));
    const sortedOptions = uniqueOptions.sort();

    setOptions(sortedOptions);
  }, [items, extractValue]);

  useEffect(() => {
    if (items === undefined) {
      return;
    }

    setFilteredItems(
      items.filter((x) => !hiddenOptions.includes(extractValue(x) ?? ""))
    );
  }, [items, hiddenOptions]);

  useEffect(() => {
    onFilteredChange?.(filteredItems);
  }, [filteredItems, onFilteredChange]);

  useEffect(() => {
    onHiddenOptionsChange?.(hiddenOptions);
  }, [hiddenOptions, onHiddenOptionsChange]);

  return (
    <Selector
      icon={<FilterListIcon />}
      subIcon={subIcon}
      tooltip={tooltip}
      options={options}
      selected={options.filter((x) => !hiddenOptions.includes(x))}
      onUnselect={(value: string) =>
        setHiddenOptions((prev) => Array.from(new Set([...prev, value])))
      }
      onSelect={(value: string) =>
        setHiddenOptions((prev) => prev.filter((x) => x !== value))
      }
      textColor={textColor}
    />
  );
};
