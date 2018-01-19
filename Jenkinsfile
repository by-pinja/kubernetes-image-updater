@Library("PTCSLibrary@1.0.3") _

// Podtemplate and node must match, dont use generic names like 'node', use more specific like projectname or node + excact version number.
// This is because CI environment reuses templates based on naming, if you create node 7 environment with name 'node', following node 8 environment
// builds may fail because they reuse same environment if label matches existing.
podTemplate(label: 'kubernetes-image-updater',
  containers: [
    containerTemplate(name: 'dotnet', image: 'microsoft/aspnetcore-build:2', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'docker', image: 'ptcos/docker-client:1.1.32', alwaysPullImage: true, ttyEnabled: true, command: '/bin/sh -c', args: 'cat')
  ]
) {
    def project = 'kubernetes-image-updater'

    def branch = (env.BRANCH_NAME)
    def namespace = "kubernetes-image-updater-${branch}"

    node('kubernetes-image-updater') {
        stage('Checkout') {
            checkout_with_tags()
        }
        stage('Build') {
            container('dotnet') {
                sh """
                    dotnet publish -c release -o out
                """
            }
        }
        stage('Test') {
            container('dotnet') {
                sh """
                    dotnet test
                """
            }
        }
        stage('Package') {
            container('docker') {
                if(branch == 'master') {
                    def publishedApi = publishContainerToGcr(project, branch);
                }
            }
        }
    }
  }