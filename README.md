# EventDrivenWebApplication

## Overview
EventDrivenWebApplication is an event-driven architecture application designed to handle various business processes efficiently. It utilizes microservices principles, leveraging messaging patterns for communication between components. This application incorporates features such as customer management, product handling, and inventory checks.

## Features
- **Customer Management**: Register, update, delete, and retrieve customer information.
- **Product Management**: Create and manage products, including inventory checks.
- **Event Handling**: Use of events and messages to facilitate communication between different parts of the system.
- **Saga Orchestration**: Manage complex workflows through state machines.
- **API Versioning**: Support for multiple API versions.
- **Documentation**: Swagger integration for API documentation.

## Technologies Used

- **.NET 8.0**: The application is built using .NET 8.0, which provides a robust framework for building web applications.
  
- **ASP.NET Core**: Used for developing the API, leveraging features like dependency injection, middleware, and routing.

- **Entity Framework Core**: An ORM used for database operations, allowing for seamless interaction with SQL Server.

- **MassTransit**: A distributed application framework for .NET that facilitates message-based communication and supports RabbitMQ.

- **RabbitMQ**: A message broker that enables asynchronous communication between different services in the application.

- **SQL Server**: A relational database management system used to store application data.

- **Docker**: Containerization platform used to package the application and its dependencies, making it easy to deploy and run in different environments.

- **Docker Compose**: Tool for defining and running multi-container Docker applications, allowing for easy orchestration of services like the API, SQL Server, and RabbitMQ.

- **Serilog**: A logging library for .NET, providing structured logging capabilities.

- **ASP.NET API Versioning**: Enables versioning of the API, allowing for backward compatibility as the application evolves.


## Folder Structure
EventDrivenWebApplication
├── docker
│   └── Dockerfile
├── docker-compose
│   └── docker-compose.yml
├── EventDrivenWebApplication.API
│   ├── Controllers
│   ├── Configuration
│   ├── Consumers
│   ├── Models
│   └── ...
├── EventDrivenWebApplication.Domain
│   ├── Entities
│   ├── Interfaces
│   ├── Messages
│   └── ...
└── EventDrivenWebApplication.Infrastructure
    ├── Data
    ├── Services
    ├── Messaging
    ├── Sagas
    └── ...

## Docker Compose

The `docker-compose.yml` file orchestrates the deployment of the application, defining the services required for the EventDrivenWebApplication. It includes configurations for the API, SQL Server, and RabbitMQ, enabling seamless interaction between these components in a Docker environment.

## Dockerfile

The `Dockerfile` specifies the instructions for building the EventDrivenWebApplication image. It sets up the necessary environment, installs dependencies, and configures SSL certificates to ensure secure communication. This file is essential for creating a production-ready containerized application.

## Running the Application

To build and run the application using Docker Compose, navigate to the `docker-compose` directory in PowerShell and execute the following command:

```bash
docker-compose up --build
```