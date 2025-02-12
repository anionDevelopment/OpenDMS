#!/bin/bash
export IsRunningInDockerContainer=true

{ cd /Workspace/Application/Backend && dotnet ./OpenDMSBackend.dll; } &
{ cd /Workspace/Application/Frontend && nginx -c /Workspace/Application/Frontend/nginx.conf -g "daemon off;"; } &

wait -n

pkill -P $$
