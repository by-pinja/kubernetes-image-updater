FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY ./out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Updater.dll"]