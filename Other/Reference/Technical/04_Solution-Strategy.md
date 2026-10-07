# 4. Solution Strategy

## Compliance

This section contains explanations to compliance-related topics which mostly refers to the German [GoBD](https://ao.bundesfinanzministerium.de/ao/2023/Anhaenge/BMF-Schreiben-und-gleichlautende-Laendererlasse/Anhang-64/inhalt.html) but may be also applicable for the requirements of many other document-management-regulation-systems.

### Audit-log

Every change-operation writes an entry into the audit-log: adding a document, updating it, changing its title, its metadata-values or its tags, uploading a new version of it, renaming a container, moving a containee, granting and revoking permissions of a storage-location, and the soft- and the hard-deletion.
An entry records which user performed the operation - or that it was an automatic system-operation, which is the case for an import and for the scheduled hard-deletion - and which object was affected by it.
The audit-log is written as the file `Audit.log` in the log-folder of the application and is configured in the `AuditLogConfiguration`-section of the configuration-file, separately from the ordinary application-log.

The audit-log is not tamper-proof yet: it can currently be modified afterwards by anybody who can reach the file.
Making the log itself immutable is [issue 6](https://github.com/anionDevelopment/OpenDMS/issues/6).

Remark: See [the context- and scope-section](./03_Context-and-Scope.md) for information about how reliable this audit-log can be.

### Permissions

Every content-object (storage-location, folder and document) can have arbitrarily many moderators ("owners") and arbitrarily many users which were granted an explicit view- or edit-permission.
The permissions are inherited down the containment-hierarchy, so a moderator or a grant on a folder applies to its contents as well.
A read-operation requires the view-permission and a change-operation (which includes hard-delete, soft-delete and editing the lock-up period and the delete-deadline) requires the edit-permission.
Only a moderator of a content-object may add or remove a moderator of it and grant or revoke a view- or edit-permission for it; a container must always keep at least one moderator.
By default nobody may retrieve or change the content, not even an administrator: an administrator only has administrative access (managing users and their global roles) and is not automatically allowed to see or change documents.
Automatic system-operations (the import of documents and the scheduled hard-deletion) act without a requesting user and are exempt from these checks.

### Business-owner-group

Additionally every document carries the group-identifier of its business-owner (`GroupOfBusinessOwner`), which is set when the document is added and which the adapt-script of an import-definition can change.
This value is currently only stored and shown; it does not influence any permission-decision yet.
Restricting the change of the retention-dates and the hard-deletion to the members of that group is a planned refinement.

### Hard-delete and soft-delete

In OpenDMS you can hard-delete and soft-delete documents.

Soft-delete means a "logical delete".
A soft-delete marks the file as deleted but only in a visual way in the UI.
The document is still there.
Every user which has write-permission for a document is allowed to soft-delete a document.
If desired, a soft-deleted document can be marked as "not deleted" again.

When hard-deleting a document it will be erased.
The content of a document which is hard-deleted is finally deleted and can not be restored.
However the audit-log will still contain certain meta-data as trace so you can always tell when which document was hard-deleted.
The only second option for a hard-delete is by reaching the delete-deadline.

### Lock-up period

For every document in OpenDMS you can define a date before which the document must not be hard-deleted (`DeleteIsNotAllowedBefore`).
This lock is enforced for every caller, including the automatic housekeeping: a hard-deletion is refused as long as the lock-up period of an affected document has not ended.
It is not applied to the soft-delete, because a soft-delete only marks the document and does not remove anything.
A user which is allowed to change the document is allowed to edit the lock-up period of it.

### Delete-deadline

For every document in OpenDMS you can define a date after which OpenDMS ensures that the document will be hard-deleted (`MustBeHardDeletedAfter`).
A scheduled housekeeping-run of the management-background-service hard-deletes every document whose delete-deadline has been reached and logs that in the audit-log with a reason.

A user which is allowed to change the document is allowed to edit the delete-deadline of it.

## Automatic document-processing on import

When importing documents by a import-definition then it is possible to define a typescript-script to edit some basic settings like the filename or the business-owner of the document.
In this script you can use existing document-properties like the original filename or the import-timestamp.

## Preview

OpenDMS automatically generates a preview for any document which can be viewed in the Web-UI.

## OCR

To use OCR a OCR-service must be available.
OpenDMS then let the OCR-service analyze documents by that service.
OpenDMS does not contain an own OCR-service.
See [the OCR-article in the reference of the OpenDMSBackend-codeunit](https://github.com/anionDev/OpenDMS/blob/main/OpenDMSBackend/Other/Reference/ReferenceContent/Articles/OCR.md) for more information.

## Bulk editing

Editing several documents in one operation is not implemented yet.
Every change-operation of the api and of the web-UI works on a single document.
