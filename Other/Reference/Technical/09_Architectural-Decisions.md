# 9. Architectural Decisions

## Decision-board

| Decision-identifier | Date | Decision | Reason and notes |
| ------------------- | ---- | -------- | ---------------- |
| D001 | 2024-11-12 | OpenDMS should be hostable on premises. | Base requirement for many user. |
| D002 | 2024-11-12 | C# for backend | Appropriate for usecase. |
| D003 | 2024-11-12 | Angular for web-frontend | Appropriate for usecase. |
| D004 | 2024-11-12 | Added support for MariaDB as database | Reasons:<br>- Open source.<br>- Free for private user. |
| D005 | 2025-07-04 | Added support for PostgreSQL as database | Reasons:<br>- MariaDB is not stable enough on all systems<br>- 10 years after opening the [ticket](https://jira.mariadb.org/browse/MDEV-4259), MariaDB does still not support transactional DDL-operations. |
| D005 | 2025-08-06 | No dedicated codeunit for an app-client | Web-Frontend-client is supposed to be usable also on mobile devices. If there are more requirements which justify the effort then an app-client can still be developed. |
