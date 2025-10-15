# DroneFleet
## Overview
This is a small C# console application (homework project) that models a fleet of drones and the logic used to create, manage and operate them. The project is written for .NET 9 and is organized to demonstrate object-oriented design, SOLID principles, and several common design patterns without depending on frameworks.

The README below explains the project structure, the runtime flow, where core OOP concepts are applied, and how SOLID and common design patterns appear in the codebase.

## Project structure

- `Program.cs` — application entry point.
- `ConsoleUI/` — user-facing console layer: `Menu/` holds the `IMenuAction` implementations and registry, `InputHelpers.cs` manages prompts, and `DroneDisplayFormatter.cs` centralizes console output formatting.
- `Contracts/` — DTOs and options used to pass data into factory/creation methods (e.g., `DroneCreationOptions`).
- `Models/` — domain model classes:
	- `Drone.cs` — base/domain class for drones.
	- `DeliveryDrone.cs`, `RacingDrone.cs`, `SurveyDrone.cs` — concrete drone types inheriting from `Drone`.
	- `Interfaces/` — capability interfaces (e.g., `ICargoCarrier`, `IFlightControl`, `INavigable`, `IPhotoCapture`, `ISelfTest`).
- `Services/` — business logic and orchestration:
	- `DroneFactory.cs`, `DroneCreationRegistry.cs`, `DroneManager.cs`, `DroneRepository.cs` — core services that create drones, keep the registry of creators, and store active drone instances.
	- `CapabilityRegistry.cs`, `Capabilities/` — registry and capability action handlers wired into the UI.
	- `Creators/` — individual creator classes for each drone type (`DeliveryDroneCreator`, `RacingDroneCreator`, `SurveyDroneCreator`).
	- `Interfaces/` — service-level interfaces (`IDroneCreator`, `IDroneFactory`, `IDroneManager`, `IDroneRepository`, `ICapabilityActionHandler`) used for decoupling and dependency inversion.

## How the application works (high-level logic)

1. The program starts in `Program.cs` and initializes the console UI (`ConsoleUI/DroneFleetApp.cs`).
2. The UI receives user input (via `InputHelpers.cs`) and delegates creation requests to a factory service.
3. `DroneFactory` consults the `DroneCreationRegistry` (mapping of types to creators) to select an appropriate `IDroneCreator` and create a concrete `Drone` instance using `DroneCreationOptions` from `Contracts`.
4. Created drone instances implement specific capability interfaces and are managed by `DroneManager`, which performs higher-level operations (registering, listing, invoking behaviors).
5. Menu actions (`ConsoleUI/Menu/Actions/*.cs`) operate through the `MenuActionRegistry`, interact only with service interfaces, and dispatch capability actions through the service-level registry, keeping presentation separate from business logic.

## OOP concepts used

- Encapsulation: Domain state and behavior are organized inside `Drone` and concrete drone classes. Properties and methods encapsulate drone behavior.
- Inheritance: `DeliveryDrone`, `RacingDrone`, and `SurveyDrone` inherit from the base `Drone` class to share common behavior and state.
- Polymorphism: Code treats drones through base types and interfaces (e.g., `Drone`, `IPhotoCapture`, `ICargoCarrier`) so different drone types can be used interchangeably where appropriate.
- Composition: Capabilities are modeled as interfaces that concrete drones implement, favoring composition of behavior over large monolithic classes.

## SOLID principles mapping

- Single Responsibility Principle (SRP): Each class has a focused responsibility: creators construct drones, `DroneManager` manages lifecycle and collection, UI components handle input and presentation.
- Open/Closed Principle (OCP): The creation system is open for extension — new drone types can be added by creating a new `IDroneCreator` implementation and registering it — without modifying existing factory logic.
- Liskov Substitution Principle (LSP): Concrete drone types can be substituted anywhere the base `Drone` type or capability interfaces are expected.
- Interface Segregation Principle (ISP): Multiple small, focused interfaces in `Models/Interfaces` avoid forcing implementers to provide methods they don't need. Service interfaces in `Services/Interfaces` also follow this pattern.
- Dependency Inversion Principle (DIP): High-level service code depends on abstractions (`IDroneCreator`, `IDroneFactory`, `IDroneManager`, `IDroneRepository`) rather than concrete implementations. The `ConsoleUI` also communicates through service interfaces.

## Design patterns observed

- Factory / Creator pattern: `DroneFactory` and the `Creators/*` classes encapsulate how concrete drone objects are constructed.
- Registry pattern: `DroneCreationRegistry` acts as a mapping/registry of available creators keyed by type; it centralizes creator discovery and supports extension.
- Dependency Injection (DI) style: The project favors programming to interfaces and passing concrete implementations into consumers (via constructors or service wiring in `Program.cs`), which is consistent with DI principles even if a DI container isn't used.
- Strategy/Capability pattern (interface-based): By modeling capabilities as interfaces (e.g., `IPhotoCapture`, `ICargoCarrier`), the code uses a strategy-like approach where behavior can be swapped or tested independently.
- Command-style menu actions: `MenuActionRegistry` coordinates discrete `IMenuAction` implementations, and capability handlers encapsulate specific behaviors, keeping the UI command flow modular and testable.

Note: The project deliberately keeps patterns minimal and explicit to demonstrate understanding rather than relying on heavy frameworks.

## Files of interest (where to look for these concepts)

- `Models/Drone.cs` and `Models/*Drone.cs` — inheritance, polymorphism, encapsulation.
- `Models/Interfaces/*.cs` — interface segregation and capability-based design.
- `Services/DroneFactory.cs` — factory/creation logic and OCP.
- `Services/DroneCreationRegistry.cs` — registry and extension point for new creators.
- `Services/CapabilityRegistry.cs` — capability handler discovery for the UI.
- `Services/Capabilities/*.cs` — capability action handlers.
- `Services/Creators/*.cs` — concrete creator implementations.
- `Services/DroneManager.cs`, `Services/DroneRepository.cs` — lifecycle coordination and persistence layer for active drones (SRP, DIP in relation to interfaces).
- `ConsoleUI/*` — presentation layer separated from business logic (separation of concerns), with `Menu/Actions/*.cs` defining menu commands.

## How to run (quick)

From the project root, run the application with the .NET CLI:

```powershell
dotnet run --project DroneFleet.csproj
```

This will start the console UI where you can create and manage drones.

## Homework notes

This repository is structured to show clear separation between presentation, domain, and services. It demonstrates core OOP techniques and the SOLID design principles. The design patterns used are lightweight and focused on extensibility and testability, important goals for maintainable code.
