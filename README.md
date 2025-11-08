# URL Shortener

![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)
![React](https://img.shields.io/badge/Frontend-React%20%2B%20Vite-61DAFB?logo=react)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)
![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql)
![Redis](https://img.shields.io/badge/Cache-Redis-DC382D?logo=redis)
![GitHub Actions](https://img.shields.io/github/actions/workflow/status/yourusername/urlshortener/ci.yml?label=CI%20Build&logo=github)
![Vercel](https://img.shields.io/badge/Deployed%20on-Vercel-black?logo=vercel)

---

## Overview

A **full-stack URL shortening service** built with **.NET 8 Web API**, **PostgreSQL**, and **Redis**, paired with a **React + Vite** frontend.  
The backend is **containerized with Docker** for consistent development, testing, and deployment, while the frontend is **deployed on Vercel** for fast, globally distributed delivery.

<img width="824" height="606" alt="image" src="https://github.com/user-attachments/assets/a137b5cd-71eb-4ec0-aa8e-d986a3777ffb" />

---

## Features

- Shorten long URLs into clean, shareable links  
- Expand shortened URLs back to their original form  
- Persistent storage with **PostgreSQL**  
- High-speed caching using **Redis**  
- Automated integration testing via **xUnit + GitHub Actions**  
- **Dockerized backend** for seamless local and CI environments  
- **React + Vite frontend**, deployed on **Vercel**

---

## Tech Stack

| Layer | Technology |
|-------|-------------|
| **Frontend** | React + Vite (TypeScript) |
| **Backend** | ASP.NET Core 8 Web API |
| **Database** | PostgreSQL |
| **Cache** | Redis |
| **Testing** | xUnit + WebApplicationFactory |
| **CI/CD** | GitHub Actions |
| **Deployment** | Backend via Docker / Frontend via Vercel |

---

⚙️ Local Development

```bash
1️. Clone the repository
git clone https://github.com/chumavii/UrlShortener.git
cd urlshortener

2️. Create a .env file
POSTGRES_USER=
POSTGRES_PASSWORD=
POSTGRES_DB=
REDIS_HOST=

3️. Run the backend with Docker Compose
docker compose up --build
This starts:
- The .NET 8 API
- PostgreSQL
- Redis

API available at → http://localhost:8080

4️. Run the frontend (Vite)
cd urlshortener.ui
npm install
npm run dev

Running Tests
To run the full integration test suite locally:
dotnet test

Your CI pipeline automatically:
- Spins up PostgreSQL & Redis containers
- Waits until services are healthy
- Runs all tests using xUnit


