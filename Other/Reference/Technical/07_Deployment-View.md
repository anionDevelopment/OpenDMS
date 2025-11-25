# 7. Deployment View

## Infrastructure-overview

TODO

## Infrastructure-requirements

OpenDMS is supposed to be run behind a reverseproxy to protect certain maintenance-routes and to manage the TLS-overhead.

## Deployment-proecsses

### General

OpenDMS is supposed to be run as container.

### Migrations

You do not have to care about internal data-migrations due to new versions.
Just run a newer version by using an updated image and then OpenDMS will run all required migrations.
