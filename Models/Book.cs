using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace GroupFourTaskMVC.Models
{
    public class Book
    {
        public string id { get; set; }
        public string title { get; set; }

    }
    public class Books
    {
        public IList<Book> books { get; set; }
    }
}
