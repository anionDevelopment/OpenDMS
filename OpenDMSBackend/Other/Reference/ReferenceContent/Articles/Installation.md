# Installation

The backend is not installed on its own. It is one part of the OpenDMS-container, into which the build-result of this codeunit is
copied, together with the web-frontend and the nginx which distinguishes the requests for the two (see the `Dockerfile` of the
codeunit `OpenDMS`). Installing OpenDMS therefore means running that container; see
[the usage-article of the codeunit OpenDMS](https://github.com/anionDev/OpenDMS/blob/main/OpenDMS/Other/Reference/ReferenceContent/Articles/Usage.md) for the
ports it offers and the reverse-proxy it expects in front of it, and
[the deployment-view](https://github.com/anionDev/OpenDMS/blob/main/Other/Reference/Technical/07_Deployment-View.md) for the infrastructure it needs.

On its first start the backend generates its configuration-file. The values which it is seeded with can be passed as
commandline-parameter, which the container takes as environment-variables of the same name; they are listed in
[the commandline-parameter-article](./CommandlineParameter.md). Which databases are supported is described in
[the supported-databases-article](./SupportedDatabases.md).

To run the backend alone on a development-machine, see the [hints](../Hints.md).
