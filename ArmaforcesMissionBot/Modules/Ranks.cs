﻿using ArmaforcesMissionBot.Attributes;
using ArmaforcesMissionBot.DataClasses;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaforcesMissionBot.Modules
{
    [Name("Rangi")]
    public class Ranks : ModuleBase<SocketCommandContext>
    {
        public IServiceProvider _map { get; set; }
        public DiscordSocketClient _client { get; set; }
        public Config _config { get; set; }

        [Command("rekrutuj")]
        [Summary("Przydziela rangę rekrut.")]
        [RequireRank(RanksEnum.Recruiter)]
        public async Task Recruit(IGuildUser user)
        {
            Console.WriteLine($"[{DateTime.Now.ToString()}] {Context.User.Username} called recruit command");
            if (user.RoleIds.Contains(_config.RecruitRole))
                await ReplyAsync($"Przecież {user.Mention} już został zrekrutowany.");
            else if (user.RoleIds.Contains(_config.SignupRole))
                await ReplyAsync($"Przecież {user.Mention} jest już w {Context.Guild.GetRole(_config.SignupRole).Mention}!");
            else
            {
                await user.AddRoleAsync(Context.Guild.GetRole(_config.RecruitRole));
                await ReplyAsync($"Gratulujemy przyjęcia {user.Mention} w grono rekrutów! Zapraszamy na swoją pierwszą misję z nami, wtedy otrzymasz rangę {Context.Guild.GetRole(_config.SignupRole).Mention}!\n" +
                    $"Polecamy też sprawdzić kanał {Context.Guild.GetTextChannel(_config.RecruitInfoChannel).Mention}.\n" +
                    $"W razie pytań pisz na {Context.Guild.GetTextChannel(_config.RecruitAskChannel).Mention}.\n" +
                    $"Twoim opiekunem do momentu dołączenia do grupy jest {Context.User.Mention}.");
            }

            await Context.Message.DeleteAsync();
        }
    }
}
