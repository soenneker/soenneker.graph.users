[![](https://img.shields.io/nuget/v/soenneker.graph.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.users/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.users/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.users/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.users/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graph.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.users/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.graph.users/actions/workflows/codeql.yml)

# Soenneker.Graph.Users

Creates, updates, queries, pages through, and queues deletion of Microsoft Graph users using an application-authenticated client.

## Install

```bash
dotnet add package Soenneker.Graph.Users
```

## Configuration

```json
{
  "Azure": {
    "AzureAd": {
      "TenantId": "<tenant ID>",
      "ClientId": "<application client ID>",
      "ClientSecret": "<client secret>",
      "NonCustomDomain": "contoso.onmicrosoft.com"
    }
  }
}
```

The app registration must have application permissions that allow the user operations your application calls.

## Register

```csharp
using Soenneker.Graph.Users.Registrars;
using Microsoft.Extensions.DependencyInjection;

services.AddGraphUsersUtilAsScoped();
```

This deliberately makes `IGraphUsersUtil` scoped while keeping `IGraphClientUtil` and the background queue singleton. A utility scope can be destroyed without tearing down the authenticated Graph client or queued work.

`AddGraphUsersUtilAsSingleton()` is available when the user-operation wrapper should also be application-wide.

## Create and query users

```csharp
User created = await graphUsers.Create(
    firstName: "Ada",
    lastName: "Lovelace",
    role: "Engineer",
    email: "ada@example.com",
    password: initialPassword,
    forceChangePassword: true,
    cancellationToken);

User? byEmail = await graphUsers.GetByEmail(
    "ada@example.com",
    cancellationToken);

List<User> allUsers = await graphUsers.GetAll(cancellationToken);
```

Created users receive an `emailAddress` identity issued by `Azure:AzureAd:NonCustomDomain`, and password expiration is disabled on the created account. `GetAll()` follows every Graph page rather than returning only the first page.

## Update and delete

```csharp
created.JobTitle = "Principal Engineer";
User? updated = await graphUsers.Update(created, cancellationToken);

await graphUsers.Delete(created.Id!, cancellationToken: cancellationToken);
```

`Update()` requires `User.Id`; Graph errors propagate. A `null` update result means Graph returned no response body, not that an error was swallowed.

`Delete()` validates that the user exists by default and then enqueues the deletion. It returns after the work is accepted by the background queue, not after Graph confirms deletion. Set `skipValidation: true` to omit the preliminary lookup.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `Create(...)` | Creates an enabled local account. | Returns the Graph representation or throws when Graph supplies none. |
| `Update(user)` | Patches the user identified by `user.Id`. | Returns Graph's response body, which may be `null`. |
| `Get(id)` | Retrieves one user with the common identity fields selected. | `null` only for a not-found response; other failures propagate. |
| `GetAll()` | Retrieves all users and follows pagination. | Returns an empty list when Graph supplies no collection. |
| `GetFirst()` | Requests one user. | Graph ordering is not defined by this package. |
| `GetByEmail(email)` | Filters mail, user principal name, and identity issuer-assigned ID. | Returns the first match or `null`. |
| `Delete(id, skipValidation)` | Optionally validates, then queues deletion. | Completion means queued, not deleted. |

Cancellation is forwarded to Graph requests and queue submission. It does not recall deletion work that has already begun in the background queue.
