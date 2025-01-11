#!/bin/bash

{ cd /Workspace/Application/Backend && dotnet ./OpenDMSBackend.dll; } &
{ cd /Workspace/Application/Frontend && nginx -c /Workspace/Application/Frontend/nginx.configuration -g "daemon off;"; } &

wait -n

pkill -P $$
