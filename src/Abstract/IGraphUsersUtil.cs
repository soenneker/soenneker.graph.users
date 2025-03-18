using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Graph.Users.Abstract;

/// <summary>
/// A utility library for Graph User related operations
/// </summary>
public interface IGraphUsersUtil
{
    /// <summary>
    /// Creates a new user in Microsoft Graph.
    /// </summary>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <param name="role">The job title or role of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="password">The password for the user account.</param>
    /// <param name="forceChangePassword">Indicates if the user must change the password on first login.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user.</returns>
    ValueTask<User> Create(string firstName, string lastName, string role, string email, string password, bool forceChangePassword = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by ID.
    /// </summary>
    /// <param name="id">The unique ID of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    [Pure]
    ValueTask<User?> Get(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all users from Microsoft Graph.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of users.</returns>
    [Pure]
    ValueTask<List<User>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first user from Microsoft Graph.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first user if available; otherwise, null.</returns>
    [Pure]
    ValueTask<User?> GetFirst(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    [Pure]
    ValueTask<User?> GetByEmail(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The unique ID of the user.</param>
    /// <param name="skipValidation">Indicates whether to skip validation before deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask Delete(string id, bool skipValidation = false, CancellationToken cancellationToken = default);
}