import { Box } from "@mui/material";
import { RichTreeView } from "@mui/x-tree-view";
import React, { useCallback, useEffect, useState } from "react";

import { useListCloudDirectory } from "../../../lib/requests/cloudstorages/useListCloudDirectory";
import LoadingCard from "../../base/feedback/LoadingCard";

interface Directory {
  id: string;
  label: string;
  path: string;
  children?: Directory[];
}

interface ConfigureOneDriveProps {
  storageId: string | undefined;
  onDirectorySelect: (directory: string) => void;
}

export const ConfigureOneDrive: React.FC<ConfigureOneDriveProps> = ({
  storageId,
  onDirectorySelect,
}) => {
  const [items, setItems] = useState<Directory[]>([]);

  const getItemId = useCallback((item: Directory) => item.id, []);
  const getItemLabel = useCallback((item: Directory) => item.label, []);
  const getItemChildren = useCallback(
    (item: Directory) =>
      item.children ?? [
        {
          id: Math.random().toString(),
          label: "Loading ...",
          path: "",
          children: [],
        },
      ],
    []
  );

  const { mutateAsync: getDirectories } = useListCloudDirectory(storageId);

  const updateNodeChildrenById = useCallback(
    (
      tree: Directory[],
      targetId: string,
      newChildren: Directory[]
    ): Directory[] =>
      tree.map((node) => {
        if (node.id === targetId) {
          return { ...node, children: newChildren };
        }

        if (node.children) {
          return {
            ...node,
            children: updateNodeChildrenById(
              node.children,
              targetId,
              newChildren
            ),
          };
        }

        return node;
      }),
    []
  );

  const getChildren = useCallback(
    async (parent: Directory): Promise<void> => {
      const response = await getDirectories(parent.path);
      if (response === undefined) {
        return;
      }

      const directories = response?.map((x) => ({
        id: x.id ?? "",
        label: x.name ?? "",
        path: (parent.path ?? "") + "/" + (x.path ?? ""),
      }));
      setItems((prev) => updateNodeChildrenById(prev, parent.id, directories));
    },
    [getDirectories, updateNodeChildrenById]
  );

  const [initialLoading, setInitialLoading] = useState(false);

  useEffect(() => {
    setInitialLoading(true);
    getDirectories("/").then((response) => {
      if (response === undefined) {
        return;
      }

      const directories = response?.map((x) => ({
        id: x.id ?? "",
        label: x.name ?? "",
        path: x.path ?? "",
      }));
      setItems(directories);

      setInitialLoading(false);
    });
  }, [getDirectories]);

  const findNodeById = useCallback(
    (tree: Directory[], id: string): Directory | undefined => {
      for (const node of tree) {
        if (node.id === id) {
          return node;
        }

        if (node.children) {
          const childResult = findNodeById(node.children, id);
          if (childResult) {
            return childResult;
          }
        }
      }

      return undefined;
    },
    []
  );

  const handleItemClick = useCallback(
    (_: React.SyntheticEvent, id: string) => {
      const node = findNodeById(items, id);
      if (node) {
        onDirectorySelect(node.path);
      }
    },
    [items, onDirectorySelect, findNodeById]
  );

  const handleItemExpansion = useCallback(
    (
      _: React.SyntheticEvent<Element, Event> | null,
      itemId: string,
      isExpanded: boolean
    ) => {
      if (!isExpanded) {
        return;
      }

      // fetch children for newly expanded item
      const node = findNodeById(items, itemId);
      if (node && node.children === undefined) {
        getChildren(node);
      }
    },
    [items, getChildren, findNodeById]
  );

  if (initialLoading) {
    return <LoadingCard itemName="Cloud Directories" useLogo delayed />;
  }

  return (
    <Box px={2}>
      <RichTreeView<Directory>
        items={items}
        getItemId={getItemId}
        getItemLabel={getItemLabel}
        getItemChildren={getItemChildren}
        onItemClick={handleItemClick}
        expansionTrigger="iconContainer"
        onItemExpansionToggle={handleItemExpansion}
      />
    </Box>
  );
};
