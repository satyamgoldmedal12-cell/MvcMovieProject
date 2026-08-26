using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;
using System.Diagnostics;

namespace MvcMovie.Controllers
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // TraceIdentifier से Error की unique ID मिलती है
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = requestId
            };

            // Exception को get करना अगर हो तो
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error is Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception occurred");
                errorViewModel.Message = exception.Message;
                errorViewModel.StackTrace = exception.StackTrace;
            }

            return View(errorViewModel);
        }
    }
}
