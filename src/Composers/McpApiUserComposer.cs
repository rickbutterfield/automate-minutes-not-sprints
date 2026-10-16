using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Webinar.Composers;

/// <summary>
/// Demo-site plumbing, not part of the talk: in Development, makes sure the API users from
/// "Demo:McpApiUser" (what the Umbraco MCP servers log in as, with client credentials) and
/// "Demo:AutomateServiceUser" (the Automate workspaces' service account, no credentials)
/// exist, so a fresh database works without clicking through the backoffice.
/// </summary>
public sealed class McpApiUserComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, EnsureMcpApiUserHandler>();
}

public sealed class EnsureMcpApiUserHandler(
    IHostEnvironment hostEnvironment,
    IRuntimeState runtimeState,
    IConfiguration configuration,
    IUserService userService,
    IUserGroupService userGroupService,
    IBackOfficeUserClientCredentialsManager clientCredentialsManager,
    ILogger<EnsureMcpApiUserHandler> logger)
    : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (!hostEnvironment.IsDevelopment() || runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        await EnsureApiUserAsync(configuration.GetSection("Demo:McpApiUser"));
        await EnsureApiUserAsync(configuration.GetSection("Demo:AutomateServiceUser"));
    }

    private async Task EnsureApiUserAsync(IConfigurationSection section)
    {
        string? email = section["Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        IUser? user = userService.GetByEmail(email);
        if (user is null)
        {
            IUserGroup? adminGroup = await userGroupService.GetAsync(Constants.Security.AdminGroupAlias);
            var createModel = new UserCreateModel
            {
                Email = email,
                UserName = email,
                Name = section["Name"] ?? email,
                Kind = UserKind.Api,
                UserGroupKeys = adminGroup is null ? new HashSet<Guid>() : new HashSet<Guid> { adminGroup.Key },
            };

            var created = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, approveUser: true);
            if (!created.Success || created.Result.CreatedUser is null)
            {
                logger.LogWarning("Could not create the API user {Email}: {Status}", email, created.Status);
                return;
            }

            user = created.Result.CreatedUser;
            logger.LogInformation("Created the API user {Email}", email);
        }

        string? clientId = section["ClientId"];
        string? clientSecret = section["ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return;
        }

        IEnumerable<string> existingClientIds = await clientCredentialsManager.GetClientIdsAsync(user.Key);
        if (existingClientIds.Contains(clientId))
        {
            return;
        }

        var saved = await clientCredentialsManager.SaveAsync(user.Key, clientId, clientSecret);
        if (saved.Success)
        {
            logger.LogInformation("Added client credentials {ClientId} to the API user {Email}", clientId, email);
        }
        else
        {
            logger.LogWarning("Could not add client credentials {ClientId} to the API user {Email}: {Status}", clientId, email, saved.Result);
        }
    }
}
