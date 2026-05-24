# 3. Context and Scope

## Context

OpenDMS as document-management-system should be usable for everyone and every organisation in a self-hosted way.

## Scope

### Deployment

The release-artifact is a deployable only as OCI-image.
The user must deploy OpenDMS on his own system.

There is no official cloud-service which provides OpenDMS.

OpenDMS does not provide any TLS-certificate.
The user must obtain his own domain and TLS-certificates.

### Audit-log

The scope of the reliability of the audit-log is the set of requests to access documents in any kind through OpenDMSs defined communication-interfaces (means: through the Rest-API-endpoints).
OpenDMS can only control when it is used through these communication-interfaces.
Like **every** other system in general too OpenDMS can not prevent changes on the underlying systems.
So the Audit-log can not write log-entries if someone is able to manipulate the database or the filesystem because he has access to it in another way than using OpenDMSs communication-interfaces.
If someone has this access you can also not assume anymore that the audit-log is not manipulated.
For this reason it is the job of the technical administrator-team to ensure that no one is able to directly access the underlying systems which are used by OpenDMS except the administrators itself.
And of course - again: it is like in **every** other system in general too - you have to trust your administrator that he is not doing bad things.
No IT-system can prevent bad things done by its administrator and OpenDMS is no exception from this rule.
