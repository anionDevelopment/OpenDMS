# Supported commandline-parameter

Currently the following commandline-parameter:

- `InitialAdminPassword`
- `InitialDatabaseType`
- `InitialDatabaseConnectionString`
- `OCRDataFolder`

This arguments will be used when there is no persisted configuration-file where the values can be loaded from.
This is typically the case on first run when the configuration is generated.
So these values can be used as "seed" to fill it directly in the correct fields in the configuration without the requirement to change the configuration later.
After the first run when the configuration is generated then these commandline-parameter will not be used anymore.

If `InitialAdminPassword` is not provided, then `admin` will be set as default password for the admin-user.

If `InitialDatabaseType` is not provided, then a transient memory-storage will be used instead.
