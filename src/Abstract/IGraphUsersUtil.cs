using System;
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
    /// Updates an existing user in Microsoft Graph using the provided <see cref="User"/> object.
    /// </summary>
    /// <param name="user">The <see cref="User"/> entity to update. The <c>Id</c> property must be populated.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The updated <see cref="User"/> if successful; otherwise, <c>null</c> if the update fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="user"/> does not have a valid <c>Id</c>.</exception>
    /// <exception cref="Microsoft.Graph.Models.ODataErrors.ODataError">Thrown if Microsoft Graph returns an error during the update.</exception>
    /// <exception cref="Exception">Thrown if an unexpected error occurs during the update.</exception>
    ValueTask<User?> Update(User user, CancellationToken cancellationToken = default);

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