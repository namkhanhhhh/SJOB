FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
EXPOSE 8080
EXPOSE 443

COPY certificate.pfx /app/certificate.pfx
RUN chmod 600 /app/certificate.pfx

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SJOB_EXE201.csproj", "./"]
RUN dotnet restore "SJOB_EXE201.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./SJOB_EXE201.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SJOB_EXE201.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SJOB_EXE201.dll"]
