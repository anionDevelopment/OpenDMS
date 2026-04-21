# 7. Deployment View

## Infrastructure-overview

OpenDMS will be deployed as OCI-container so the administrator needs a container-runtime to run OpenDMS.

## Infrastructure-requirements

OpenDMS is supposed to be run behind a reverse-proxy to protect certain maintenance-routes and to manage the TLS-overhead.

## Deployment-processes

### General

OpenDMS is supposed to be run only as container.
Updates will be applied by restarting the container with a newer image-version.

### Migrations

You do not have to care about internal data-migrations due to new versions.
Just run a newer version by using an updated image and then OpenDMS will run all required migrations.
