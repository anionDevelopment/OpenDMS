# Minimal docker-compose-file

## Start

Run `task BaseExampleStart` from the repository-base-folder.

Optionally you can create a `Variables.env` in this folder to add some configuration-values.

Example-content for a valid `Variables.env`-file:

```bash
InitialAdminPassword=Adm1npa55w0rd
```

## Access

### Database

connectionstring: `postgresql://root:R00tpa55w0rd@opendms_database:5432/OpenDMSDatabase`

Data for adminer:

System: `PostgreSQL`

Server: `opendms_database`

Username: `root`

Password: `R00tpa55w0rd`

Database: `OpenDMSDatabase`
