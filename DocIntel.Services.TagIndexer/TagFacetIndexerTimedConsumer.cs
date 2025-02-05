using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocIntel.Core.Authorization;
using DocIntel.Core.Exceptions;
using DocIntel.Core.Logging;
using DocIntel.Core.Models;
using DocIntel.Core.Repositories;
using DocIntel.Core.Services;
using DocIntel.Core.Settings;
using DocIntel.Core.Utils.Indexation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocIntel.Services.TagIndexer;

public class TagFacetIndexerTimedConsumer : DynamicContextConsumer, IHostedService, IDisposable
{
    private readonly ITagFacetIndexingUtility _indexingUtility;
    private readonly ILogger<TagFacetIndexerTimedConsumer> _logger;
    private readonly ITagFacetRepository _facetRepository;
    private Timer? _timer;
    private int executionCount;

    public TagFacetIndexerTimedConsumer(ILogger<TagFacetIndexerTimedConsumer> logger,
        ITagFacetRepository facetRepository,
        ApplicationSettings appSettings,
        IServiceProvider serviceProvider,
        AppUserClaimsPrincipalFactory userClaimsPrincipalFactory,
        ITagFacetIndexingUtility indexingUtility, UserManager<AppUser> userManager)
        : base(appSettings, serviceProvider, userClaimsPrincipalFactory, userManager)
    {
        _logger = logger;
        _facetRepository = facetRepository;
        _indexingUtility = indexingUtility;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"TagFacetIndexerTimedConsumer is started and will check indexing every {_appSettings.Schedule.IndexingFrequencyCheck} minutes");

        var fromMinutes = TimeSpan.FromMinutes(_appSettings.Schedule.IndexingFrequencyCheck);
        _timer = new Timer(DoWork, null, fromMinutes, fromMinutes);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TagFacetIndexerTimedConsumer is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        var count = Interlocked.Increment(ref executionCount);
        _logger.LogInformation($"TagFacetIndexerTimedConsumer executes (Count: {count})");

        using var scope = _serviceProvider.CreateScope();
        if (_appSettings == null || _appSettings.Schedule == null
                                 || _appSettings.Schedule.MaxIndexingDelay <= 0) {
            _logger.LogWarning(
                "TagFacetIndexerTimedConsumer failed to run due to incorrect configuration");
            return;
        }

        using var ambientContext = await GetAmbientContext(scope.ServiceProvider);
        if (ambientContext == null || _facetRepository == null)
        {
            _logger.LogWarning(
                "TagFacetIndexerTimedConsumer failed to run as it cannot access the database");
            return;
        } 
        
        var listAsync = await _facetRepository.GetAllAsync(ambientContext,
                _ => _.Where(__ => __.LastIndexDate == DateTime.MinValue 
                                   || __.LastIndexDate == DateTime.MaxValue 
                                   || (__.Tags.Any() && __.Tags.Max(___ => ___.ModificationDate) - __.LastIndexDate > TimeSpan.FromMinutes(_appSettings.Schedule.MaxIndexingDelay))
                                   || __.ModificationDate - __.LastIndexDate > TimeSpan.FromMinutes(_appSettings.Schedule.MaxIndexingDelay)))
            .ToListAsync();

        foreach (var facet in listAsync)
            try
            {
                _indexingUtility.Update(facet);
                facet.LastIndexDate = DateTime.UtcNow;
            }
            catch (UnauthorizedOperationException)
            {
                _logger.Log(LogLevel.Warning,
                    TagIndexerMessageConsumer.Unauthorized,
                    new LogEvent(
                            $"User '{ambientContext.CurrentUser.UserName}' attempted to retrieve tag without legitimate rights")
                        .AddUser(ambientContext.CurrentUser)
                        .AddProperty("tag.id", facet.FacetId),
                    null,
                    LogEvent.Formatter);
            }
            catch (NotFoundEntityException)
            {
                _logger.Log(LogLevel.Warning,
                    TagIndexerMessageConsumer.EntityNotFound,
                    new LogEvent(
                            $"User '{ambientContext.CurrentUser.UserName}' attempted to retrieve a non-existing tag")
                        .AddUser(ambientContext.CurrentUser)
                        .AddProperty("tag.id", facet.FacetId),
                    null,
                    LogEvent.Formatter);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning,
                    TagIndexerMessageConsumer.EntityNotFound,
                    new LogEvent($"User '{ambientContext.CurrentUser.UserName}' could not index the tag: " +
                                 e.GetType() + " (" + e.Message + ")")
                        .AddUser(ambientContext.CurrentUser)
                        .AddProperty("tag.id", facet.FacetId)
                        .AddException(e),
                    null,
                    LogEvent.Formatter);
                _logger.LogDebug(e.StackTrace);
            }
        await ambientContext.DatabaseContext.SaveChangesAsyncWithoutNotification();
        _logger.LogInformation($"TagFacetIndexerTimedConsumer successfully executed");
    }
}