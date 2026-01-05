# 4. Solution Strategy

## Compliance

This section contains explanations to compliance-related topics which mostly refers to the German [GoBD](https://ao.bundesfinanzministerium.de/ao/2023/Anhaenge/BMF-Schreiben-und-gleichlautende-Laendererlasse/Anhang-64/inhalt.html) but may be also applicable for the requirements of many other document-management-regulation-systems.

### Audit-log

TODO

Remark: See [The context- and scope-section](./04_Solution-Strategy.md) for information about how reliable this audit-log can be. 

### Business-owner

In OpenDMS every document has a clearly defined usergroup which represents the business-owner.
Only user which are business-owner of a document are allowed to hard-delete a document.
Only user which are business-owner of a document are allowed to add/change/remove permissions to read/edit/soft-delete a document.

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

For every document in OpenDMS you can define define a date where it is not possible to delete the document before this date.
This date will be applied for hard- and soft-delete.
Only a user which is business-owner of the document is allowed to edit the lock-up period of a document.

### Delete-deadline

For every document in OpenDMS you can define define a date where OpenDMS ensures that the document will be hard-deleted immediately after this date.

Only a user which is business-owner of the document is allowed to edit the delete-deadline of a document.

## Automatic document-processing on import

When importing documenty by a import-definition then it is possible to define a typescript-script to edit some basic settings like the filename or the business-owner of the document.
In this script you can use existing document-properties like the original filename or the import-timestamp.

## Preview

OpenDMS automatically generates a preview for any document which can be viewed in the Web-UI.

## OCR

To use OCR a OCR-service must be available.
OpenDMS then let the OCR-service analyze documents by that service.
OpenDMS does not contain an own OCR-service.
See [the OCR-article in the OCRBackend-reference](https://github.com/anionDev/OpenDMS/blob/main/OpenDMSBackend/Other/Reference/ReferenceContent/articles/OCR.md) for more information.

## Bulk editing

TODO
