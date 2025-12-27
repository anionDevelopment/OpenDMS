#!/bin/bash
set -o errexit
configfile="/Workspace/Configuration/Configuration.xml"
protocol=$(sed -n 's/.*exs:type="ns10:\(HTTP\|HTTPS\)".*/\1/p' $configfile)
protocol="${protocol,,}" # make protocol to lower
port=$(sed -n 's/.*Port="\([0-9]\+\)".*/\1/p' $configfile)
healthcheckcommand="curl --fail -v"
if [ "$protocol" == "https" ]; then
    healthcheckcommand="$healthcheckcommand --insecure"
fi
healthcheckcommand="$healthcheckcommand \"$protocol://127.0.0.1:$port/API/Other/Maintenance/HealthCheck\""
echo "health-check-command: $healthcheckcommand"
eval $healthcheckcommand
