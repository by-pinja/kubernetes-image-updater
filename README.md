[![Docker pulls](https://img.shields.io/docker/pulls/ptcos/kubernetes-image-updater.svg)](https://hub.docker.com/r/ptcos/kubernetes-image-updater/)
[![Build Status](https://jenkins.protacon.cloud/buildStatus/icon?job=www.github.com/kubernetes-image-updater/master)](https://jenkins.protacon.cloud/job/www.github.com/job/kubernetes-image-updater/job/master/)

# kubernetes-image-updater

When CI finishes build of new container, test environment(s) should update automatically.
However there isn't out of box support for this kind of feature in either kubernetes or
any common registery. It's common to update test environments directly from build scripts
with command like `kubectl set image deployment/image-$branch image-$branch=$published.image:$published.tag --namespace=something`.

However this raises many issues:

- When multiple projects use same container (which is common case) i
  need to reconfigure existing CI pipeline for shared container to update everything.
- What happens if you delete old project namespace from testing environment? Build will fail...
- Unnecessary boilerplate.

## How it works

When event about updated image arrives, scan current cluster through for
matching images and if any is found, then update image to current version.

Service gives http callback apis which is easy to integrate most systems
via hooks. Theres also possibility to integrate this directly GCR registry
with pub events and functions, however we don't use them so there are no
'ready to use' example available.

## Running in kubernetes

Software uses kuberetes 'InClusterConfiguration' model and has acces for it's
current cluster.

```yaml
apiVersion: v1
kind: Deployment
metadata:
  name: image-updater
  namespace: your-namespace
  labels:
    app: image-updater
spec:
  replicas: 1
  template:
    metadata:
      labels:
        name: image-updater
    spec:
      containers:
      - image: ptcos/kubernetes-image-updater:latest
        imagePullPolicy: Always
        name: image-updater
```

# Setting up development environment

Open kubectl proxy to connect targeted testing cluster.

```bash
kubectl proxy

```

Run application with development configuration.

```bash
dotnet run --environment=Development
```

Navigate to `http://localhost:5000/doc/`.
