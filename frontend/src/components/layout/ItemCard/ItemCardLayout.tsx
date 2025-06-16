import type { TypographyProps } from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement } from "react";

import DetailedItemCardLayout, {
  DetailedMetadataItem,
} from "./DetailedItemCardLayout";
import NormalItemCardLayout, {
  NormalMetadataItem,
} from "./NormalItemCardLayout";

export type ItemCardType = "normal" | "detailed";

export interface MetadataItemProps extends TypographyProps {
  icon?: ReactNode;
  children: ReactNode;
  metadataVariant?: ItemCardType;
}

export const MetadataItem = ({
  metadataVariant,
  ...props
}: MetadataItemProps): ReactElement => {
  switch (metadataVariant) {
    case "detailed":
      return <DetailedMetadataItem {...props} />;
    case "normal":
    default:
      return <NormalMetadataItem {...props} />;
  }
};

export interface ItemCardLayoutProps {
  title: string | null | undefined;
  description?: string | null | undefined;
  link?: string | null | undefined;
  onClick?: () => void | undefined;
  image?: string | null | undefined;
  fallbackImage?: string | null | undefined;
  badge?: string | null | undefined;
  squareBadge?: boolean;
  metadata: ReactNode[];

  itemVariant?: ItemCardType;
}

const ItemCardLayout = ({
  itemVariant,
  ...props
}: ItemCardLayoutProps): ReactElement => {
  switch (itemVariant) {
    case "detailed":
      return <DetailedItemCardLayout {...props} />;
    case "normal":
    default:
      return <NormalItemCardLayout {...props} />;
  }
};

export default ItemCardLayout;
