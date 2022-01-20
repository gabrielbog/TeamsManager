using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace TeamsManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //XDocument xDocument = XDocument.Load("data.xml");
            //xDocument.Descendants("item").Select(p => new
            //{
            //    id = p.Attribute("id").Value
            //}).ToList().ForEach(p => {
            //    Console.WriteLine("Id:" + p.id);
            //});
            
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
        
        public ActionResult Contact()
        {
            return View();
        }
    }
}