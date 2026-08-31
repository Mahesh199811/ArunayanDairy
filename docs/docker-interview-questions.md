# Docker Interview Questions — Arunayan Dairy Project

Detailed answers based on what was actually implemented in Steps 9–16 (containerizing
`UserService`, `ProductService`, `OrderService`, MySQL, and the React frontend, then
orchestrating everything with Docker Compose).

---

## 1. Basic Docker Questions

### Q1. What is Docker?

Docker is a containerization platform that packages an application together with its
runtime, dependencies, libraries, and configuration into a portable, isolated unit
called a **container**. Containers share the host OS kernel (unlike VMs, which
virtualize an entire OS), which makes them lightweight and fast to start.

In this project, instead of running `dotnet run` directly on the host machine for each
service, a Docker image was built for each of:

```
UserService
ProductService
OrderService
React Frontend (Nginx)
```

and each ran as an isolated container with its own filesystem, process space, and
network interface, while still being able to talk to the others over a shared Docker
network.

**Key point to make in an interview:** Docker solves the "works on my machine" problem
by shipping the *environment* along with the *code*.

---

### Q2. Why did you use Docker in your project?

**Model answer:**

> We had multiple microservices (User, Product, Order) plus a React frontend and a
> MySQL database, each with different runtime requirements (.NET 8 runtime, Node/Nginx,
> MySQL server). Docker let us package each service consistently so the same artifact
> that runs on a developer's laptop also runs in CI and eventually in AWS. It also made
> each microservice independently buildable, deployable, and restartable, and gave us a
> single command (`docker compose up`) to bring up the entire stack instead of manually
> installing .NET, Node, and MySQL locally.

```
UserService      ProductService
     ↓                  ↓
Docker Image      Docker Image
     ↓                  ↓
 Container          Container
```

Additional benefits worth mentioning:
- Consistent environment across dev/test/prod → eliminates dependency drift.
- Fast onboarding — a new developer just needs Docker, not five different runtimes.
- Foundation for later AWS ECS deployment (same images run in ECS tasks).

---

## 2. Image vs Container

### Q3. What's the difference between a Docker image and a container?

An **image** is an immutable, read-only template that contains the application code,
runtime, libraries, and filesystem snapshot needed to run the app. It's built once and
can be reused to start many containers.

A **container** is a running (or stopped) *instance* of an image — it adds a thin
writable layer on top of the image and its own process, network namespace, and
lifecycle.

```
Dockerfile
    ↓ (docker build)
Docker Image  (immutable template)
    ↓ (docker run)
Docker Container  (running instance)
```

Example from the project:

```
docker build -t arunayandairy-user-service:1.0 .
```
creates the **image**. Then:

```
docker run -d --name arunayandairy-user-service -p 5080:8080 arunayandairy-user-service:1.0
```
creates a **container** from that image. You can run this command multiple times
(with different `--name`/ports) to get multiple independent containers from the
same image — this is exactly how horizontal scaling works later in ECS.

---

### Q4. What happens when you run `docker build`?

Docker reads the Dockerfile top to bottom and executes each instruction, producing a
new filesystem **layer** for most instructions (`FROM`, `RUN`, `COPY`, etc.). Layers are
cached, so if an earlier instruction and its inputs haven't changed, Docker reuses the
cached layer instead of re-executing it — this is why `COPY *.csproj` + `dotnet
restore` are done *before* `COPY . .` in the Dockerfiles: dependency restore layers can
be cached even when application source code changes.

```
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrderService.Api.csproj", "./"]
RUN dotnet restore "OrderService.Api.csproj"
COPY . .
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish
```

Each of `FROM`, `COPY`, `RUN` produces a layer. The final set of layers is assembled
into the resulting image, tagged (e.g. `arunayandairy-order-service:latest`), and
stored in the local image cache (`docker images`).

---

## 3. Dockerfile Questions

### Q5. Explain the Dockerfile you created.

The backend services (`UserService`, `ProductService`, `OrderService`) all use the same
shape of multi-stage Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["UserService.Api.csproj", "./"]

RUN dotnet restore "UserService.Api.csproj"

COPY . .

