import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.script

version = "2025.07"

project {
    buildType(BuildAndPushDocker)
}

object BuildAndPushDocker : BuildType({
    name = "Build & Push Docker Image"

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        script {
            name = "Restore, Build & Test"
            scriptContent = """
                dotnet restore
                dotnet build --configuration Release
                dotnet test --no-build --configuration Release
            """.trimIndent()
        }
        script {
            name = "Build Docker Image"
            scriptContent = """
                docker build -t localhost:5000/people-api:%build.number% -f Dockerfile .
            """.trimIndent()
        }
        script {
            name = "Push to Local Registry & Save Digest"
            scriptContent = """
                docker push localhost:5000/people-api:%build.number% | tee push.log
                docker inspect --format='{{index .RepoDigests 0}}' localhost:5000/people-api:%build.number% > image.digest
            """.trimIndent()
        }
    }

    artifacts {
        artifactRules = "image.digest"
    }
})
