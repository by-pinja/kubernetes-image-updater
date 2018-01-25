# kubernetes-image-updater

Now theres support for events when new images are updated to GCR https://cloud.google.com/container-registry/docs/configuring-notifications 

This gives possibility to implement service that updates test cluster images to latest version when pushed, common solution is to add command to CI like `kubectl set image deployment/image-$branch image-$branch=$published.image:$published.tag --namespace=something`. However this is far from ideal.

- When multiple projects use same container (which is common case) i need to reconfigure existing CI pipeline for shared container to update everything. What happens if you delete old project namespace from testing environment? Build will fail...
- Unnecessary boilerplate.

## How

When event about updated image arrives, scan current cluster through for matching images and if any is found, then update image to current version.

This may require further checks like that only update images which uses latest tagging.
