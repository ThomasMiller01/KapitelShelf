import MailIcon from "@mui/icons-material/Mail";
import InboxIcon from "@mui/icons-material/MoveToInbox";
import {
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import type { ReactElement } from "react";

import { ResponsiveDrawer } from "./base/ResponsiveDrawer";

interface SidebarProps {
  open: boolean;
  mobile: boolean;
  onClose: () => void;
}

export const Sidebar = ({
  open,
  onClose,
  mobile,
}: SidebarProps): ReactElement => (
  <ResponsiveDrawer
    open={open}
    mobile={mobile}
    onClose={onClose}
    name="KapitelShelf"
    logo="/kapitelshelf.png"
  >
    <List>
      {["Inbox", "Starred", "Send email", "Drafts"].map((text, index) => (
        <ListItem key={text} disablePadding>
          <ListItemButton>
            <ListItemIcon>
              {index % 2 === 0 ? <InboxIcon /> : <MailIcon />}
            </ListItemIcon>
            <ListItemText primary={text} />
          </ListItemButton>
        </ListItem>
      ))}
    </List>
  </ResponsiveDrawer>
);
