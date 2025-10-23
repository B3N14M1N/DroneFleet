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
updateRegistry.Register(new BatteryUpdateHandler(), "set-battery");
updateRegistry.Register(new TakeOffUpdateHandler(), "launch", "fly", "take-off");
updateRegistry.Register(new LandUpdateHandler(), "ground", "touchdown");
updateRegistry.Register(new WaypointUpdateHandler(), "wp", "way-point");
updateRegistry.Register(new CargoLoadUpdateHandler(), "cargo", "set-load");
updateRegistry.Register(new CargoUnloadUpdateHandler(), "clear-load", "drop");
updateRegistry.Register(new CapturePhotoUpdateHandler(), "snap", "capture");

registry.Register(help, "?");
registry.Register(new ExitCommand(), "quit");
registry.Register(new ImportCommand());
registry.Register(new ListCommand(), "ls");
registry.Register(new StatsCommand());
registry.Register(new ChargeCommand());
registry.Register(new UpdateCommand(updateRegistry), "set");
registry.Register(new ExportCommand());

var app = new FleetConsoleApp(registry, fleetService);
await app.RunAsync();
