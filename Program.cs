﻿using ArmaforcesMissionBot.DataClasses;
using ArmaforcesMissionBot.Handlers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace ArmaforcesMissionBot
{
    class Program
    {
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IServiceProvider    _services;
        private List<IInstallable>  _handlers;
        private Config              _config;
        private Timer               _timer;
        private int                 _statusCounter;

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig();
            //config.LogLevel = LogSeverity.Verbose;
            _client = new DiscordSocketClient(config: config);

            _client.Log += Log;

            _config = new Config();
            _config.Load();

            _client.GuildAvailable += Load;

            _services = BuildServiceProvider();

            _handlers = new List<IInstallable>();
            foreach (var handler in Assembly.GetEntryAssembly().DefinedTypes)
            {
                if (handler.ImplementedInterfaces.Contains(typeof(IInstallable)))
                {
                    _handlers.Add((IInstallable)Activator.CreateInstance(handler));
                    _handlers.Last().Install(_services);
                }
            }

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();

            _timer = new Timer();
            _timer.Interval = 5000;

            _timer.Elapsed += UpdateStatus;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private void UpdateStatus(object sender, ElapsedEventArgs e)
        {
            var signups = _services.GetService<SignupsData>();
            Game status;
            if(_statusCounter == 0 || signups.Missions.Count == 0)
                status = new Game($"Prowadzone zapisy: {signups.Missions.Where(x => !x.Editing).Count()}");
            else
            {
                var mission = signups.Missions.ElementAt(_statusCounter - 1);
                status = new Game($"Miejsc: {Helpers.MiscHelper.CountFreeSlots(mission)}/{Helpers.MiscHelper.CountAllSlots(mission)} - {mission.Title}");
            }

            if (_statusCounter >= signups.Missions.Where(x => !x.Editing).Count())
                _statusCounter = 0;
            else
                _statusCounter++;

            _client.SetActivityAsync(status);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
        .AddSingleton(_client)
        .AddSingleton<SignupsData>()
        .AddSingleton(_config)
        .BuildServiceProvider();

        private async Task Load(SocketGuild guild)
        {
            if(guild.CategoryChannels.Any(x => x.Id == _config.SignupsCategory))
            {
                var signups = guild.CategoryChannels.Single(x => x.Id == _config.SignupsCategory).Channels.Where(x => x.Id != _config.SignupsArchive);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