RUN dotnet publish "UserService.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UserService.Api.dll"]
```

Line by line:
- `FROM ... sdk:8.0 AS build` — starts a named build stage using the full SDK (compiler + tooling).
- `WORKDIR /src` — sets the working directory inside the image for subsequent instructions.
- `COPY ["*.csproj", "./"]` — copies only the project file first, to maximize layer-cache reuse.
- `RUN dotnet restore` — downloads NuGet dependencies (cached unless the `.csproj` changes).
- `COPY . .` — copies the rest of the source code.
- `RUN dotnet publish ... -o /app/publish` — compiles and publishes a self-contained-less, framework-dependent output into `/app/publish`.
- `FROM ... aspnet:8.0 AS final` — starts a second, smaller stage using only the ASP.NET **runtime** (no compilers/SDK).
- `COPY --from=build /app/publish .` — copies just the published output from the build stage into the final image.
- `EXPOSE 8080` — documents the port the app listens on.
- `ENV ASPNETCORE_URLS=http://+:8080` — tells Kestrel to bind to all interfaces on port 8080 inside the container.
- `ENTRYPOINT ["dotnet", "UserService.Api.dll"]` — the command run when the container starts.

You should be able to justify every single line if asked.

---

### Q6. Why did you use a multi-stage Docker build?

**Strong answer:**

> I used a multi-stage build to separate *compilation* from *runtime*. The SDK image is
> required to restore NuGet packages, compile, and publish the .NET application, but
> the production container only needs the ASP.NET **runtime** to execute the already-
> compiled DLLs. So I copy only the published output (`/app/publish`) from the build
> stage into a smaller runtime-only final image. This reduces final image size
> (roughly a third to half the size of the SDK image) and reduces the attack surface,
> since compilers, SDKs, and build tooling aren't shipped to production.

```
SDK Image (build stage)
   |
   +-- dotnet restore
   +-- dotnet build
   +-- dotnet publish
          |
          v
      /app/publish
          |
          v
ASP.NET Runtime Image (final stage)
          |
          v
      Container (only what's needed to run)
```

The same pattern was applied to the React frontend: a `node:22-alpine` stage runs
`npm ci` + `npm run build`, and only the resulting static `dist/` folder is copied into
an `nginx:alpine` final stage — Node.js itself never ships in the production image.

---

### Q7. Why didn't you use the SDK image as the final image?

Because the SDK image bundles the full .NET SDK: compilers, MSBuild, NuGet CLI tooling,
templates, etc. — none of which are needed to *run* an already-published app.

Using the SDK image in production would:
- Increase image size significantly (SDK images are much larger than runtime images).
- Increase the attack surface — more binaries/packages means more potential CVEs.
- Slow down image pulls/deploys (larger images take longer to push/pull, which matters a lot for ECS rolling deployments and auto scaling).
- Include unnecessary tooling that provides no runtime benefit and could even be misused if the container were compromised.

---

### Q8. What is `EXPOSE 8080`?

`EXPOSE` is documentation/metadata embedded in the image that tells anyone reading the
Dockerfile (and tools like Compose) which port the containerized application listens
on. In this project:

```
ASP.NET Core (Kestrel)
     ↓
Container port 8080
```

Important nuance: `EXPOSE` does **not** publish the port to the host by itself — it's
purely informational and used for inter-container communication defaults. To make the
port reachable from the host machine's browser/curl, you must explicitly publish it:

```
docker run -p 5080:8080 ...
```

or in Compose:

```yaml
ports:
  - "5080:8080"
```

---

## 4. Port Mapping

### Q9. Explain `-p 5000:8080`.

The syntax is `-p <host-port>:<container-port>`. It creates a mapping so that traffic
hitting the **host** on port 5000 gets forwarded by Docker's networking layer into the
**container** on port 8080.

```
Host port       Container port
    5000   →       8080
```

So:

```
localhost:5000  →  Docker (iptables/NAT)  →  container:8080  →  Kestrel
```

Actual mappings used in this project:

```
UserService     → -p 5080:8080
ProductService  → -p 5001:8080
OrderService    → -p 5002:8080
Frontend        → -p 3000:80
```

Each service listens on `8080` (or `80` for Nginx) *inside* its own container, but is
reachable on a different, unique **host** port, because all containers share the same
host network namespace for published ports.

---

### Q10. What happens if port 5000 is already being used?

Docker fails to bind the host-side port and the container either fails to start or
exits immediately, with an error such as:

```
Error starting userland proxy: listen tcp4 0.0.0.0:5000: bind: address already in use
```
or
```
Bind for 0.0.0.0:5000 failed: port is already allocated
```

