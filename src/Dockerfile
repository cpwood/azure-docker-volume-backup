FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ARG DLL
ARG TARGETPLATFORM
RUN mkdir /volumes
COPY publish/$TARGETPLATFORM /app
WORKDIR /app
ENV LAUNCH_DLL=$DLL
ENTRYPOINT dotnet $LAUNCH_DLL