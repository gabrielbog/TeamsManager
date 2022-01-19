using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;
using System.Data.Entity;
using TeamsManager.Models;

namespace TeamsManager.Controllers
{
    public class PlanController : Controller
    {
        private PlanDbContext planDbCtx = new PlanDbContext();

        // GET: Plan
        public ActionResult Index()
        {
            //is the user logged in?
            if (Session != null && Session["UserID"] != null && Session["UserName"] != null)
            {
                //select this specific user's plans
                var res = from plan in planDbCtx.Plans
                          where plan.IdUser == (int)Session["UserId"]
                          select plan;

                //return this list
                return View(res.ToList());
            }

            //otherwise
            ModelState.AddModelError("", "Please log in first.");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": User not logged in");

            return RedirectToAction("Index");
        }

        //
        // Create
        //
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(PlanModel plan)
        {
            if (ModelState.IsValid)
            {
                //check date
                int res = DateTime.Compare(plan.Deadline, DateTime.Now);
                if (res < 0)
                {
                    ModelState.AddModelError("", "The deadline can't be earlier than the current time.");
                    Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Impossible Deadline");

                    return View(plan);
                }

                //add otherwise
                plan.IdUser = (int) Session["UserId"];
                plan.UserName = Session["UserName"].ToString();

                planDbCtx.Plans.Add(plan);
                planDbCtx.SaveChanges();

                return RedirectToAction("Index");
            }

            //error
            ModelState.AddModelError("", "An error has occured.");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Plan Creator Error");

            return View(plan);
        }

        //
        // Delete
        //
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": 404 Error");
                return HttpNotFound();
            }

            PlanModel plan = planDbCtx.Plans.Find(id.Value);
            if (plan == null)
            {
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": 404 Error");
                return HttpNotFound();
            }

            return View(plan);
        }

        [HttpPost]
        public ActionResult Delete(PlanModel plan)
        {
            if (ModelState.IsValid)
            {
                planDbCtx.Entry(plan).State = EntityState.Deleted;
                planDbCtx.SaveChanges();
                return RedirectToAction("Index");
            }

            //error
            ModelState.AddModelError("", "An error has occured.");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Plan Deletion Error");
            
            return View(plan);
        }
    }
}