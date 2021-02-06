﻿using System.Diagnostics;
using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Controllers
{
    [Route("")]
    public sealed class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index([FromServices]Bot bot)
        {
            User model = await bot.GetUserAsunc();
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(model);
        }
    }
}
