Here's the improved `README.md` file that incorporates the new content while maintaining the existing structure and information:


# UserAccessManagement

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-xUnit%20%7C%20FluentAssertions-green)](#)
[![Docker](https://img.shields.io/badge/docker-enabled-blue)](#)
[![License](https://img.shields.io/badge/license-MIT-blueviolet)](LICENSE)

## 📌 Descripción del proyecto

Sistema de gestión de usuarios construido con Clean Architecture, CQRS y principios de DDD. Proporciona operaciones para administrar usuarios (crear, actualizar, listar) y está preparado para producción con logging estructurado (Serilog → ELK), autenticación JWT y despliegue en Docker.

## 🏗️ Arquitectura

Aplicación organizada en capas (Clean Architecture):

- Domain — Entidades, Value Objects, Reglas del dominio.
- Application — Casos de uso (Commands/Queries, Handlers), DTOs, Interfaces.
- Infrastructure — Implementaciones concretas (EF Core, repositorios, seguridad, logging).
- API — Web API (ASP.NET Core), middlewares, rutas y configuración.

Diagrama de capas (ASCII):


Domain
  ↑
  │  (interfaces)
Application  <-- Handlers (CQRS), DTOs, Services
  ↑
  │  (DI / Implementations)
Infrastructure  <-- EF Core, Repositories, Security, Logging
  ↑
  │
API (ASP.NET Core)  <-- Controllers, Middleware, Swagger


## 🧰 Stack de tecnologías

- 🟣 C# / .NET 8 — Plataforma y lenguaje.
- 🧭 Clean Architecture — Separación de responsabilidades por capas.
- ⚖️ CQRS + DDD — Commands / Queries, Handlers, Entidades y Value Objects.
- 🪪 Entity Framework Core — Acceso a datos (SQL Server configurado en Infrastructure).
- 🔐 JWT (Microsoft.AspNetCore.Authentication.JwtBearer) — Autenticación y autorización.
- 🧾 Serilog — Logging estructurado, salida a consola y archivo JSON.
- 📦 Docker / Docker Compose — Contenerización y despliegue.
- 📊 ELK (Elasticsearch, Logstash, Kibana) + Filebeat — Centralizado de logs.
- 🧪 Testing: xUnit + FluentAssertions + Moq + coverlet — Pruebas unitarias y cobertura.

## 📐 Patrones implementados

- **CQRS**: Separación clara entre Commands y Queries. Los handlers resuelven cada caso de uso de forma independiente (p. ej. `CreateUserHandler`, `GetUsersHandler`, `UpdateUserHandler`).
- **DDD**: Modelado del dominio con Entidades (`User`), Value Objects (`Email`), Repositorios (`IUserRepository`) y reglas de negocio en el dominio.
- **Dependency Injection**: Todas las dependencias inyectadas via contenedor de servicios (Startup/Program y `DependencyInjection` en Infrastructure).

## 📁 Estructura del proyecto

Raíz (resumen):

- `src/` (o proyecto raíz)
  - `UserAccessManagement.Api/` — API Web (Program.cs, middlewares, rutas, Swagger)
  - `UserAccessManagement.Application/` — Casos de uso, DTOs, interfaces, MediatR handlers
  - `UserAccessManagement.Domain/` — Entidades, Value Objects, interfaces de dominio
  - `UserAccessManagement.Infrastructure/` — EF Core, Repositorios, Implementaciones, Seguridad (JWT), DI
  - `UserAccessManagement.Tests/` — Pruebas unitarias (xUnit, Moq, FluentAssertions)

Explicación rápida por carpeta:

- `Api` — Punto de entrada, configuración de middleware, autenticación JWT, Serilog y Swagger.
- `Application` — Contiene Commands/Queries (MediatR), Handlers, DTOs (`UserResponse`), `ApiResponse<T>`, `PagedResult<T>`.
- `Domain` — Modelo de dominio: `User`, `Email` (ValueObject), interfaces como `IUserRepository`.
- `Infrastructure` — `UserAccessDbContext` (EF Core), `UserRepository`, `UnitOfWork`, `BCryptPasswordHasher`, `JwtTokenGenerator`, y la extensión `DependencyInjection`.
- `Tests` — Pruebas por handler (`CreateUserHandlerTests`, `UpdateUserHandlerTests`, `GetUsersHandlerTests`), usando Moq para mocks y FluentAssertions para aserciones.

## ✨ Funcionalidades principales

- Registro y creación de usuarios (hash de contraseña con BCrypt).
- Actualización de usuarios.
- Listado paginado de usuarios (Query + PagedResult).
- Autenticación con JWT.
- Logging estructurado y transporte a ELK a través de Filebeat.
- Pruebas unitarias aisladas (sin DB real) usando Moq.

## ⚙️ Prerrequisitos

- .NET 8 SDK
- Docker & Docker Compose (para despliegue local con dependencias: SQL Server, Elasticsearch, Kibana)
- (Opcional) SQL Server local si no utiliza Docker

## 🚀 Despliegue y configuración con Docker

Se asume que hay un `docker-compose.yml` con servicios: `api`, `db` (SQL Server), `elasticsearch`, `kibana`, `filebeat`.

Ejemplo de comandos (desde la raíz del repositorio):


# Build y levantar servicios
docker-compose build --pull
docker-compose up -d

# Ver logs del API
docker-compose logs -f api


Notas importantes:
- Configurar `appsettings.json`/variables de entorno para la cadena de conexión (`DefaultConnection`) y `Jwt` settings.
- Serilog está configurado para escribir un archivo JSON en `../logs/useraccess.json` — Filebeat está configurado para leer `/app/logs/*.json` (ver `filebeat.yml` más abajo).

Filebeat (extracto relevante) — ya incluido en la solución:


filebeat.inputs:
  - type: log
    enabled: true
    paths:
      - /app/logs/*.json
    json:
      keys_under_root: true
      overwrite_keys: true
      add_error_key: true
      expand_keys: true
      message_key: message
    multiline:
      pattern: '^{'
      negate: true
      match: after

processors:
  - add_host_metadata:
      when.not.contains.tags: forwarded
  - add_cloud_metadata: ~
  - add_docker_metadata: ~
  - decode_json_fields:
      fields: ["message"]
      process_array: false
      max_depth: 2
      target: ""
      overwrite_keys: true

output.elasticsearch:
  hosts: ["elasticsearch:9200"]
  index: "useraccess-logs-%{+yyyy.MM.dd}"

setup.template.name: "useraccess"
setup.template.pattern: "useraccess-*"
setup.template.enabled: true
setup.template.overwrite: false

logging.level: info
logging.to_files: true
logging.files:
  path: /usr/share/filebeat/logs
  name: filebeat
  keepfiles: 7


Este pipeline permite que los logs JSON escritos por Serilog sean enviados a Elasticsearch y visualizados en Kibana.

## 🧪 Ejecutar pruebas

Desde la raíz del proyecto de pruebas o solución:


# Ejecutar todas las pruebas unitarias
dotnet test

# Ejecutar una colección específica y ver cobertura (coverlet collector agregado)
dotnet test --collect:"XPlat Code Coverage"


Buenas prácticas en las pruebas del proyecto:
- Uso de Moq para simulación de `IUserRepository`, `IUnitOfWork`, `IPasswordHasher`, etc.
- FluentAssertions para aserciones legibles.
- Las pruebas siguen Arrange / Act / Assert y se centran en handlers (no se testea controlador ni DB real).

## 📘 Documentación de la API (endpoints principales)

Swagger está habilitado en entorno de desarrollo en `/swagger`.

Rutas base: `/api/v1`

Endpoints (ejemplos):

- **POST** `/api/v1/users` — Crear usuario
  - Body (JSON):
    ```json
    {
      "name": "John",
      "lastname": "Doe",
      "email": "john.doe@example.com",
      "password": "P@ssw0rd",
      "address": "Calle 123"
    }
    ```
  - Respuesta: `ApiResponse<UserResponse>` con `UserResponse` y mensaje de éxito.

- **PUT** `/api/v1/users/{id}` — Actualizar usuario
  - Body (JSON):
    ```json
    {
      "name": "John",
      "lastname": "Doe",
      "email": "john.updated@example.com",
      "address": "Nueva direccion"
    }
    ```
  - Respuesta: `ApiResponse<Unit>` con mensaje de éxito.

- **GET** `/api/v1/users?pageNumber=1&pageSize=10` — Listado paginado
  - Respuesta: `ApiResponse<PagedResult<UserResponse>>`.

Autenticación: endpoints protegidos requieren encabezado `Authorization: Bearer <token>`.

Ejemplo `curl` con JWT:


curl -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"John","lastname":"Doe","email":"john@example.com","password":"P@ss"}' \
  https://localhost:5001/api/v1/users


## 🤝 Contribuir

¡Gracias por querer contribuir! Algunas pautas:

1. Abre un `issue` para discutir cambios importantes.
2. Crea una rama con prefijo `feature/` o `fix/`.
3. Sigue las convenciones del repositorio (`.editorconfig` y `CONTRIBUTING.md` si existen).
4. Añade pruebas unitarias para la lógica nueva o modificada.
5. Envía un Pull Request con descripción clara y referencia al issue.

## 📜 Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo `LICENSE` para más detalles.

---

Si quieres, puedo:
- Generar un `docker-compose.yml` base para levantar API + SQL Server + ELK + Filebeat.
- Añadir un `CONTRIBUTING.md` y `.editorconfig` con las reglas del proyecto.
- Crear ejemplos de Postman / OpenAPI más detallados.

This version of README.md maintains the original structure while seamlessly integrating new content, ensuring clarity and consistency throughout the document.