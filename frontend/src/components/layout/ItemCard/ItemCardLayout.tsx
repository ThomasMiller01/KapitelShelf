import type { TooltipProps } from "@mui/material";
import {
  Chip,
  Grid,
  Stack,
  styled,
  Tooltip,
  tooltipClasses,
  type TypographyProps,
} from "@mui/material";
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
  metadata?: ReactNode[];
  selected?: boolean;

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

const MissingItemsTooltip = styled(({ className, ...props }: TooltipProps) => (
  <Tooltip {...props} classes={{ popper: className }} />
))(({ theme }) => ({
  [`& .${tooltipClasses.tooltip}`]: {
    maxWidth: 500,
    backgroundColor: theme.palette.background.paper,
    backgroundImage:
      "linear-gradient(rgba(255, 255, 255, 0.03), rgba(255, 255, 255, 0.03))",
  },
}));

interface ItemsMetadataProps {
  items: (string | undefined | null)[];
  maxItems?: number;
  variant?: "outlined" | "filled";
  color?: "default" | "primary";
}

export const ItemsMetadata = ({
  items,
  maxItems = 3,
  variant = "outlined",
  color = "default",
}: ItemsMetadataProps): ReactElement => (
  <Grid size={12}>
    <Stack direction="row" spacing={1} flexWrap="wrap" alignItems="center">
      {items.slice(0, maxItems).map((item, index) => (
        <Chip
          key={index}
          label={item}
          variant={variant}
          color={color}
          size="small"
          sx={{ my: "4px !important" }}
        />
      ))}
      {items.length > maxItems && (
        <MissingItemsTooltip
          title={
            <MissingItems
              missingItems={items.slice(maxItems)}
              variant={variant}
              color={color}
            />
          }
          arrow
          placement="top"
        >
          <Chip
            label="..."
            variant={variant}
            color={color}
            size="small"
            sx={{ my: "4px !important" }}
          />
        </MissingItemsTooltip>
      )}
    </Stack>
  </Grid>
);

interface MissingItemsProps {
  missingItems: (string | undefined | null)[];
  variant?: "outlined" | "filled";
  color?: "default" | "primary";
}

const MissingItems = ({
  missingItems,
  variant,
  color,
}: MissingItemsProps): ReactElement => (
  <Stack direction="row" spacing={1} flexWrap="wrap" alignItems="center">
    {missingItems.map((missingItem, index) => (
      <Chip
        key={index}
        label={missingItem}
        variant={variant}
        color={color}
        size="small"
        sx={{ my: "4px !important" }}
      />
    ))}
  </Stack>
);

export default ItemCardLayout;
