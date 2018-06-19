FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY ./out .

ARG KUBECTL_VERSION="1.7.2"

RUN apt-get update && \
    apt-get install -y curl openssh-client && \
    rm -rf /var/lib/apt/lists/* && \
    curl -L https://storage.googleapis.com/kubernetes-release/release/v${KUBECTL_VERSION}/bin/linux/amd64/kubectl -o /usr/local/bin/kubectl && \
    chmod -v +x /usr/local/bin/kubectl

EXPOSE 5000

ENTRYPOINT ["dotnet", "Updater.dll"]