[![Docker pulls](https://img.shields.io/docker/pulls/ptcos/kubernetes-image-updater.svg)](https://hub.docker.com/r/ptcos/kubernetes-image-updater/)
[![Build Status](https://jenkins.protacon.cloud/buildStatus/icon?job=www.github.com/kubernetes-image-updater/master)](https://jenkins.protacon.cloud/job/www.github.com/job/kubernetes-image-updater/job/master/)
# kubernetes-image-updater
When CI finishes build of new container, test environment(s) should update automatically. However there isn't out of box support for this kind of feature in either kubernetes or any common registery. It's common to update test environments directly from build scripts with command like `kubectl set image deployment/image-$branch image-$branch=$published.image:$published.tag --namespace=something`.

However this raises many issues:

- When multiple projects use same container (which is common case) i need to reconfigure existing CI pipeline for shared container to update everything.
- What happens if you delete old project namespace from testing environment? Build will fail...
- Unnecessary boilerplate.

## How it works
When event about updated image arrives, scan current cluster through for matching images and if any is found, then update image to current version.

On real installations we strongly recommended by using service with google service account instead of personal accounts ...

Service gives http callback apis which is easy to integrate most systems via hooks. Theres also possibility to integrate this directly GCR registry with pub events and functions, however we don't use them so there are no 'ready to use' example available.

# Running service
Behind scenes kubectl command line tool is used and it needs context and credentials for targeted environment. Basically it needs same kubernetes 'config' file you use when you run commands via kubernetes tooling.

## In linux
```bash
docker run -p 5000:5000 -v /home/yourhome/.kube/:/root/.kube/ -it ptcos/kubernetes-image-updater
```

Then navigate to `http://localhost:5000/doc/`.

## In windows
```bash
docker run -p 5000:5000 -v C:\users\YOUR_HOME\.kube\:/root/.kube/ -it ptcos/kubernetes-image-updater
```

Then navigate to `http://localhost:5000/doc/`.

## In kubernetes
```yaml
apiVersion: v1
kind: Deployment
apiVersion: extensions/v1beta1
metadata:
  name: image-updater
  namespace: you-namespace
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
        volumeMounts:
        - name: config-volume
          mountPath: /root/.kube/
      volumes:
      - name: config-volume
        configMap:
          name: image-updater-kubectl
          items:
          - key: config
            path: config
```

You must put your kubernetes config file to `image-updater-kubectl` config map.

# Setting up development environment
```bash
dotnet restore
dotnet run
```

Navigate to `http://localhost:5000/doc/`.

