using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically checks for events starting within <see cref="ReminderSettings.LookaheadMinutes"/> that haven't
/// had a reminder sent yet, and posts one to Discord. Each event is reminded exactly once (`ReminderSentAtUtc`).
/// </summary>
public class EventReminderService(
    IServiceScopeFactory scopeFactory,
    IOptions<ReminderSettings> settings,
    ILogger<EventReminderService> logger) : BackgroundService
{
    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, settings.Value.CheckIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Błąd podczas sprawdzania przypomnień o wydarzeniach");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    #endregion

    #region Private Methods

    private async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Application.Abstractions.IApplicationDbContext>();
        var discordNotifier = scope.ServiceProvider.GetRequiredService<IDiscordNotifier>();

        var now = DateTime.UtcNow;
        var lookahead = now.AddMinutes(settings.Value.LookaheadMinutes);

        var dueEvents = await dbContext.Events
            .Where(e => e.ReminderSentAtUtc == null && e.StartsAtUtc > now && e.StartsAtUtc <= lookahead)
            .ToListAsync(cancellationToken);

        foreach (var calendarEvent in dueEvents)
        {
            var minutesUntil = (int)(calendarEvent.StartsAtUtc - now).TotalMinutes;
            var location = string.IsNullOrWhiteSpace(calendarEvent.Location) ? "" : $" @ {calendarEvent.Location}";

            await discordNotifier.SendAsync(
                $"⏰ **{calendarEvent.Title}** zaczyna się za {minutesUntil} min{location}",
                cancellationToken);

            calendarEvent.ReminderSentAtUtc = now;
        }

        if (dueEvents.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion
}
