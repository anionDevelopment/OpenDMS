# Supported databases

Currently the following databases are supported:

- MariaDB
- PostgreSQL
- Transient

"Transient" is a kind of persistence for testing- and demonstration-purposes.
There are usually 2 usecases for that:

- Accelerate testcases which need a persistence but not necessarily a database-persistence.
- Running the backend without the requirement of having a database.

From the perspective of the usage (as a backend-developer) Transient is similar to an in-memory-database which is internally included in the backend and does not have any third-party-dependencies.
The persisted data will always be removed when the backend will get stopped when using the transient-persistence.
So Transient is not appropriate for productive-usage for obviousreasons.
