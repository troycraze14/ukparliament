import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnet.*
import jetbrains.buildServer.configs.kotlin.buildSteps.dockerCommand.*
import jetbrains.buildServer.configs.kotlin.buildSteps.script

version = "2025.07"

object PeopleApiCi : BuildType({
  name = "People API CI"

  vcs {
    root(DslContext.settingsRoot)
  }

  steps {
    dotnetRestore {
      name     = "Restore NuGet packages"
      projects = "**/*.sln"
    }
    dotnetBuild {
      name         = "Build solution"
      projects     = "**/*.sln"
      configuration = "Release"
    }
    dotnetTest {
      name         = "Run unit tests"
      projects     = "**/*Tests/*.csproj"
      configuration = "Release"
      args         = "--no-build --verbosity normal"
    }

    dockerCommand {
      name          = "Build Docker image"
      imagePlatform = Linux
      commandType   = build {
        source = path { path = "." }
        namesAndTags = "people-api:%build.number%,localhost:5000/people-api:%build.number%"
      }
    }

    dockerCommand {
      name          = "Push to local registry"
      imagePlatform = Linux
      commandType   = push {
        namesAndTags = "localhost:5000/people-api:%build.number%"
      }
    }

    script {
      name = "Extract image.digest"
      scriptContent = """
        #!/usr/bin/env bash
        IMAGE="localhost:5000/people-api:%build.number%"
        DIGEST=$(docker inspect --format='{{index .RepoDigests 0}}' "$IMAGE")
        echo "$DIGEST" > image.digest
      """.trimIndent()
    }
  }

  artifactRules = "image.digest"

  features {
    feature {
      type = "DockerSupport"
      param("teamcity.docker.registry.0.host",          "localhost:5000")
      param("teamcity.docker.registry.0.protocol",      "http")
      param("teamcity.docker.registry.0.allowUnsecure", "true")
    }
  }

  triggers {
    vcs { }
  }
})