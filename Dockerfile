#Custom image so that monogame content is able to load fonts :))))))
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN apt-get update
RUN apt-get install libfreetype6 zip -y
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
