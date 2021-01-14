﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Config;
using MoreLinq.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Web.Models.Commands
{
    public sealed class ShopCommand : Command
    {
        protected override string Name => "shop";

        internal ShopCommand(IReadOnlyList<Item> allItems)
        {
            _allItems = allItems;
            _keyboard = GetKeyboard();
        }

        internal Task ProcessNumberAsync(ITelegramBotClient client, ChatId chatId, int number)
        {
            if (_currentItem == null)
            {
                return client.SendTextMessageAsync(chatId, "Продукт не задан!");
            }

            Add(number);

            return InvokeNextActionAsync(client, chatId);
        }

        internal override async Task ExecuteAsync(ITelegramBotClient client, ChatId chatId)
        {
            Reset();

            await client.SendTextMessageAsync(chatId, "Сейчас есть:");

            await InvokeNextActionAsync(client, chatId);
        }

        private static ReplyKeyboardMarkup GetKeyboard()
        {
            IEnumerable<KeyboardButton> buttons = Enumerable.Range(0, ButtonsTotal).Select(CreateButton);

            IEnumerable<IEnumerable<KeyboardButton>> keyboard = buttons.Batch(ButtonsPerRaw);

            return new ReplyKeyboardMarkup(keyboard);
        }

        private static KeyboardButton CreateButton(int option) => new KeyboardButton(option.ToString());

        private void Add(int amount)
        {
            if (_currentAmountIsPacks)
            {
                amount *= _currentItem.PackSize;
            }

            if (_itemAmounts.ContainsKey(_currentItem))
            {
                _itemAmounts[_currentItem] += amount;
            }
            else
            {
                _itemAmounts[_currentItem] = amount;
            }
        }

        private Task InvokeNextActionAsync(ITelegramBotClient client, ChatId chatId)
        {
            if (_currentAmountIsPacks || (_items.Count > 0))
            {
                string question = PrepareQuestion();
                return client.SendTextMessageAsync(chatId, question, replyMarkup: _keyboard);
            }

            _currentItem = null;
            string result = PrepareResult();
            return client.SendTextMessageAsync(chatId, result, disableWebPagePreview: true, replyMarkup: NoKeyboard);
        }

        private void Reset()
        {
            _items = new Queue<Item>(_allItems.OrderBy(i => i.AskOrder));
            _itemAmounts = new Dictionary<Item, int>();
            _currentItem = null;
            _currentAmountIsPacks = false;
        }

        private string PrepareQuestion()
        {
            if (_currentAmountIsPacks)
            {
                _currentAmountIsPacks = false;
                return $"{_currentItem.Name}, штуки:";
            }

            _currentItem = _items.Dequeue();
            if (_currentItem.PackSize == 1)
            {
                return $"{_currentItem.Name}:";
            }

            _currentAmountIsPacks = true;
            return $"{_currentItem.Name}, пачки:";
        }

        private string PrepareResult()
        {
            int days = GetDaysBeforeNextSunday();
            var sb = new StringBuilder();
            foreach (Item item in _itemAmounts.Keys.OrderBy(i => i.ResultOrder))
            {
                int need = item.GetRefillingAmount(_itemAmounts[item], days);
                if (item.HasHalves)
                {
                    int need2 = need / 2;
                    int need1 = need - need2;
                    sb.AppendLine($"{item.Half1}: {need1}");
                    sb.AppendLine(item.UriHalf1.AbsoluteUri);
                    sb.AppendLine();
                    sb.AppendLine($"{item.Half2}: {need2}");
                    sb.AppendLine(item.UriHalf2.AbsoluteUri);
                }
                else
                {
                    if (item.HasMass)
                    {
                        decimal mass = item.GetRefillingMass(need);
                        sb.AppendLine($"{item.Name}: {mass.ToString(CultureInfo.InvariantCulture)} кг.");
                    }
                    else
                    {
                        sb.AppendLine($"{item.Name}: {need}");
                    }
                    sb.AppendLine(item.Uri.AbsoluteUri);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static int GetDaysBeforeNextSunday() => 8 + (7 + (DayOfWeek.Sunday - DateTime.Today.DayOfWeek)) % 7;

        private const int ButtonsTotal = 12;
        private const int ButtonsPerRaw = 4;
        private static readonly ReplyKeyboardRemove NoKeyboard = new ReplyKeyboardRemove();

        private readonly ReplyKeyboardMarkup _keyboard;
        private readonly IReadOnlyList<Item> _allItems;

        private Queue<Item> _items;
        private Dictionary<Item, int> _itemAmounts;
        private Item _currentItem;
        private bool _currentAmountIsPacks;
    }
}
