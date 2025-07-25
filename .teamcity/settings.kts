import jetbrains.buildServer.configs.kotlin.*  
import jetbrains.buildServer.configs.kotlin.buildSteps.*  
import jetbrains.buildServer.configs.kotlin.triggers.*  
import jetbrains.buildServer.configs.kotlin.vcs.*  
import jetbrains.buildServer.configs.kotlin.buildFeatures.*  
import jetbrains.buildServer.configs.kotlin.projectFeatures.*

version = "2025.07"

project {
  // Project-level HTTP registry
  features {
    dockerRegistry {
      id                    = "LOCAL_REG"
      name                  = "Local Registry"
      url                   = "http://localhost:5000"
      allowUnsecureProtocol = true
    }
  }

  // Register the CI build
  buildType(PeopleApiCi)
}

object PeopleApiCi : BuildType({
  name = "People API CI"

  // VCS root pointing at the .teamcity folder itself
  vcs {
    root(DslContext.settingsRoot)
  }

  params {
    param("IMAGE_NAME",  "localhost:5000/people-api")
    param("IMAGE_TAG",   "%build.number%")
    param("FULL_IMAGE",  "%IMAGE_NAME%:%IMAGE_TAG%")
  }

  steps {
    script {
      name          = "Restore, Build & Test"
      scriptContent = """
        dotnet restore src/People.API/People.API.csproj
        dotnet build   src/People.API/People.API.csproj --configuration Release
        dotnet test    src/People.Tests/People.Tests.csproj --configuration Release --no-build --verbosity normal
      """.trimIndent()
    }

    script {
      name          = "Build Docker Image"
      scriptContent = """
        docker build -t people-api:%build.number% -t %FULL_IMAGE% .
      """.trimIndent()
    }

    script {
      name          = "Push to Registry"
      scriptContent = "docker push %FULL_IMAGE%"
    }

    script {
      name          = "Extract Image Digest"
      scriptContent = """
        #!/usr/bin/env bash
        docker inspect --format='{{index .RepoDigests 0}}' "%FULL_IMAGE%" > image.digest
      """.trimIndent()
    }
  }

  artifactRules = "image.digest"

  features {
    // Hook the build into your HTTP registry
    dockerSupport {
      loginToRegistry = on {
        dockerRegistryId = "LOCAL_REG"
      }
    }
  }

  triggers {
    vcs { }  // build on every VCS change
  }
})