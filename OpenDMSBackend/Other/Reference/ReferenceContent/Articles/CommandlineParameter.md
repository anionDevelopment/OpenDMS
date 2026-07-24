# Supported commandline-parameter

Currently the following commandline-parameter:

- `InitialAdminPassword`
- `InitialDatabaseType`
- `InitialDatabaseConnectionString`
- `InitialOCRDataServiceAddress`
- `InitialOCRDataServiceAPIKey`
- `InitialDomain`
- `InitialEnableEndpointAvailabilityCheckValue`
- `InitialEnableEndpointInitializationStateValue`
- `InitialEnableEndpointCurrentVersionValue`
- `InitialEnableEndpointShowAllEndpointsValue`
- `InitialEnableEndpointHealthCheckValue`
- `InitialEnableEndpointMetricsValue`

This arguments will be used when there is no persisted configuration-file and the configuration-file is generated.
This is typically the case on first run when the OpenDMS-backend will be started the first time in a new deployment-environment.
So these values can be used as "seed" to fill it directly in the correct fields in the configuration without the requirement to change the configuration later.
After the first run when the configuration is generated then these commandline-parameter will not be used anymore.

The values can be changed later in the configuration-file.
When doing that then the configuration-change will be applied after next restart of the OpenDMS-backend.

If `InitialAdminPassword` is not provided, then `admin` will be set as default password for the admin-user.

If `InitialDatabaseType` is not provided, then `Transient` will be used as default.
