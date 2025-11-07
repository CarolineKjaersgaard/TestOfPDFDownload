# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . .
COPY PDFDownload/ListFolder/GRI_2017_2020.xlsx PDFDownload/ListFolder/
COPY PDFDownload/Output/ PDFDownload/Output/


RUN dotnet restore PDFDownload/PDFDownload.csproj
RUN dotnet build PDFDownload/PDFDownload.csproj --configuration Release --no-restore
RUN dotnet test Test/Test.csproj --no-build --verbosity normal
RUN dotnet publish PDFDownload/PDFDownload.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "PDFDownload.dll"]