# ================================
# -------- Development Stage ---------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
ARG PARENT_VERSION=dotnet10.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}

WORKDIR /home/dotnet/src

# Build context is repository root; copy the BatchJobs solution tree plus Apha.Common,
# which Apha.BatchJobs.csproj references via a sibling ProjectReference
# (..\Apha.Common\Apha.Common.csproj, i.e. /home/dotnet/Apha.Common relative to here).
COPY --chown=root:dotnet --chmod=640 src/Apha.BatchJobs/ ./
COPY --chown=root:dotnet --chmod=640 src/Apha.Common/ ../Apha.Common/
USER root
RUN find . ../Apha.Common -type d -exec chmod g+wx {} \;
USER dotnet

# Restore, build and publish the worker project.
RUN dotnet restore ./Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
RUN dotnet publish ./Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj -c Release -o /home/dotnet/out /p:UseAppHost=false

# ================================
# -------- Production Stage --------
# ================================
ARG PARENT_VERSION=dotnet10.0
FROM defradigital/dotnetcore:$PARENT_VERSION AS production
ARG PARENT_VERSION=dotnet10.0
LABEL uk.gov.defra.parent-image=defra-dotnetcore:${PARENT_VERSION}

USER 0
RUN apk update && apk add --no-cache icu-libs krb5-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=1

WORKDIR /app

# Copy published output as root.
COPY --from=development /home/dotnet/out/ ./

# Files: read only, Dirs: read+execute only.
RUN find . -type f -exec chmod 444 {} \; && \
    find . -type d -exec chmod 555 {} \;

USER app

ENTRYPOINT ["dotnet", "Apha.BatchJobs.Worker.dll"]