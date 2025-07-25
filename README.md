# People API Submission


## Part 1 – API

**_Completed_**

* _GET  /people/           : returns all people records_ .
* _GET  /people/{id:int}   : returns person identified by Id_ .
* _POST /people            : Add person to in memory data_.
* _PUT  /people            : Update existing person_.
* _GET  /health            : Health check_.

_Tests provided for service and minimal API endpoints._ 



## Part 2 – Dockerfiles & Compose

### Runtime container

**_Completed_**

_docker compose up_ will start API locally on port 8080
* Swagger: http://localhost:8080/swagger/index.html
    
### Agent container

**_Completed_**


## Part 3 – TeamCity CI stack (server + agent + pipeline)

**_Completed_**

_docker compose -f compose.ci.yml up_ will start containers
* teamcity-server
* local-registry
* teamcity-agent   
        
        
In `/.teamcity` create a single build configuration that:
    
    1. Restores, builds, and tests the solution.
        
    2. Builds the Docker image tagged `people-api:%build.number%`.
        
    3. Pushes the image (unauthenticated HTTP) to `localhost:5000/people-api:%build.number%` and stores `image.digest` as the build artifact.

- **_Not Completed_**

*I have created the build configuration settings.kts but was unable to import to Team City when testing end to end.  It appears that I am not making the Kotlin runner available in Team city (versions?) - was unable to resolve within the timeframe*
        
- Provide a PowerShell script `build.ps1` containing a `ci-up` function/task that spins up the CI stack and waits until `http://localhost:8111` responds 200 OK.
    
**_Completed_**
    
* .\build.ps1 ci-up *

---
## Delivered

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