# DroneFleet

Modern console-based drone fleet management sample built on .NET 9.

## Highlights
- Command-driven console UI (no interactive menus): `list`, `import`, `export`, `action`, `stats`, `help`, `clear`, `exit`.
- Unified Result pattern (Result / Result<T>) with domain-specific `ResultCodes` mapped to HTTP-style status strings (200 OK, 400 Bad Request, 404 Not Found, 409 Conflict, 500 Error).
- Colored console output: successes green, validation warnings yellow, errors red, duplicates treated as conflict (409) but grouped.
- Asynchronous import/export operations (JSON & CSV import, JSON/CSV export) with path auto-resolution (solution root, working directory, base directory).
- Multi-format import (`ImportAsync`) auto-detects by extension: `.csv` parsed via tokenizer + `DroneCsvParser`; `.json` deserialized to an array of `DroneSnapshot`.
- Aggregated import reporting: single success header + per-issue status-coded lines (duplicate, not found, validation).
- Rich fleet analytics via `stats` sub-modes (battery, battery-below <threshold>, cargo-remaining, kind, airborne, grounded, top N queries) powered by LINQ extension methods.
- Partial `DroneFleetService` split for maintainability (core operations vs import/export logic).

## Layers
- Domain: Pure models (`Drone`, `DeliveryDrone`, `SurveyDrone`, `RacingDrone`), value objects & snapshots (`DroneSnapshot`), analytics summaries, LINQ extensions, Result types.
- Infrastructure: In-memory repository, file I/O (CSV tokenizer & parser), fleet service (partial) implementing domain operations + import/export.
- App: Console commands + formatting utilities (HTTP status formatter, colorized writer).

## Import Format Details
### CSV
Header required:
```
Id,Name,Kind,BatteryPercent,IsAirborne,LoadKg,WaypointLat,WaypointLon,PhotoCount
```
Rows may omit optional numeric values (leave blank). Boolean airborne accepts: true/false, 1/0, yes/no, y/n.

### JSON
Array of objects matching `DroneSnapshot` properties:
```json
[
  {
    "Id": 1,
    "Name": "Alpha",
    "Kind": "Delivery",
    "BatteryPercent": 87.5,
    "IsAirborne": false,
    "LoadKg": 3.2,
    "WaypointLat": 51.501,
    "WaypointLon": -0.142,
    "PhotoCount": null
  }
]
```

## Stats Examples
- `stats battery` – list drones ordered by battery (desc).
- `stats battery-below 30` – drones below 30%.
- `stats cargo-remaining` – delivery drones with remaining capacity.
- `stats kind` – counts per kind.
- `stats airborne` / `stats grounded` – filter by flight state.
- `stats battery top 5` – top 5 by battery.

## Action Verbs
`action <droneId> <verb> [args]`
Supported verbs: `charge`, `battery`, `takeoff`, `land`, `waypoint <lat> <lon>`, `load <kg>`, `unload`, `photo`.

## Build & Run
From solution root:
```
dotnet run --project src/DroneFleet.App
```
Type `help` or `help import` for command usage details.

