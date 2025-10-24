using DroneFleet.App.ConsoleApp;
using DroneFleet.App.ConsoleApp.Commands;
using DroneFleet.App.ConsoleApp.Updates;
using DroneFleet.App.ConsoleApp.Updates.Handlers;
using DroneFleet.Infrastructure.Repositories;
using DroneFleet.Infrastructure.Services;

var repository = new InMemoryDroneRepository();
var fleetService = new DroneFleetService(repository);

var registry = new CommandRegistry();
var help = new HelpCommand(registry);

var updateRegistry = new DroneUpdateRegistry();
updateRegistry.Register(new BatteryChargeHandler());
updateRegistry.Register(new TakeOffUpdateHandler(), "fly");
updateRegistry.Register(new LandUpdateHandler());
updateRegistry.Register(new WaypointUpdateHandler(), "wp");
updateRegistry.Register(new CargoLoadUpdateHandler());
updateRegistry.Register(new CargoUnloadUpdateHandler());
updateRegistry.Register(new CapturePhotoUpdateHandler(), "snap");

registry.Register(help, "?");
registry.Register(new ImportCommand());
registry.Register(new ClearCommand(), "cls");
registry.Register(new ExportCommand());
registry.Register(new ListCommand());
registry.Register(new StatsCommand());
registry.Register(new ActionCommand(updateRegistry));
registry.Register(new ExitCommand(), "quit");

var app = new FleetConsoleApp(registry, fleetService);
await app.RunAsync();