Troubleshooting steps:
1. `docker ps` — check if another container already published that host port.
2. On macOS/Linux: `lsof -i :5000` (or `sudo lsof -i :5000`) to find the OS-level process holding the port.
3. Either stop the conflicting container/process (`docker stop <container>` or `kill <pid>`), or simply choose a different, free host port:
   ```
   docker run -p 5010:8080 ...
   ```
   The container-side port (8080) doesn't need to change — only the host-side mapping does.

---

## 5. Docker Networking

This is one of the most important areas covered in the project.

### Q11. Why did you create a Docker network?

Because the microservices need to talk to each other (Order Service → Product Service)
and to MySQL, and by default, separate containers can't resolve each other by name
unless they're placed on the same **user-defined bridge network**.

```
docker network create arunayandairy-network
```

Then `UserService`, `ProductService`, `OrderService`, `MySQL`, and later the `frontend`
container were all attached to this network (in Compose this happens automatically for
every service unless configured otherwise). Being on the same user-defined network gives
two things:
1. **Isolation** — only containers on this network can reach each other by default.
2. **Docker DNS** — containers can resolve each other by their **service/container
   name** instead of hardcoded IPs, which change every time a container restarts.

---

### Q12. Why can't Order Service use `localhost` to communicate with Product Service?

Because each container has its **own network namespace**. Inside a container,
`localhost` (127.0.0.1) always refers to *that same container*, never to a sibling
container, even if they're on the same Docker network.

```
OrderService container
       |
       ↓ localhost
       |
       ↓
   OrderService itself (not ProductService!)
```

So a request to `http://localhost:8080/api/products` from inside the OrderService
container would try to hit a port on the OrderService container itself, not the
Product Service container, and would fail (connection refused, since OrderService
doesn't serve `/api/products`).

The correct approach is to use the other container's **service name** as the hostname,
which Docker's embedded DNS resolves to that container's internal IP address:

```
OrderService
      |
      ↓  http://product-service:8080
      |
      ↓
ProductService container
```

This is exactly why `appsettings.json` uses `http://localhost:5001/` for local
development (browser/host access) but `docker-compose.yml` overrides it with
`Services__ProductService=http://product-service:8080/` for container-to-container
calls.

---

### Q13. How does Docker resolve `product-service`?

On a **user-defined bridge network** (which is what Docker Compose creates by default
for every `docker-compose.yml`), Docker runs an embedded DNS server. Every container on
that network is automatically registered with a DNS entry equal to its **Compose
service name** (and any `container_name`/network aliases).

If `docker-compose.yml` declares:

```yaml
services:
  product-service:
    ...
```

then any other container on the same network can resolve the hostname `product-service`
to that container's internal IP address:

```
OrderService
     |
     |  DNS query: "product-service"
     v
Docker embedded DNS (per user-defined network)
     |
     |  resolves to internal container IP
     v
ProductService container : 8080
```

Note: this DNS resolution **only** works on user-defined networks — the legacy default
`bridge` network does *not* provide automatic name resolution, which is one reason
Compose (and `docker network create`) is preferred over ad hoc `docker run` without a
network.

---

### Q14. What is the difference between Docker network and port publishing?

This distinguishes two completely different communication paths:

**Port publishing (`-p` / `ports:`)** exposes a container's port to the **host machine**
(and, from there, potentially the outside world). This is what lets your **browser**
(running outside Docker) reach a container.

```
Host  →  Container
localhost:5001  →  ProductService:8080
```

**Docker network** allows **container-to-container** communication using service names,
completely independent of any host port publishing.

```
Container → Container
OrderService  →  product-service:8080  →  ProductService
```

Crucially: **you don't need to publish a container's port to the host at all for
another container on the same Docker network to reach it.** In this project, MySQL's
port 3306 is published to the host (useful for local `mysql` CLI/GUI access), but even
if it weren't, `UserService`, `ProductService`, and `OrderService` could still reach
`mysql:3306` purely over the Docker network.

---

## 6. Real Production Troubleshooting

### Q15. Your container is running but the application is not accessible. How would you troubleshoot?

Systematic checklist, in order:

```
1. docker ps                         → confirm the container is actually running
2. docker logs <container>           → check for startup errors / crashes / exceptions
3. docker inspect <container>        → verify env vars, network, mounted volumes, IP
4. docker port <container>           → confirm actual host↔container port mapping
5. curl http://localhost:<port>/...  → test connectivity from the host directly
6. Confirm the app is listening on the *container* port Docker expects
```

For example, if the ASP.NET app is configured to listen on port `5000` inside the
container (via `ASPNETCORE_URLS`), but the Dockerfile/Compose publishes `5001:8080`,
the app will never receive traffic — Docker forwards to `8080`, but nothing is listening
there.

```
Application listens on → 8080  (correct)
Docker mapping         → 5001:8080  (host:container)
```

If those two don't match, you get connection refused/timeouts even though `docker ps`
shows the container as "running" — a running container is not the same as a correctly
configured, listening application.

---

### Q16. Container immediately exits after starting. What do you check?

```
docker ps -a          → confirms it exited, and shows the exit code
docker logs <container> → shows *why* it exited (stack trace, missing config, etc.)
docker inspect <container> → check ENTRYPOINT/CMD, env vars, restart policy
```

Common root causes observed in practice:
- The application threw an unhandled exception at startup (e.g. a bad connection
  string) and the process terminated — this is exactly what would happen if
  `ConnectionStrings__DefaultConnection` pointed at an unreachable MySQL host.
- A required environment variable was missing or misspelled (e.g.
  `Services__ProductService` vs a typo like `Service__ProductService`).
- The database wasn't ready yet (no retry/wait logic — order-of-startup issue, e.g.
  OrderService started before MySQL finished initializing).
