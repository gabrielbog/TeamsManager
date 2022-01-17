using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using TeamsManager.Models;

namespace TeamsManager.Controllers
{
    public class UserController : Controller
    {
        private UserDbContext userDbCtx = new UserDbContext();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        //
        // LogIn
        //

        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn(UserModel user)
        {
            //pentru Vasi
            //informatii despre Session, la sfarsit: https://www.c-sharpcorner.com/article/simple-login-application-using-Asp-Net-mvc/

            //check model state
            if (ModelState.IsValid)
            {
                //hash encrypt password
                using (SHA256 hash = SHA256.Create())
                {
                    byte[] bytes;

                    bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(user.Parola));

                    StringBuilder s = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        s.Append(bytes[i].ToString("x2"));
                    }

                    user.Parola = s.ToString();
                }

                //search for password
                var res = userDbCtx.Users.SingleOrDefault(u => u.Username == user.Username && u.Parola == user.Parola);
                if(res != null)
                {
                    //cookies stuff for memorizing the user
                    Session["UserID"] = res.Id.ToString();
                    Session["UserName"] = res.Username.ToString();
                    return RedirectToAction("Index");
                }
            }

            //error
            ModelState.AddModelError("", "Invalid login information.");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Log In Error");

            //final
            return View(user);
        }

        //
        // Register
        //

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserModel user)
        {
            //pentru Vasi
            //informatii despre Session, la sfarsit: https://www.c-sharpcorner.com/article/simple-login-application-using-Asp-Net-mvc/

            //check model state
            if (ModelState.IsValid)
            {
                //check if user exists
                var res = userDbCtx.Users.SingleOrDefault(u => u.Username == user.Username && u.Email == user.Email);
                if (res != null)
                {
                    //return
                    ModelState.AddModelError("", "Username or email is already existant.");
                    Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Duplicated username/email");

                    return View(user);
                }

                //hash encrypt password
                using (SHA256 hash = SHA256.Create())
                {
                    byte[] bytes;

                    bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(user.Parola));

                    StringBuilder s = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        s.Append(bytes[i].ToString("x2"));
                    }

                    user.Parola = s.ToString();
                }

                //save in database
                userDbCtx.Users.Add(user);
                userDbCtx.SaveChanges();

                //cookies stuff for memorizing the user
                Session["UserID"] = user.Id.ToString();
                Session["UserName"] = user.Username.ToString();

                //final
                return RedirectToAction("Index");
            }

            //error
            ModelState.AddModelError("", "Invalid registration information");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Register Error");

            return View(user);
        }

        //
        // LogOut
        //

        public ActionResult LogOut()
        {
            //delogare: https://www.youtube.com/watch?v=0y_HeholX4I

            int userId = (int) Session["user_id"];
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}