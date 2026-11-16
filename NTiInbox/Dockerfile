# Build stage: compiles the application with the .NET 10 SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish NTiInbox.csproj -c Release -o /app

# Run stage: the smallest official ASP.NET image, running as a non-root user.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Connection profiles and data protection keys live here.
# Mount a volume to keep them between restarts.
RUN mkdir App_Data && chown $APP_UID App_Data
VOLUME /app/App_Data

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "NTiInbox.dll"]