- Wrong `ENTRYPOINT`/`CMD` — pointing at a DLL that doesn't exist in the final image.
- A missing dependency (rare with a proper multi-stage build, but possible with native
  dependencies not present in the runtime image).

---

### Q17. Your Order Service is running but orders are failing. Product Service is also running. What would you check?

Investigate in this order (this mirrors the real Step 15/16 failure test where
`product-service` was stopped and order creation failed with a 500 and a
`SocketException: Name or service not known`):

```
1. Order Service logs          → what exception is thrown? (HttpRequestException? timeout?)
2. Product Service logs        → is it even receiving the request?
3. Docker network               → are both containers on the same network?
4. DNS / service name           → is the hostname used (product-service) correct?
5. Port                         → is 8080 correct (not 5001, which is the host mapping)?
6. HTTP endpoint / route        → does /api/products/{id} actually exist and return 200?
7. Configuration                → is Services__ProductService set correctly for this environment?
```

Useful command:

```
docker network inspect arunayandairy-network
```

This lists every container attached to the network along with its internal IP —
useful for confirming both services really are on the same network. Then verify the
configured base URL (`http://product-service:8080/`) matches the actual Compose service
name and the port the target container's app listens on internally (not the host-
published port). This is precisely the kind of dependency failure that was
deliberately tested by stopping `product-service` and confirming order creation failed,
then restarting it and confirming recovery.

---

## 7. Docker Compose

### Q18. Why did you introduce Docker Compose?

Before Compose, each service had to be started manually and repetitively:

```
docker run -d --name arunayandairy-mysql ... mysql:8.4
docker run -d --name arunayandairy-user-service ... 
docker run -d --name arunayandairy-product-service ...
docker run -d --name arunayandairy-order-service ...
docker run -d --name arunayandairy-frontend ...
```

each requiring the correct network, port mappings, environment variables, and
dependency ordering remembered by hand.

Compose (`docker-compose.yml`) lets the entire application stack be defined
**declaratively** in one file:

```yaml
services:
  mysql:
  user-service:
  product-service:
  order-service:
  frontend:

networks:
  arunayandairy-network:

volumes:
  mysql-data:
```

Then a single command:

```
docker compose up -d
```

builds/starts everything, connects every service to the shared network automatically,
and wires up dependencies (`depends_on`). It also makes the whole environment
reproducible and version-controllable (the compose file is checked into Git).

---

### Q19. What happens when you run `docker compose up --build`?

Compose performs these steps:

1. Reads and parses `docker-compose.yml`.
2. Builds any images that have a `build:` section (forced rebuild because of `--build`).
3. Creates the network(s) declared (or reuses existing ones).
4. Creates named volumes if they don't already exist.
5. Creates containers for each service, wiring in the correct network, ports, env vars, and volumes.
6. Starts the containers, respecting `depends_on` ordering.
7. Attaches every container to the Compose-managed network so they can resolve each other by service name.

```
docker-compose.yml
       ↓
   Network created
       ↓
  Images built
       ↓
 Containers created
       ↓
 Containers started
       ↓
Full application stack running
```

