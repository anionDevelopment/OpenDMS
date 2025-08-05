# 4. Solution Strategy

## Compliance

(This section contains explanations to compliance-related topics which mostly refers to the German [GoBD](https://ao.bundesfinanzministerium.de/ao/2023/Anhaenge/BMF-Schreiben-und-gleichlautende-Laendererlasse/Anhang-64/inhalt.html).)

### Audit-log

TODO

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
In OpenDMS every document has a clearly defined usergroup which represents the business-owner.
Only user which are business-owner of the document are allowed to hard-delete a document.
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

TODO

## OCR

TODO

## Bulk editing

TODO
