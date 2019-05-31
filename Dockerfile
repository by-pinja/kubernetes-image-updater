FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder
WORKDIR /src
COPY ./ ./
RUN dotnet publish -c Release -o /out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder ./out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Updater.dll"]