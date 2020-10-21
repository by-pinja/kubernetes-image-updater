library 'jenkins-ptcs-library@3.0.0'

// Podtemplate and node must match, dont use generic names like 'node', use more specific like projectname or node + excact version number.
// This is because CI environment reuses templates based on naming, if you create node 7 environment with name 'node', following node 8 environment
// builds may fail because they reuse same environment if label matches existing.
podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'microsoft/dotnet:2.2-sdk', ttyEnabled: true, command: '/bin/sh -c', args: 'cat')
  ]
) {
    node(pod.label) {
        stage('Checkout') {
            checkout scm
        }
        stage('Test') {
            container('dotnet') {
                sh """
                    dotnet test
                """
            }
        }
        stage('Build Image') {
            publishContainerToGcr("kubernetes-image-updater");
            publishTagToDockerhub("kubernetes-image-updater")
        }
    }
  }
