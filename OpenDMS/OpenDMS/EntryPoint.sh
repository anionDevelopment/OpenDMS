#!/bin/bash
export IsRunningInDockerContainer=true

argument="--RealRun"

if [[ -n "${InitialAdminPassword}" ]]; then
  argument+=" --InitialAdminPassword $InitialAdminPassword"
fi

if [[ -n "${InitialDatabaseType}" ]]; then
  argument+=" --InitialDatabaseType $InitialDatabaseType"
fi

if [[ -n "${InitialDatabaseConnectionString}" ]]; then
  argument+=" --InitialDatabaseConnectionString $InitialDatabaseConnectionString"
fi

argument+=" --InitialOCRDataFolder /Workspace/OCRData"

{ cd /Workspace/Application/Backend && dotnet ./OpenDMSBackend.dll $argument; } &
{ cd /Workspace/Application/Frontend && nginx -c /Workspace/Application/Frontend/nginx.conf -g "daemon off;"; } &

wait -n

pkill -P $$
