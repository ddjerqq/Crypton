# Crypton

This is a sample ASP.NET Core Web API project that showcases the usage of Clean Architecture and Domain Driven Design principles. The
project is built on .NET 8 preview and utilizes various technologies, including Microsoft Identity framework with JWT tokens, Entity
Framework Core, Digital Signature from the local Diamond project (which is similar to OnlyFans' Digital Signature algorithm), MediatR,
pipeline behaviors, and EF Core interceptors.

## Project Structure

The project follows a modular structure based on Clean Architecture and Domain Driven Design, which promotes a clear separation of concerns
and facilitates maintainability and scalability.

The solution is structured into the following projects:

1. **Crypton.Domain**
    - The Domain layer holds the core business entities, aggregates, value objects, and domain repositories/interfaces.
    - It represents the heart of the application and is independent of any infrastructure or application services.


2. **Crypton.Application**
    - The Application layer contains the business logic of the application and implements use cases and application services.
    - It references the Domain layer for domain entities and repositories and interfaces with infrastructure services.


3. **Crypton.Infrastructure**
    - The Infrastructure layer provides implementations for the interfaces defined in the Application layer and interacts with external
      services.


4. **Crypton.Infrastructure.Identity**
    - The Identity project provides the user authentication and authorization functionality using Microsoft Identity framework with JWT
      tokens.


5. **Crypton.Infrastructure.Persistence**
   - The Persistence project provides the data persistence functionality using Entity Framework Core.


6. **Crypton.Infrastructure.Diamond**
    - The Diamond project provides the Digital Signature functionality that is used in the application.


7. **Crypton.WebAPI**
    - The Web API project serves as the entry point for the application and handles HTTP requests/responses.
    - It references the Application layer for business logic and MediatR for handling commands and queries.


8. **Crypton.WebUI** 
    - The WebUI project is a client application that consumes the Web API and provides a user interface for the application.


## Technologies Used

- ASP.NET Core 8 preview: The foundation of the project, providing the web API functionality.
- Clean Architecture: A software architecture design that separates concerns into layers for maintainability and testability.
- Domain Driven Design (DDD): A design approach that focuses on modeling the application based on the business domain.
- Microsoft Identity framework with JWT tokens: Handling user authentication and authorization using JWT tokens instead of cookies.
- Entity Framework Core: An Object-Relational Mapping (ORM) tool for database interactions.
- MediatR: A library for implementing the Mediator pattern to handle commands and queries.
- Pipeline Behaviors: Custom behaviors to enrich the MediatR pipeline with cross-cutting concerns.
- EF Core Interceptors: Intercepting database commands to apply additional logic or actions.
- Digital Signature from the Diamond project: Implementing digital signature functionality within the application.

## Getting Started

Eventually this project will have docker compose

## Contributing

I welcome contributions to this project. If you want to add new features, fix bugs, or suggest improvements, feel free to submit a pull
request, or open an issue.

## License

This project is licensed under the [MIT License](https://opensource.org/license/mit/).

## Acknowledgments

- The clean architecture and DDD concepts used in this project were inspired by various online resources and books.
- The Digital Signature functionality is based on the Diamond project, which itself is based on OnlyFans' Digital Signature algorithm.