# LightList
An over-engineered but simple & distraction free task management application, optimised for those with ADHD.

LightList is a cross-platform task management application built with .NET MAUI for use on iOS and macOS (MacCatalyst)
devices, with cross-platform synchronization achieved through AWS (Cognito, AppSync, DynamoDB). 

## Features
- Create, update, and manage tasks.
- Synchronisation between devices (iOS, macOS).
- Cross-platform UI built with .NET MAUI and MVVM.
- Cloud backend powered by AWS AppSync (GraphQL API).
- Clean Architecture and SOLID principles.

## Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- .NET MAUI development workloads
- An Apple device or simulator for iOS/macOS builds
- AWS Account (optional, for remote sync functionality)

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/davebyrne222/LightList.git
cd lightlist
```

### 2. Install Dependencies
Make sure you have the MAUI workload installed:

```bash
dotnet workload install maui
```

Restore NuGet packages:

```bash
dotnet restore
```

### 3. Configure AWS (Optional)
To enable remote synchronisation:
- Set up an **AWS AppSync** instance with the provided GraphQL schema (see `/docs/GraphQLSchema.graphql`).
- Add your AppSync endpoint URL and authentication details to the app's configuration (`/Constants.cs`)

If you don't configure AWS, the app will work **locally** without cloud sync.

### 4. Build and Run
You can run the project on iOS, Mac Catalyst, or Android.

Using command line:
```bash
dotnet build
dotnet maui run --device [device name]
```

Or directly from your IDE of choice (the project was developed using JetBrains Rider)
- Set the target platform (iOS / MacCatalyst / Android)
- Press **Run** ▶️

## Project Structure

| Folder | Purpose |
|--------|---------|
| `Converters/` | UI value converters for binding |
| `Database/` | Local database models and helpers |
| `Messages/` | Messaging communication between components |
| `Models/` | Core application models (Tasks, Labels) |
| `Repositories/` | Data access abstraction layers |
| `Services/` | Business logic and orchestration (Tasks, Auth, Sync) |
| `Utils/` | Helper classes like logging and constants |

## Troubleshooting
- If you encounter MAUI or iOS build errors, ensure you have the latest Xcode and Visual Studio updates.
- Confirm that your local machine has proper Apple certificates and provisioning profiles for iOS builds.

## Future Work
- Task reminders and notifications
- UI/UX improvements for larger screens
- Automated unit and integration tests