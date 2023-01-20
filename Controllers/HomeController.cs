using GroupFourTaskMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using System;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting.Internal;

namespace GroupFourTaskMVC.Controllers
{
    public class HomeController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; 
        }

        public IActionResult Index(string searchString)
        {
           string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\Books.json");
           string jsonText = System.IO.File.ReadAllText(path);

           JArray jsonBooks = JArray.Parse(jsonText);
           IList<Book> books = ((JArray)jsonBooks).ToObject<IList<Book>>();

            if (books == null)
                return Problem("Books json doesn't exist");
            else if (searchString == null)
            {
                return View(books);
            }
            else
            {
                IEnumerable<Book> searchedBooks = books.Where(book => book.title.Contains(searchString));



                return View(searchedBooks);

            }
            

        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}