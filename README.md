# Senior Engineer Take‑Home Exercise: People API Submission

## Part 1 – API

- **_Completed_**

*Tests provided for service and minimal api enpoints*.
 
---

## Part 2 – Dockerfiles & Compose

### Runtime container

- **_Completed_**
    
### Agent container

- **_Completed_**
    
---

## Part 3 – TeamCity CI stack (server + agent + pipeline)

- Provide a separate `compose.ci.yml` file that launches:
    
    - `teamcity-server` – - **_Completed_**
        
    - `teamcity-agent` – - **_Completed_**
        
    - `registry` – - **_Completed_**
        
- After the containers are up and the license is accepted, when we're assessing this we will:
    
    1. Add a VCS root pointing at the repository.
        
    2. Import the Kotlin DSL project (which you committed under `/.teamcity` - see next bullet).
        
- In `/.teamcity` create a single build configuration that:
    
    1. Restores, builds, and tests the solution.
        
    2. Builds the Docker image tagged `people-api:%build.number%`.
        
    3. Pushes the image (unauthenticated HTTP) to `localhost:5000/people-api:%build.number%` and stores `image.digest` as the build artifact.

- **_Not Completed_**

*I have created the build configuration settings.kts but was unable to import to Team City when testing end to end.  It appears that I am not making the Kotlin runner available in Team city (versions?) - was unable to resolve within the timeframe*
        
- Provide a PowerShell script `build.ps1` containing a `ci-up` function/task that spins up the CI stack and waits until `http://localhost:8111` responds 200 OK.
    
- Document the one‑liner in the README:
    
**_ Build.ps1 Created _**

* .\build.ps1 ci-up *

---

## Deliverables

```
├── .teamcity/
│   └── settings.kts         # Kotlin DSL build definition
├── src/
│   ├── People.Api/          # ASP.NET Core Minimal API project
│   ├── People.Data/         # EF Core in‑memory database layer
│   └── People.Tests/        # unit test project
├── build.ps1                # ci-up helper
├── compose.ci.yml           # spins up TeamCity server + agent + registry
├── docker-compose.yml       # runs the API locally
├── Dockerfile               # runtime image for the API
├── Dockerfile.agent         # custom agent with Docker CLI
└── README.md                # This document - replace with your own comments
```

