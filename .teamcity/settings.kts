import jetbrains.buildServer.configs.kotlin.v2023_05.*
import jetbrains.buildServer.configs.kotlin.projectFeatures.v2023_05.*
import jetbrains.buildServer.configs.kotlin.vcs.v2023_05.*
import jetbrains.buildServer.configs.kotlin.triggers.v2023_05.*
import jetbrains.buildServer.configs.kotlin.buildSteps.v2023_05.*

version = "2025.03"

object PeopleApiCi : BuildType({
    name = "People API CI"

    vcs {
        root(DslContext.settingsRoot)
    }

    params {
        param("IMAGE_TAG", "%build.number%")
        param("IMAGE_NAME", "localhost:5000/people-api")
        param("FULL_IMAGE", "%IMAGE_NAME%:%IMAGE_TAG%")
    }

    steps {
        script {
            name = "Restore NuGet packages"
            scriptContent = """
                dotnet restore src/People.API/People.API.csproj
            """.trimIndent()
        }

        script {
            name = "Build solution"
            scriptContent = """
                dotnet build src/People.API/People.API.csproj --configuration Release
            """.trimIndent()
        }

        script {
            name = "Run unit tests"
            scriptContent = """
                dotnet test src/People.Tests/People.Tests.csproj --configuration Release --no-build --verbosity normal
            """.trimIndent()
        }

        script {
            name = "Build Docker image"
            scriptContent = """
                docker build -t people-api:%build.number% -t %FULL_IMAGE% .
            """.trimIndent()
        }

        script {
            name = "Push to local registry"
            scriptContent = """
                docker push %FULL_IMAGE%
            """.trimIndent()
        }

        script {
            name = "Extract image digest"
            scriptContent = """
                DIGEST=$(docker inspect --format='{{index .RepoDigests 0}}' "%FULL_IMAGE%")
                echo "$DIGEST" > image.digest
            """.trimIndent()
        }
    }

    artifactRules = "image.digest"

    features {
        dockerSupport {
            loginToRegistry = on {
                dockerRegistryId = "PROJECT_EXT_1"
            }
        }
    }

    triggers {
        vcs { }
    }
})

project {
    buildType(PeopleApiCi)

    features {
        dockerRegistry {
            id = "PROJECT_EXT_1"
            name = "Local Registry"
            url = "http://localhost:5000"
        }
    }
}
