FROM debian:stable-slim

RUN mkdir /Workspace && \
    mkdir /Workspace/Application && \
    mkdir /Workspace/Application/Backend && \
    mkdir /Workspace/Application/Frontend && \
    mkdir /Workspace/Application/Frontend/WebApplicationFiles && \
    mkdir /Workspace/Configuration && \
    mkdir /Workspace/Logs && \
    mkdir /Workspace/Other && \
    mkdir /Workspace/Other/EntryPoint && \
    apt-get update && \
    apt-get install -y wget iputils-ping curl procps nginx

WORKDIR /Workspace/Other/EntryPoint

#Install .NET and ffmpeg
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-9.0 ffmpeg

ADD Other/Resources/DependentCodeUnits/OpenDMSBackend/BuildResult_DotNet_win-x64 /Workspace/Application/Backend

ADD Other/Resources/DependentCodeUnits/OpenDMSFrontend/BuildResult_WebApplication /Workspace/Application/Frontend/WebApplicationFiles
COPY OpenDMS/nginx.configuration /Workspace/Application/Frontend

COPY OpenDMS/EntryPoint.sh /Workspace/Other/EntryPoint
RUN chmod +x /Workspace/Other/EntryPoint/EntryPoint.sh

HEALTHCHECK CMD curl --insecure --fail https://localhost:443/Backend/API/Other/Maintenance/HealthCheck
#TODO add frontend to healthcheck

ENTRYPOINT ["./EntryPoint.sh"]
