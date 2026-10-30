# IoT Analytics Dashboard — Módulo ADO.NET + PostgreSQL

Módulo en C# (.NET 8) que realiza operaciones CRUD sobre sensores IoT usando **ADO.NET** con el proveedor **Npgsql**, sobre PostgreSQL desplegado con **Docker** y **Kubernetes**.

## Requisitos
- Docker Desktop (con Kubernetes activado para la opción B)
- No es necesario instalar .NET: el SDK corre dentro de un contenedor

## Opción A — Docker Compose (menú interactivo)
```bash
docker compose run --rm app
```
Levanta PostgreSQL con SSL, crea la tabla con 3 sensores y abre el menú.

## Opción B — Kubernetes (Docker Desktop)
```bash
docker build -t iot-analytics-adonet:1.0 .
kubectl apply -f k8s/iot-analytics.yaml
kubectl wait --for=condition=complete job/iot-analytics-demo --timeout=300s
kubectl logs job/iot-analytics-demo
```
Para limpiar: `kubectl delete -f k8s/iot-analytics.yaml`

## Seguridad
- Conexión cifrada con SSL/TLS (`SslMode=Require`).
- Contraseña en la variable de entorno `DB_PASSWORD` (Secret en Kubernetes); no está en el código.
- Todas las consultas usan parámetros (`@nombre`, `@id`) para evitar inyección SQL.

## Estructura
```
Program.cs                 → menú y modo demo
Data/Conexion.cs           → cadena de conexión, SSL y pool
Data/SensorRepository.cs   → CRUD (modelo conectado y desconectado)
Models/Sensor.cs           → modelo de datos
sql/init.sql               → tabla y datos iniciales
docker-compose.yml         → PostgreSQL + SDK de .NET
Dockerfile                 → imagen del módulo para Kubernetes
k8s/iot-analytics.yaml     → Secret, ConfigMap, PostgreSQL y Job
```
