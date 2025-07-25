import jetbrains.buildServer.configs.kotlin.v2025_03.*                  // core types
import jetbrains.buildServer.configs.kotlin.v2025_03.buildSteps.*       // script, dotnet, dockerCommand if you need them
import jetbrains.buildServer.configs.kotlin.v2025_03.triggers.*         // vcs trigger
import jetbrains.buildServer.configs.kotlin.v2025_03.vcs.*              // vcsRoot helpers
import jetbrains.buildServer.configs.kotlin.v2025_03.buildFeatures.*    // dockerSupport, etc.
import jetbrains.buildServer.configs.kotlin.v2025_03.projectFeatures.*  // dockerRegistry

version = "2025.03"

project {
  // register your CI build
  buildType(PeopleApiCi)

  // declare the HTTP registry as a project-level feature
  features {
    dockerRegistry {
      id                    = "PROJECT_EXT_1"
      name                  = "Local Registry"
      url                   = "http://localhost:5000"
      allowUnsecureProtocol = true
    }
  }
}

object PeopleApiCi : BuildType({
  name = "People API CI"

  // hook up your VCS root (the .teamcity dir itself)
  vcs {
    root(DslContext.settingsRoot)
  }

  // convenience params for re-use
  params {
    param("IMAGE_TAG",  "%build.number%")
    param("IMAGE_NAME", "localhost:5000/people-api")
    param("FULL_IMAGE","%IMAGE_NAME%:%IMAGE_TAG%")
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
        dotnet test src/People.Tests/People.Tests.csproj \
          --configuration Release --no-build --verbosity normal
      """.trimIndent()
    }

    script {
      name = "Build Docker image"
      scriptContent = """
        docker build \
          -t people-api:%build.number% \
          -t %FULL_IMAGE% \
          .
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
        #!/usr/bin/env bash
        DIGEST=$(docker inspect --format='{{index .RepoDigests 0}}' "%FULL_IMAGE%")
        echo "$DIGEST" > image.digest
      """.trimIndent()
    }
  }

  // expose the digest for downstream usage
  artifactRules = "image.digest"

  // tell the agent to use your project-level registry
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