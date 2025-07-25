import jetbrains.buildServer.configs.kotlin.v2021_2.*
import jetbrains.buildServer.configs.kotlin.v2021_2.buildSteps.dotnet.*
import jetbrains.buildServer.configs.kotlin.v2021_2.buildSteps.dockerCommand.*
import jetbrains.buildServer.configs.kotlin.v2021_2.buildSteps.script

object PeopleApiCi : BuildType({
  name = "People API CI"

  // 1. VCS Root (assumes settings.kts sits at repo root)
  vcs {
    root(DslContext.settingsRoot)
  }

  // 2. .NET Restore / Build / Test
  steps {
    dotnetRestore {
      name     = "Restore NuGet packages"
      projects = "**/*.sln"
    }
    dotnetBuild {
      name       = "Build solution"
      projects   = "**/*.sln"
      configuration = "Release"
    }
    dotnetTest {
      name       = "Run unit tests"
      projects   = "**/*Tests/*.csproj"
      configuration = "Release"
      args       = "--no-build --verbosity normal"
    }

    // 3. Build Docker image with two tags: local and registry
    dockerCommand {
      name          = "Build Docker image"
      imagePlatform = Linux
      commandType   = build {
        source = path { path = "." }
        namesAndTags = "people-api:%build.number%, localhost:5000/people-api:%build.number%"
      }
    }

    // 4. Push only the registry-tagged image
    dockerCommand {
      name          = "Push to local registry"
      imagePlatform = Linux
      commandType   = push {
        namesAndTags = "localhost:5000/people-api:%build.number%"
      }
    }

    // 5. Capture the resulting digest
    script {
      name          = "Extract image.digest"
      scriptContent = """
        #!/usr/bin/env bash
        IMAGE="localhost:5000/people-api:%build.number%"
        DIGEST=$(docker inspect --format='{{index .RepoDigests 0}}' $IMAGE)
        echo $DIGEST > image.digest
      """.trimIndent()
    }
  }
  // 2. .NET Restore / Build / Test
  steps {
    dotnetRestore {
      name     = "Restore NuGet packages"
      projects = "**/*.sln"
    }
    dotnetBuild {
      name       = "Build solution"
      projects   = "**/*.sln"
      configuration = "Release"
    }
    dotnetTest {
      name       = "Run unit tests"
      projects   = "**/*Tests/*.csproj"
      configuration = "Release"
      args       = "--no-build --verbosity normal"
    }

    // 3. Build Docker image with two tags: local and registry
    dockerCommand {
      name          = "Build Docker image"
      imagePlatform = Linux
      commandType   = build {
        source = path { path = "." }
        namesAndTags = "people-api:%build.number%, localhost:5000/people-api:%build.number%"
      }
    }

    // 4. Push only the registry-tagged image
    dockerCommand {
      name          = "Push to local registry"
      imagePlatform = Linux
      commandType   = push {
        namesAndTags = "localhost:5000/people-api:%build.number%"
      }
    }

    // 5. Capture the resulting digest
    script {
      name          = "Extract image.digest"
      scriptContent = """
        #!/usr/bin/env bash
        IMAGE="localhost:5000/people-api:%build.number%"
        DIGEST=$(docker inspect --format='{{index .RepoDigests 0}}' $IMAGE)
        echo $DIGEST > image.digest
      """.trimIndent()
    }
  }

  // 6. Expose image.digest as a build artifact
  artifactRules = "image.digest"

  // 7. Enable HTTP (insecure) registry at localhost:5000
  features {
    feature {
      type = "DockerSupport"
      param("teamcity.docker.registry.0.host",         "localhost:5000")
      param("teamcity.docker.registry.0.protocol",     "http")
      param("teamcity.docker.registry.0.allowUnsecure","true")
    }
  }

  // 8. Optional: trigger on each VCS change
  triggers {
    vcs {
      // builds on every commit
  // 6. Expose image.digest as a build artifact
  artifactRules = "image.digest"

  // 7. Enable HTTP (insecure) registry at localhost:5000
  features {
    feature {
      type = "DockerSupport"
      param("teamcity.docker.registry.0.host",         "localhost:5000")
      param("teamcity.docker.registry.0.protocol",     "http")
      param("teamcity.docker.registry.0.allowUnsecure","true")
    }
  }

  // 8. Optional: trigger on each VCS change
  triggers {
    vcs {
      // builds on every commit
    }
  }
})
  }
})