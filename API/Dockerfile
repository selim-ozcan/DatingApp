FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app

# copy .csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build.
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "API.dll" ]

# önce sdk image'i kullandık çünkü dotnet restore ve dotnet publish komutlarını kullanmamız lazım.
# sonrasında aspnet image'i kullandık ve önceki image'de oluşturduğumuz prod. build'i buraya kopyaladık.
# ve entrypoint kullanarak, container çalıştırıldığında hangi komutun çağrılacağını belirttik.

