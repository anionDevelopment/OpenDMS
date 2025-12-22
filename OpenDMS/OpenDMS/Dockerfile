FROM debian:stable-slim
ENV ISRUNNINGINCONTAINER="true"

RUN mkdir /Workspace && \
    mkdir /Workspace/Application && \
    mkdir /Workspace/Application/Backend && \
    mkdir /Workspace/Application/Frontend && \
    mkdir /Workspace/Application/Frontend/WebApplicationFiles && \
    mkdir /Workspace/Configuration && \
    mkdir /Workspace/Logs && \
    mkdir /Workspace/Other && \
    mkdir /Workspace/Other/Certificates && \
    mkdir /Workspace/Other/EntryPoint && \
    apt-get update && \
    apt-get install -y curl nginx git libreoffice-core libreoffice-writer libreoffice-calc wget

WORKDIR /Workspace/Other/EntryPoint

RUN wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y aspnetcore-runtime-10.0

RUN rm -rf /var/lib/apt/lists/*

ADD Other/Resources/DependentCodeUnits/OpenDMSBackend/BuildResult_DotNet_linux-x64 /Workspace/Application/Backend
ADD Other/Resources/DependentCodeUnits/OpenDMSFrontend/BuildResult_WebApplication /Workspace/Application/Frontend/WebApplicationFiles
COPY Other/Resources/DevelopmentCertificate/OpenDMSDevelopmentCertificate.crt /Workspace/Other/Certificates
COPY Other/Resources/DevelopmentCertificate/OpenDMSDevelopmentCertificate.key /Workspace/Other/Certificates
COPY OpenDMS/nginx.conf /Workspace/Application/Frontend

COPY OpenDMS/EntryPoint.sh /Workspace/Other/EntryPoint
RUN chmod +x /Workspace/Other/EntryPoint/EntryPoint.sh
COPY OpenDMS/HealthCheck.sh /Workspace/Other/EntryPoint
RUN chmod +x /Workspace/Other/EntryPoint/HealthCheck.sh

HEALTHCHECK CMD ./HealthCheck.sh

ENTRYPOINT ["./EntryPoint.sh"]
