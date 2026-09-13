# Builds the React frontend and the .NET backend into a single image — the backend serves the
# built frontend as static files (see Program.cs), so the whole app is one deployable unit with
# one URL. See docs/DEPLOYMENT.md for how this gets deployed and configured.

FROM node:22-alpine AS frontend-build
WORKDIR /src/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src/backend
COPY backend/ ./
RUN dotnet publish src/HarnasHub.Api/HarnasHub.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HarnasHub.Api.dll"]
