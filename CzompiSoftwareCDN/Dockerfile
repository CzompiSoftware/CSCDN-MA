FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

ENV \
    # CSCDN-MA version
    CSCDNMA_VERSION=1.0.0-build10

VOLUME [ "/data" ]


# Download CSCDN-MA binaries
RUN wget https://packages.czompi.cloud/products/czsoft/cscdn-ma/$CSCDNMA_VERSION.tar.gz -O cscdn-ma.tar.gz \
    && tar -ozxf cscdn-ma.tar.gz -C /bin \
    && rm cscdn-ma.tar.gz
WORKDIR /bin
EXPOSE 80
ENTRYPOINT ["dotnet", "CSCDN-MA.dll"]