In this project this is the exact command used to bring up MySQL, all three
microservices, and the frontend together (`docker compose up --build -d`).

---

### Q20. Difference between `docker compose up` and `docker compose up --build`?

`docker compose up` starts containers using **existing images** if they're already
built — it does *not* automatically pick up code changes unless the image is rebuilt.

`docker compose up --build` forces Compose to **rebuild** every image with a `build:`
section before starting containers, ensuring the latest Dockerfile and source code
changes are baked into the image.

Rule of thumb — use `--build` after changing:
```
Dockerfile
Application source code
Dependencies (csproj / package.json)
Build configuration (tsconfig, appsettings baked into image, etc.)
```
If you only changed a Compose-level environment variable or port mapping (not the
image itself), a plain `docker compose up -d` is sufficient and faster.

---

### Q21. What does `docker compose down` do?

It stops all Compose-managed containers and removes them, along with the network(s)
Compose created. By default it does **not** remove named volumes:

```
docker compose down
```
→ stops/removes containers + network, **keeps** `mysql-data` volume (data survives).

```
docker compose down -v
```
→ also removes named volumes — **this deletes the MySQL data permanently.**

This distinction is critical for any service backed by a database: `docker compose
down` is safe to run routinely (e.g., to reclaim resources or reset containers), but
`-v` should only be used deliberately, since it wipes persistent state.

---

## 8. Docker Volumes

### Q22. Why did you create a volume for MySQL?

```yaml
volumes:
  - mysql-data:/var/lib/mysql
```

Containers are ephemeral by design — anything written to a container's writable layer
is lost when the container is removed (`docker rm`, or an image rebuild that recreates
the container). MySQL's actual data files live at `/var/lib/mysql` inside the
container, so without a volume:

```
MySQL Container
      ↓
Container deleted/recreated
      ↓
All data lost
```

By mounting a named Docker **volume** at that path, the actual data lives outside the
container's writable layer, managed by the Docker engine on the host:

```
MySQL Container  (ephemeral)
      ↓ mounts
mysql-data volume  (persistent, lives on host)
      ↓
Data survives container restarts, recreation, and rebuilds
```

This was verified directly in the project: orders created via the API remained present
after `docker compose stop/start order-service` **and** after a full `docker compose
restart` of every service.

---

### Q23. What would happen if you run `docker compose down -v`?

The `-v` flag additionally removes any named volumes declared in the compose file
(as long as no other container is still using them). For this project:

```
mysql-data volume
     ↓
REMOVED
     ↓
arunayandairy_users, arunayandairy_products, arunayandairy_orders — all gone
```

The next `docker compose up` would start a **brand-new, empty** MySQL instance — EF
Core migrations would need to run again, and all previously created users, products,
and orders would be permanently lost (no automatic backup unless one was taken
separately). This is why `-v` should never be used casually against an environment with
data you care about, and is generally reserved for a genuinely clean-slate reset during
local development.

---

## 9. Environment Variables

### Q24. Why did you use environment variables in Docker Compose?

```yaml
environment:
  ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=arunayandairy_orders;User=root;Password=rootpassword;"
  Services__ProductService: "http://product-service:8080/"
```

Environment variables let configuration be **injected at container start time**,
without ever having to change or rebuild the application code/image. The exact same
Docker image can then be run in different contexts simply by supplying different
environment variables:

```
Local dev (outside Docker)  → appsettings.json → Server=localhost;...
Docker Compose               → env var override → Server=mysql;...
AWS ECS (future)             → env var / Secrets Manager → Server=<RDS endpoint>;...
```

This is the "build once, configure everywhere" principle — critical for moving the same
image from local Docker Compose into ECS later without any code changes.

---

### Q25. Why use `ConnectionStrings__DefaultConnection` instead of modifying appsettings.json?

ASP.NET Core's configuration system supports multiple providers layered on top of each
other, and by default the **environment variable provider** has higher precedence than
`appsettings.json`. It maps double-underscore (`__`) in an environment variable name to
a nested JSON path, because most shells don't allow colons (`:`) in environment variable
names:

```
Environment variable:        ConnectionStrings__DefaultConnection
Maps to appsettings.json:    { "ConnectionStrings": { "DefaultConnection": "..." } }
```

Benefits of doing it this way instead of editing `appsettings.json` per environment:
- No need to bake environment-specific secrets/values into the image at build time.
- The same built image works in dev, Compose, and (later) ECS — only the *environment
  variables* passed to the container differ.
