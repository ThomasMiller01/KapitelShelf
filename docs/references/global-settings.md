# Global Settings

Global settings affect the entire KapitelShelf instance rather than a single user.

## Cloud Storage

| Category      | Setting                | Default | Description                                                                 | Note                                                                              |
| ------------- | ---------------------- | ------- | --------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| Cloud Storage | Enable `rclone bisync` | `false` | Allows two-way synchronization between KapitelShelf and the cloud provider. | _Experimental_, when disabled, `rclone sync` for one-way synchronisation is used. |
