using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Polly;
using Polly.Retry;
using Soenneker.Exceptions.Suite;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.Enumerable;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Graph.Client.Abstract;
using Soenneker.Graph.Users.Abstract;
using Soenneker.Utils.BackgroundQueue.Abstract;
using Soenneker.Utils.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Graph.Users;

///<inheritdoc cref="IGraphUsersUtil"/>
public class GraphUsersUtil : IGraphUsersUtil
{
    private readonly IConfiguration _config;
    private readonly ILogger<GraphUsersUtil> _logger;
    private readonly IBackgroundQueue _backgroundQueue;
    private readonly IGraphClientUtil _graphClientUtil;

    public GraphUsersUtil(IConfiguration config, ILogger<GraphUsersUtil> logger, IBackgroundQueue backgroundQueue, IGraphClientUtil graphClientUtil)
    {
        _config = config;
        _logger = logger;
        _backgroundQueue = backgroundQueue;
        _graphClientUtil = graphClientUtil;
    }

    public async ValueTask<User> Create(string firstName, string lastName, string role, string email, string password, bool forceChangePassword = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("^^ GRAPHUSERUTIL: Creating user {email} ...", email);

        var user = new User
        {
            AccountEnabled = true,
            Surname = lastName,
            GivenName = firstName,
            DisplayName = $"{firstName} {lastName}",
            PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = forceChangePassword,
                Password = password
            },
            Identities =
            [
                new ObjectIdentity
                {
                    SignInType = "emailAddress",
                    Issuer = _config.GetValueStrict<string>("Azure:AzureAd:NonCustomDomain"),
                    IssuerAssignedId = email
                }
            ],
            JobTitle = role,
            PasswordPolicies = "DisablePasswordExpiration"
        };

        User? result;

