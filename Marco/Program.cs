using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Extensions;
using Marco.Commands;
using Marco.Data;
using Marco.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using X10D.Hosting.DependencyInjection;

Directory.CreateDirectory("data");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("data/config.json", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

const DiscordIntents intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages | DiscordIntents.MessageContents;
builder.Services.AddDiscordClient(Environment.GetEnvironmentVariable("DISCORD_TOKEN")!, intents);
builder.Services.AddCommandsExtension((_, commands) =>
{
    commands.AddCommands<AddMacroCommand>();
    commands.AddCommands<DeleteMacroCommand>();
    commands.AddCommands<EditMacroCommand>();
    commands.AddCommands<InfoCommand>();
    commands.AddCommands<ListMacrosCommand>();
    commands.AddCommands<MacroCommand>();
});
builder.Services.ConfigureEventHandlers(events =>
{
    events.AddEventHandlers<MacroService>();
    events.AddEventHandlers<MacroListeningService>();
    events.AddEventHandlers<ModalResponseService>();
});

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<MacroCooldownService>();
builder.Services.AddSingleton<MacroListeningService>();
builder.Services.AddSingleton<ModalResponseService>();

builder.Services.AddHostedSingleton<MacroService>();

builder.Services.AddDbContext<MarcoContext>();

builder.Services.AddHostedSingleton<BotService>();

IHost app = builder.Build();
await app.RunAsync();
