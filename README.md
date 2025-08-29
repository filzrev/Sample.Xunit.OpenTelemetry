# Sample.Xunit.OpenTelemetry

This repository contains sample project to integrate OpenTelemetry Trace/Log functionality with xUnit.net.

## How to test OpenTelemetry

1. Launch Aspire standalone dash board container image with following commands.
    > docker run --rm -it -p 18888:18888 -p 4317:18889 -d --name aspire-dashboard -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true mcr.microsoft.com/dotnet/aspire-dashboard:9.4
2. Run `Sample.Xunit.OpenTelemetry` project's tests as exe or Test Explorer.
3. Confirm OpenTelemetry Trace/Logs by using Aspire Dashbord with following URL.
    > http://localhost:18888
