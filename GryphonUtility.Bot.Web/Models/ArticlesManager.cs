﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Models
{
    public sealed class ArticlesManager
    {
        internal ArticlesManager(Manager<SortedSet<Article>> saveManager) { _saveManager = saveManager; }

        internal static bool TryParseArticle(string text, out Article article)
        {
            article = null;
            string[] parts = text.Split(' ');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0], out DateTime date))
            {
                return false;
            }

            if (!Uri.TryCreate(parts[1], UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            article = new Article(date, uri);
            return true;
        }

        internal Task ProcessNewArticleAsync(ITelegramBotClient client, ChatId chatId, Article article)
        {
            AddArticle(article);

            string articleText = GetArticleMessageText(article);
            string firstArticleText = GetArticleMessageText(_saveManager.Data.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Добавлено: `{articleText}`.");
            sb.AppendLine();
            sb.AppendLine($"Первая статья: {firstArticleText}");

            return client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
        }

        internal Task SendFirstArticleAsync(ITelegramBotClient client, ChatId chatId)
        {
            _saveManager.Load();

            string text = GetArticleMessageText(_saveManager.Data.First());
            return client.SendTextMessageAsync(chatId, text);
        }

        internal Task DeleteFirstArticleAsync(ITelegramBotClient client, ChatId chatId)
        {
            _saveManager.Load();

            Article article = _saveManager.Data.First();
            string articleText = GetArticleMessageText(article);

            _saveManager.Data.Remove(article);

            _saveManager.Save();

            string firstArticleText = GetArticleMessageText(_saveManager.Data.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Удалено: `{articleText}`.");
            sb.AppendLine();
            sb.AppendLine($"Первая статья: {firstArticleText}");

            return client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
        }

        private void AddArticle(Article article)
        {
            _saveManager.Load();

            _saveManager.Data.Add(article);

            _saveManager.Save();
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private readonly Manager<SortedSet<Article>> _saveManager;
    }
}
