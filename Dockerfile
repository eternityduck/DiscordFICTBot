
FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

COPY ./*.sln ./nuget.config  ./

# Copy the main source project files
COPY ./src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

################################################################
# change the WORKDIR to the csproj here if you want
# else that restore will restore all the solution
################################################################
RUN dotnet restore

################################################################
# if you changed the WORKDIR go back the the sln one
# WORKDIR /app
################################################################
# copy everything else and build
COPY . ./

# use dotnet build if you want

RUN dotnet publish ./src/the.actual.project.csproj -c release -o /app/publish/the.actual.project


