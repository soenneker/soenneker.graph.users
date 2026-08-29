[![](https://img.shields.io/nuget/v/soenneker.graph.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.users/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.users/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graph.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.users/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.graph.users/actions/workflows/codeql.yml)

# Soenneker.Graph.Users

A utility library for Graph User related operations.

## Install

```bash
dotnet add package Soenneker.Graph.Users
```

## Quick start

```csharp
using Soenneker.Graph.Users.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddGraphUsersUtilAsSingleton();
```

Adds `IGraphUsersUtil` as a singleton service.

## What you get

- `IGraphUsersUtil` — A utility library for Graph User related operations.
- `GraphUsersUtilRegistrar` — A utility library for Graph User related operations.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IGraphUsersUtil.Create(firstName, lastName, role, email, password, forceChangePassword, cancellationToken)` | Creates a new user in Microsoft Graph. | The created user. |
| `IGraphUsersUtil.Update(user, cancellationToken)` | Updates an existing user in Microsoft Graph using the provided `User` object. | The updated `User` if successful; otherwise, `null` if the update fails. |
| `IGraphUsersUtil.Get(id, cancellationToken)` | Retrieves a user by ID. | The user if found; otherwise, null. |
| `IGraphUsersUtil.GetAll(cancellationToken)` | Retrieves all users from Microsoft Graph. | A list of users. |
| `IGraphUsersUtil.GetFirst(cancellationToken)` | Retrieves the first user from Microsoft Graph. | The first user if available; otherwise, null. |
| `IGraphUsersUtil.GetByEmail(email, cancellationToken)` | Retrieves a user by email address. | The user if found; otherwise, null. |
| `IGraphUsersUtil.Delete(id, skipValidation, cancellationToken)` | Deletes a user by ID. | Completes when the requested deletion has finished. |
| `GraphUsersUtilRegistrar.AddGraphUsersUtilAsSingleton(services)` | Adds `IGraphUsersUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `GraphUsersUtilRegistrar.AddGraphUsersUtilAsScoped(services)` | Adds `IGraphUsersUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Important behavior

- `IGraphUsersUtil.Update(user, cancellationToken)`: Thrown if `user` does not have a valid `Id`. Thrown if Microsoft Graph returns an error during the update. Thrown if an unexpected error occurs during the update.

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
