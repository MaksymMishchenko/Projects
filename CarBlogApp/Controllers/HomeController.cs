﻿using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CarBlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactForm form)
        {
            if (string.IsNullOrEmpty(form.Name))
            {
                ModelState.AddModelError("Name", "Input your name, please");
            }

            if (form.Email != null && !new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").IsMatch(form.Email))
            {
                ModelState.AddModelError("Email", "Input correct email, please");
            }

            if (string.IsNullOrEmpty(form.Message))
            {
                ModelState.AddModelError("Message", "Input your message, please");
            }
            if (ModelState.IsValid)
            {
                return View("Success");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}