        try
        {
            result = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users.PostAsync(user,
                                                                                       requestConfiguration =>
                                                                                       {
                                                                                           requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                                                                                       }, cancellationToken)
                                                                                   .NoSync();
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError e)
        {
            string? reason = e.Error?.Message;

            _logger.LogError(e, "^^ GRAPHUSERUTIL: Could not create AAD user: {reason}", reason);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "^^ GRAPHUSERUTIL: Could not create AAD user");
            throw;
        }

        _logger.LogDebug("^^ GRAPHUSERUTIL: Created user ({email}), it has ID {id}", email, result!.Id);

        if (result.Id.IsNullOrEmpty())
            throw new Exception($"^^ GRAPHUSERUTIL: User ID not returned after creation: {email}");

        User? newUser = await Get(result.Id, cancellationToken).NoSync();

        if (newUser == null)
            throw new Exception($"^^ GRAPHUSERUTIL: Unable to retrieve AAD user after creation: {email}");

        return newUser;
    }

    public async ValueTask<User?> Update(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id.IsNullOrEmpty())
            throw new ArgumentException("^^ GRAPHUSERUTIL: User ID must be populated to perform update", nameof(user));

        _logger.LogDebug("^^ GRAPHUSERUTIL: Updating user ({id}) ...", user.Id);

        try
        {
            User? updatedUser = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users[user.Id]
                                                                                              .PatchAsync(user,
                                                                                                  requestConfiguration =>
                                                                                                  {
                                                                                                      requestConfiguration.Headers.Add("ConsistencyLevel",
                                                                                                          "eventual");
                                                                                                  }, cancellationToken)
                                                                                              .NoSync();

            _logger.LogDebug("^^ GRAPHUSERUTIL: Successfully updated user ({id})", user.Id);

            return updatedUser;
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError e)
        {
            string? reason = e.Error?.Message;
            _logger.LogError(e, "^^ GRAPHUSERUTIL: Failed to update user ({id}): {reason}", user.Id, reason);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "^^ GRAPHUSERUTIL: Unexpected error updating user ({id})", user.Id);
            throw;
        }
    }

    public async ValueTask<User?> Get(string id, CancellationToken cancellationToken = default)
    {
        User? user = null;

        try
        {
            AsyncRetryPolicy? retryPolicy = Policy.Handle<Exception>()
                                                  .WaitAndRetryAsync(3, retryAttempt =>
                                                          TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // exponential back-off with jitter
                                                          + TimeSpan.FromMilliseconds(RandomUtil.Next(0, 500)),
                                                      (exception, timespan, retryCount) =>
                                                      {
                                                          _logger.LogError(exception,
                                                              "^^ GRAPHUSERUTIL: Failed to retrieve Graph user, waiting for eventuality {delay}s ... count: {retryCount}",
                                                              timespan.Seconds, retryCount);
                                                      });

            await retryPolicy.ExecuteAsync(async () => { user = await InternalGet(id, cancellationToken).NoSync(); }).NoSync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "^^ GRAPHUSERUTIL: Final error. Could not retrieve AAD user: {reason}", e.Message);

            return null;
        }

        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieved user ({id})", id);
        return user;
    }

    private async ValueTask<User?> InternalGet(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieving user ({id}) ...", id);

        User? user = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users[id]
                                                                                   .GetAsync(requestConfiguration =>
                                                                                   {
                                                                                       requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                                                                                       requestConfiguration.QueryParameters.Select =
                                                                                       [
                                                                                           "id", "displayName", "createdDateTime", "identities", "jobTitle",
                                                                                           "givenName",
                                                                                           "surname"
                                                                                       ];
                                                                                   }, cancellationToken)
                                                                                   .NoSync();

        return user;
    }

    public async ValueTask<List<User>> GetAll(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieving all users...");

        UserCollectionResponse? getUserResponse = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users.GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Select = ["id", "displayName", "createdDateTime", "identities", "jobTitle", "givenName", "surname"];
            }, cancellationToken)
            .NoSync();

        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieved {count} users", getUserResponse!.Value!.Count);

        var users = new List<User>();

        PageIterator<User, UserCollectionResponse>? pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(
            await _graphClientUtil.Get(cancellationToken).NoSync(), getUserResponse, user =>
            {
                users.Add(user);
                return true;
            });

        await pageIterator.IterateAsync(cancellationToken).NoSync();

        _logger.LogDebug("^^ GRAPHUSERUTIL: Finished retrieving {count} total users", getUserResponse.Value.Count);

        return users;
    }

    public async ValueTask<User?> GetFirst(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieving first user...");

        UserCollectionResponse? getUserResponse = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users.GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Select = ["id", "displayName", "createdDateTime", "identities", "jobTitle", "givenName", "surname"];
                requestConfiguration.QueryParameters.Top = 1;
            }, cancellationToken)
            .NoSync();

        if (getUserResponse == null || getUserResponse.Value.IsNullOrEmpty())
        {
            _logger.LogWarning("^^ GRAPHUSERUTIL: There are apparently no users in Graph at this time");
            return null;
        }

        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieved first user");

        return getUserResponse.Value.FirstOrDefault();
    }

    // TODO: Probably a way to grab the user via userPrincipalName instead of filtering on identities
    public async ValueTask<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieving user ({email}) ...", email);

        UserCollectionResponse? getUserResponse = await (await _graphClientUtil.Get(cancellationToken).NoSync()).Users.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = ["id", "displayName", "createdDateTime", "identities", "jobTitle", "givenName", "surname"];
                requestConfiguration.QueryParameters.Filter =
                    $"identities/any(c:c/issuerAssignedId eq '{email}' and c/issuer eq '{_config.GetValueStrict<string>("Azure:AzureAd:Domain")}')";
            }, cancellationToken)
            .NoSync();

        if (getUserResponse == null || getUserResponse.Value.IsNullOrEmpty())
        {
            _logger.LogWarning("^^ GRAPHUSERUTIL: Could not find user ({email})", email);
            return null;
        }

        _logger.LogDebug("^^ GRAPHUSERUTIL: Retrieved user");

        return getUserResponse.Value.FirstOrDefault();
    }

    public async ValueTask Delete(string id, bool skipValidation = false, CancellationToken cancellationToken = default)
    {
        if (skipValidation)
        {
            User? user = await Get(id, cancellationToken);

            if (user == null)
                throw new EntityNotFoundException("User ({id}) does not exist, cannot delete");
        }

        _logger.LogInformation("^^ GRAPHUSERUTIL: Deleting user ({id}) ...", id);

        await _backgroundQueue.QueueTask(async ct => { await (await _graphClientUtil.Get(ct).NoSync()).Users[id].DeleteAsync(null, ct); }, cancellationToken)
                              .NoSync();

        _logger.LogDebug("^^ GRAPHUSERUTIL: Deleted user ({id})", id);
    }
}