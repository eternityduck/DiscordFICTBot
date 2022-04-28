FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY ./DS_Bot/*.sln ./
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DS_Bot/DS_Bot.csproj", "DS_Bot/"]

RUN dotnet restore "DS_Bot/DS_Bot.csproj"
COPY . .
WORKDIR "/src/DS_Bot"
RUN dotnet build "DS_Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DS_Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app`enter code here`
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DS_Bot.dll"]




# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
# WORKDIR /src
# COPY ["DS_Bot/DS_Bot.csproj", "DS_Bot/"]
# RUN dotnet restore "DS_Bot/DS_Bot.csproj"
# COPY . ./
# RUN dotnet publish -c Release -o out 
# FROM mcr.microsoft.com/dotnet/sdk:6.0
# WORKDIR /app
# COPY --from=build-env /app/out .
# ENTRYPOINT ["dotnet", "DS_Bot.dll"]
