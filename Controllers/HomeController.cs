using GroupFourTaskMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using System;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting.Internal;
using System.Collections;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
            // Read the file where the book information is stored. 
            // If the task involved adding or changing this data, a database would be used instead. 

            IList<Book> books = getBooks();
            
            
            if (books == null)
            {
                return Problem("Books json doesn't exist");
            }
            foreach (Book book in books)
            {
                ReservationStatus checkStatus = getReservationStatus().SingleOrDefault(res => res.BookID.Equals(book.id));
                book.resID = null;
                if (checkStatus != null)
                {
                    book.resID = checkStatus.ReservationID;
                }
            }
            // If no search, return all books
            if (searchString == null)
            {
                return View(books);
            }
            else
            {
                IEnumerable<Book> searchedBooks = books.Where(book => book.title.Contains(searchString));
                return View(searchedBooks);
            }
        }

        private IList<Book> getBooks()
        {
            string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\Books.json");
            string jsonText = System.IO.File.ReadAllText(path);

            JArray jsonBooks = JArray.Parse(jsonText);
            IList<Book> books = ((JArray)jsonBooks).ToObject<IList<Book>>();
            return books;
        }

        public IList<ReservationStatus> getReservationStatus()
        {
            string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\ReservationStatus.csv");
            string statusText = System.IO.File.ReadAllText(path);
            IList<ReservationStatus> statuses = new List<ReservationStatus>();
            foreach (string statusRow in statusText.Split('\n'))
            {
                if (!string.IsNullOrEmpty(statusRow))
                {
                    ReservationStatus newStatus = new ReservationStatus();
                    string[] statusCols = statusRow.Split(",");
                    newStatus.BookID = statusCols[0].Trim();
                    newStatus.ReservationID = statusCols[1].Trim();
                    statuses.Add(newStatus);

                }
            }
            return statuses;
        }
        public IActionResult Cancel(string bookID)
        {
            string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\ReservationEvents.csv");

            // Send cancel event
            if (true)
            {
                using (FileStream resFile = new FileStream(path, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter resWriter = new StreamWriter(resFile))
                    {
                        // Trigger event
                        resWriter.WriteLine(DateTime.Now.ToString() + ", Cancelled, " + bookID + ", ");
                    }
                }
            }
            updateReservationStatus();
            return RedirectToAction("Index");
        }
        public void updateReservationStatus()
        {
            // Read through the event log to create a snapshot of the current reservation status. 
            // This can be done asynchronously for a more responsive program, but with the size of this test, isn't needed. 
            string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\ReservationEvents.csv");
            string eventsText = System.IO.File.ReadAllText(path);

            IList<ReservationEvent> events = new List<ReservationEvent>();
            IList<ReservationStatus> statuses = new List<ReservationStatus>();
            foreach (string eventRow in eventsText.Split('\n'))
            {
                if (!string.IsNullOrEmpty(eventRow))
                {
                    ReservationEvent newEvent = new ReservationEvent();
                    string[] eventCols = eventRow.Split(',');
                    newEvent.timestamp = eventCols[0].Trim();
                    newEvent.eventType = eventCols[1].Trim();
                    newEvent.BookID = eventCols[2].Trim();
                    newEvent.ReservationID = eventCols[3].Trim();
                    events.Add(newEvent);
                   
                    var resToUpdate = statuses.SingleOrDefault(status => status.BookID.Equals(newEvent.BookID));
                    
                    if (resToUpdate != null)
                    {
                        if (newEvent.eventType.Trim() == "Cancelled")
                        {
                            statuses.Remove(resToUpdate);
                        }
                    }
                    else 
                    {
                        if (newEvent.eventType.Trim() == "Reserved")
                        {
                            ReservationStatus newStatus = new ReservationStatus();
                            newStatus.BookID = newEvent.BookID;
                            newStatus.ReservationID = newEvent.ReservationID;
                            statuses.Add(newStatus);
                        }
                    }
                }
            }
                
            string statusPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\ReservationStatus.csv");
            using (FileStream statusFile = new FileStream(statusPath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter statusWriter = new StreamWriter(statusFile))
                {
                    foreach(ReservationStatus status in statuses)
                    {
                        statusWriter.WriteLine(status.BookID + ", " + status.ReservationID);
                    }
                }
            }
            


        }
        public IActionResult Reserve(string bookID)
        {
            // perform action
            string path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data\\ReservationEvents.csv");            
            ReservationEvent newRes = new ReservationEvent();
            newRes.ReservationID = Guid.NewGuid().ToString();
            newRes.BookID = bookID;
            // Check if book is reserved, if not reserve it. 
            ReservationStatus resStatus = getReservationStatus().SingleOrDefault(res => res.BookID.Equals(bookID));            
            if (resStatus == null)
            {
                    using (FileStream resFile = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter resWriter = new StreamWriter(resFile))
                        {
                            // Trigger event
                            resWriter.WriteLine(DateTime.Now.ToString() + ", Reserved, " + newRes.BookID + ", " + newRes.ReservationID);
                        }
                    }


            }
            updateReservationStatus();
            return RedirectToAction("Index");
        }
        public IActionResult ShowReservationButton(string bookID)
        {
            ReservationStatus resStatus = getReservationStatus().SingleOrDefault(res => res.BookID.Equals(bookID));
            ViewBag.bookid = bookID;
            return PartialView("_ReservationButtonPartial", resStatus);
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