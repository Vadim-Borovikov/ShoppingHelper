﻿using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ArticleCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "article";
        protected override string Description => null;

        public ArticleCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return Bot.ArticlesManager.SendFirstArticleAsync(message.Chat);
        }
    }
}
