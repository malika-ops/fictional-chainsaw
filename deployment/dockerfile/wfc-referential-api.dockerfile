# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Install OpenSSL, CA certificates, and ICU libraries
RUN apt-get update && \
    apt-get install -y \
        openssl \
        ca-certificates \
        libgdiplus \
        libc6-dev \
        curl \
        unzip \
        libicu-dev \
        --no-install-recommends && \
    fc-cache -f -v && \
    rm -rf /var/lib/apt/lists/*
    
# Copy published application 
COPY ./src/Output/wfc.referential.API/ .

# Create logs directory
RUN mkdir -p /app/logs && chmod 777 /app/logs

# For debug only
RUN echo "Listing files in /app:" && ls -lat /app
RUN echo "Listing certificate files in /app:" && ls -lat /usr/local/share/ca-certificates/

ENTRYPOINT ["dotnet", "wfc.referential.API.dll"]