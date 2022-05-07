using System;
using Core.Attributes;
using CoreCache.ApiForTest.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CoreCache.ApiForTest.Controllers;

public class HomeController : Controller
{
    [Caching(typeof(Cacheable), "page", "view:{id}", 2)]
    public IActionResult Index(string id)
    {
        return View(DataUtils.GetData());
    }

    [Evicting(typeof(CacheEvict), new[] { "page" }, "view:{id}")]
    public IActionResult Tow(string id)
    {
        return View(DataUtils.GetData());
    }
}