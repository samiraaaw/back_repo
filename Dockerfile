# Etapa base con SDK de .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

# Establecer el directorio de trabajo dentro del contenedor
WORKDIR /app 

# Copiar solo el archivo .csproj al principio (para aprovechar cache de Docker)
COPY DICREP.EcommerceSubastas.API/*.csproj ./DICREP.EcommerceSubastas.API/

# Restaurar dependencias
RUN dotnet restore ./DICREP.EcommerceSubastas.API/DICREP.EcommerceSubastas.API.csproj

# Copiar todo el código del proyecto (una vez restaurado)
COPY . ./

# Establecer el directorio de trabajo dentro del proyecto principal
WORKDIR /app/DICREP.EcommerceSubastas.API

# Exponer el puerto de la API
EXPOSE 5096

# Comando para correr la API en modo desarrollo con hot reload
CMD ["dotnet", "watch", "run", "--urls=http://0.0.0.0:5096"]