- Sensitive values (passwords, connection strings) can be swapped for secret-manager
  references without ever touching source control.

---

## 10. Docker Security

### Q26. Would you put the MySQL root password directly in docker-compose.yml in production?

**No.** It was done here (`MYSQL_ROOT_PASSWORD: rootpassword`) purely for local learning
convenience — it is explicitly **not** production practice.

In a real production deployment I would:
```
✓ Use a proper secrets manager (e.g. AWS Secrets Manager or SSM Parameter Store)
  and inject the value at runtime rather than hardcoding it in a YAML file.
✓ Avoid using the MySQL `root` user for application connections — create a
  least-privilege application-specific database user instead.
✓ Restrict database network access (e.g. RDS in a private subnet, not publicly
  reachable, security group locked down to only the app's security group).
✓ Encrypt connections (TLS) where the data sensitivity warrants it.
✓ Rotate credentials periodically, and immediately if ever exposed.
```

The current setup is intentionally simple to focus on learning Docker fundamentals
before the AWS phase introduces proper secret management.

---

### Q27. Should you commit passwords to GitHub?

**No, never** — even the demo password used here (`rootpassword`) is only acceptable
because this is a disposable local learning environment with no real data or public
exposure.

Best practice going forward:
- Real credentials should never be committed to source control (use `.gitignore` for
  any file containing secrets, e.g. a local `.env` that's excluded from Git).
- Use environment-specific secret stores (AWS Secrets Manager, Parameter Store, Vault,
  etc.) so secrets live outside the repository entirely.
- If a secret is ever accidentally committed, it must be treated as compromised —
  rotate it immediately, not just remove it from the latest commit (Git history still
  contains it).

---

## 11. Frontend Docker Questions

### Q28. Why did you use Nginx for React?

React (via Vite) compiles down to plain **static assets** — HTML, CSS, and JavaScript
bundles — after running `npm run build`. Once that build step is done, Node.js itself
is no longer needed to *serve* the app; you just need any static file server.

```
Node.js
 ↓ npm run build
dist/  (static HTML/CSS/JS)
 ↓ served by
Nginx
 ↓
Browser
```

Nginx is a very small, fast, battle-tested static file / reverse-proxy server, so using
it as the runtime for the compiled frontend gives a lightweight, production-grade
container rather than shipping a full Node.js runtime (or worse, running Vite's dev
server) in production.

---

### Q29. Why did you use a multi-stage build for React?

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**First stage (`build`):** uses `node:22-alpine` to install dependencies (`npm ci`,
which does a clean, reproducible install from `package-lock.json`) and run the Vite
production build (`npm run build`), producing a `dist/` folder.

**Second stage (final):** uses `nginx:alpine` and copies **only** the compiled `dist/`
output from the first stage.

The final image deliberately does **not** contain:
```
node_modules/
npm / Node.js itself
TypeScript/Vite source files
Dev dependencies
```

Only the final compiled static assets are shipped, which keeps the production image
small, fast to pull/deploy, and free of any Node.js-related attack surface or dev
tooling. This mirrors the exact same reasoning as the .NET multi-stage builds — separate
the *build environment* from the *runtime environment*.

---

## 12. Microservices Questions

### Q30. Why did you create separate containers for each microservice?

Because each service should be independently:
```
Built       — its own Dockerfile/image, its own release cadence
Deployed    — can be redeployed without touching the others
Scaled      — can run more or fewer replicas based on its own load
Restarted   — a crash/restart in one doesn't require restarting the others
Monitored   — separate logs, separate health checks, separate metrics
```

Concretely, if Product Service traffic spikes, only Product Service needs to scale:

```
Product traffic increases
        ↓
Scale Product Service: 2 → 5 containers
```

while User Service and Order Service remain unaffected:

```
UserService
        ↓
remains at 2 containers (unaffected)
```

This is precisely the model that AWS ECS (with its own service-level Auto Scaling per
task definition) is built around — each Compose "service" here maps naturally to an ECS
"service" later, each independently scalable.

---

### Q31. What happens if Product Service goes down?

Order Service has a **hard runtime dependency** on Product Service — every order
creation flow calls `ProductServiceClient.GetProduct(...)` to fetch price/details and
`.../reduce-stock` to decrement inventory. This was actually tested directly:

```
docker compose stop product-service
      ↓
POST /api/orders
      ↓
OrderService
      |
      X   (SocketException: "Name or service not known (product-service:8080)")
ProductService (unreachable)
      ↓
HTTP 500 returned to the caller
```

When Product Service is restarted:

```
docker compose start product-service
      ↓
ProductService → Healthy
      ↓
POST /api/orders retried
      ↓
Order succeeds (HTTP 201)
```

In a real production architecture, this brittle "hard failure" behavior should be
improved with resiliency patterns such as:
```
Timeouts           — fail fast instead of hanging
Retries            — transient network blips shouldn't fail the whole request
Circuit breakers   — stop hammering a known-down dependency
Health checks      — ALB/ECS should only route to healthy tasks
Load balancing     — spread requests across multiple healthy ProductService tasks
Observability      — structured logs/metrics/tracing to see *where* a failure occurred
```

---

## 13. Scenario-Based Questions

### Q32. "Your Docker container works locally but fails in production. How do you debug?"

Systematically compare environments rather than guessing:

```
Application version         — is prod running the same image/tag as local?
Docker image                — same digest/tag, not just same "latest" name?
Environment variables       — connection strings, service URLs, feature flags
Secrets                     — are they actually injected in prod (vs local .env)?
Ports                       — correct container port bound and published?
Network                     — is the container even on the right network/VPC/subnet?
DNS                         — can it resolve dependent service hostnames in prod?
CPU/memory                  — is prod resource-constrained (OOM kill, throttling)?
Dependencies                — external DB/API reachable from prod's network?
File permissions            — non-root user in prod image causing access issues?
Runtime version             — same .NET/Node runtime version as local?
```

Then use the standard toolkit:
```
docker logs <container>
docker inspect <container>
```

If this is running in AWS ECS specifically, extend the checklist to the orchestration
layer:
```
ECS Task status              — did the task even start, or is it stuck/failed?
Container exit code           — what did the container report on exit?
CloudWatch Logs               — the container's stdout/stderr, centralized
Target Group health checks    — is ALB marking the task healthy?
Security Groups               — is traffic actually allowed between ALB/tasks/RDS?
IAM Task Role                 — does the task have permission to reach the resources it needs?
ECR image                     — was the correct image actually pushed and pulled?
```

---

### Q33. "Your container is healthy but users get 502."

A container reporting "healthy" only means *the process is running* — it says nothing
about whether traffic can actually reach it end-to-end. Think about the full request
path, not just the container:

```
User
 ↓
Load Balancer (ALB)
 ↓
Target Group
 ↓
Container (task)
 ↓
Application (Kestrel/Nginx)
```

A 502 typically means the load balancer couldn't get a valid response from the
target, so check each hop:
```
ALB listener       — correct protocol/port configured?
Target group       — is the container's task actually registered as a target?
Health check       — is the health check path/port/expected status code correct?
Container port     — does the target group point at the port the app *actually* listens on?
Security group     — does the ALB's SG have outbound/the task's SG have inbound allowed on that port?
Application binding— is the app bound to 0.0.0.0 inside the container, not just 127.0.0.1?
```

This is exactly the "container port vs app binding" mismatch discussed in Q15/Q9 —
"running" ≠ "reachable."

---

### Q34. "How would you reduce Docker image size?"

```
Multi-stage builds              — ship only the compiled/published output, not the SDK/build tools
Smaller base images              — e.g. *-alpine variants instead of full Debian/Ubuntu-based images
.dockerignore                    — exclude node_modules, bin/, obj/, .git, etc. from the build context
Remove unnecessary dependencies  — no dev-only packages in the final image
Don't include source/build artifacts — only ship what's needed to run, not to build
Use runtime-only images          — ASP.NET runtime instead of SDK, Nginx instead of Node.js
```

This project already applies all of these:
```
✓ Multi-stage build   (SDK → ASP.NET runtime; Node → Nginx)
✓ .dockerignore        (node_modules/, dist/, .git/, .env, etc. excluded from build context)
✓ Runtime image        (final stage uses aspnet:8.0 / nginx:alpine, not the full SDK/Node image)
```

---

### Q35. "How do you troubleshoot high CPU usage in a container?"

Start with a quick, live snapshot:

```
docker stats
```

This shows per-container CPU %, memory usage, network I/O, and block I/O in real time,
letting you quickly identify *which* container is the culprit.

Then dig deeper into the specific resource dimensions:
```
Container CPU        — sustained high % vs a transient spike?
Memory                — is it also climbing (possible leak) or stable?
Network               — unusually high throughput (retry storm, DDoS, chatty client)?
I/O                   — heavy disk/database read-write?
Application logs       — errors, exceptions, retry loops, tight loops?
Application profiling  — attach a profiler / use framework-specific diagnostics (dotnet-trace, Node --prof, etc.)
```

Finally, narrow down the root cause category:
```
Application       — inefficient code, infinite/tight loop, unbounded recursion
Database          — slow/unindexed queries causing the app to spin waiting or retry
External dependency — a slow downstream call causing thread/connection pool exhaustion
Insufficient resources — the container's CPU/memory limits are simply too low for its workload
Traffic spike      — legitimate load increase, may need horizontal scaling
Memory leak        — steadily increasing memory eventually triggering GC pressure/CPU spikes
```

---

## 14. Commands You Should Be Able to Explain

You don't need to memorize every flag, but you should be able to explain *why* you'd
use each of these, not just recite them:

```
docker --version              — check installed Docker CLI version
docker info                   — engine-wide info (containers, images, storage driver, etc.)
docker images                 — list locally built/pulled images
docker ps                     — list running containers
docker ps -a                  — list all containers, including stopped/exited ones
docker build                  — build an image from a Dockerfile
docker run                    — create + start a container from an image
docker stop                   — gracefully stop a running container (SIGTERM then SIGKILL)
docker start                  — start an existing, stopped container
docker restart                — stop then start a container
docker rm                     — remove a (stopped) container
docker rmi                    — remove an image
docker logs                   — view a container's stdout/stderr
docker exec                   — run a command inside a running container (e.g. a shell)
docker inspect                — detailed JSON metadata about an image/container/network/volume
docker stats                  — live resource usage (CPU/memory/network/IO) per container
docker network ls              — list Docker networks
docker network create          — create a user-defined network
docker network inspect          — see which containers are attached and their IPs
docker network connect/disconnect — attach/detach a running container to/from a network
docker volume ls                — list named volumes
docker volume inspect            — see a volume's mount point and metadata
docker compose up               — start (and build if needed) the whole stack
docker compose down             — stop and remove containers + network (keeps volumes unless -v)
docker compose ps               — list containers managed by the current compose project
docker compose logs             — view logs for one or all compose services
docker compose build             — (re)build images defined in the compose file
docker compose exec              — run a command inside a running compose-managed container
```

For interviews: **knowing why you use a command in a given troubleshooting scenario is
far more valuable than reciting the command from memory.**

---

## 15. Your 10 Most Important Questions

If preparation time is limited, master these first, in this order:

| # | Question | Importance |
|---|----------|------------|
| 1 | Image vs Container | ⭐⭐⭐⭐⭐ |
| 2 | Explain your Dockerfile | ⭐⭐⭐⭐⭐ |
| 3 | Why multi-stage build? | ⭐⭐⭐⭐⭐ |
| 4 | Docker networking | ⭐⭐⭐⭐⭐ |
| 5 | Why can't containers use `localhost`? | ⭐⭐⭐⭐⭐ |
| 6 | Docker Compose | ⭐⭐⭐⭐⭐ |
| 7 | Docker volumes | ⭐⭐⭐⭐⭐ |
| 8 | Container troubleshooting | ⭐⭐⭐⭐⭐ |
| 9 | Environment variables/secrets | ⭐⭐⭐⭐⭐ |
| 10 | Container works but API isn't reachable | ⭐⭐⭐⭐⭐ |

---

## One Interview Answer You Should Practice

If the interviewer asks:

> **"Explain how you containerized your project."**

A strong, project-based answer:

> "I built Arunayan Dairy as a microservices-based application with separate User,
> Product, and Order services and a React frontend. I created multi-stage Dockerfiles
> for the .NET services, using the .NET SDK image for build and the ASP.NET runtime
> image for the final container. Each service listens on port 8080 internally. I used
> Docker Compose to orchestrate the services locally and created a dedicated Docker
> network so the services could communicate using service names rather than localhost.
> For persistence, I added a MySQL container with a named Docker volume and migrated
> each service from EF Core InMemory to its own logical MySQL database. The Order
> Service communicates with Product Service through the Docker network. The React
> application is built using Node and served using Nginx. Finally, I tested service
> failures, database persistence, container logs, networking, and the complete
> end-to-end order flow."

That's a **project-based DevOps answer** grounded in real, verified behavior — not just
a textbook definition of Docker.
