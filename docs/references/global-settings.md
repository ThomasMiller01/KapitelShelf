# Global Settings

Global settings affect the entire KapitelShelf instance rather than a single user.

## Cloud Storage

| Setting                | Default | Description |
| ---------------------- | ------- | ----------- |
| Enable `rclone bisync` | `false` | Allows two-way synchronisation between KapitelShelf and the cloud. |

> _Experimental:_ when disabled, KapitelShelf uses `rclone sync` for uni-directional imports.
