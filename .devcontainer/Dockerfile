FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim

RUN apt-get update && apt-get install -y \
    git \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install nats-server
ARG NATS_VERSION=v2.12.4
RUN curl -sL "https://github.com/nats-io/nats-server/releases/download/${NATS_VERSION}/nats-server-${NATS_VERSION}-linux-amd64.tar.gz" \
    | tar xz -C /tmp \
    && cp /tmp/nats-server-${NATS_VERSION}-linux-amd64/nats-server /usr/local/bin/ \
    && rm -rf /tmp/nats-server-*
