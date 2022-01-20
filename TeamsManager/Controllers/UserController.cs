using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using TeamsManager.Models;

namespace TeamsManager.Controllers
{
    public class UserController : Controller
    {
        private AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        private UserDbContext userDbCtx = new UserDbContext();
        private EncryptionDbContext encryptionDbContext = new EncryptionDbContext();

        private PlanDbContext planDbCtx = new PlanDbContext();

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

            //check if user already exists, if not show error
            var res = userDbCtx.Users.SingleOrDefault(u => u.Username == user.Username);
            if (res == null)
            {
                //error
                ModelState.AddModelError("", "Invalid login information.");
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Username Not Found");

                //final
                return View(user);
            }

            //get keys from key database, if not show error
            //both should have the same ID when inserted in databases
            var keyRes = encryptionDbContext.Encryptions.SingleOrDefault(e => e.Id == res.Id);
            if (keyRes == null)
            {
                //error
                ModelState.AddModelError("", "Invalid login information.");
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Keys Not Found");

                //final
                return View(user);
            }

            //decrypt password
            byte[] encryptedBytes = Convert.FromBase64String(res.Parola);
            byte[] clearBytes;

            byte[] key = Convert.FromBase64String(keyRes.Key);
            byte[] iv = Convert.FromBase64String(keyRes.IV);

            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cs.Close();
                }

                clearBytes = ms.ToArray();
            }

            //if password matches the decrypted password, load in the encrypted one
            if (user.Parola == Encoding.UTF8.GetString(clearBytes))
            {
                user.Parola = res.Parola;

                //cookies stuff for memorizing the user
                Session["UserID"] = user.Id.ToString();
                Session["UserName"] = user.Username.ToString();

                //final
                return RedirectToAction("Index");
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

        public ActionResult Profile()
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

                    ModelState.AddModelError("", "Username or email is already existant.");
                    return RedirectToAction("LogIn");
                }

                //encrypt password on database with matching id
                byte[] clearBytes = Encoding.UTF8.GetBytes(user.Parola);
                byte[] encryptedBytes;

                aes.KeySize = 128;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }

                user.Parola = Convert.ToBase64String(encryptedBytes);

                //save user in database
                userDbCtx.Users.Add(user);
                userDbCtx.SaveChanges();

                //save keys in database, they should save on the same id on both databases
                //assuming the databases are empty
                EncryptionModel encryptionModel = new EncryptionModel();
                encryptionModel.Key = Convert.ToBase64String(aes.Key);
                encryptionModel.IV = Convert.ToBase64String(aes.IV);
                encryptionDbContext.Encryptions.Add(encryptionModel);
                encryptionDbContext.SaveChanges();

                //save changes

                //cookies stuff for memorizing the user
                Session["UserID"] = user.Id.ToString();
                Session["UserName"] = user.Username.ToString();

                //final
                return RedirectToAction("Index");
            }

            //error
            ModelState.AddModelError("", "Invalid registration information");
            Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Register Error");

            return RedirectToAction("LogIn");
        }

        //
        // LogOut
        //
        public ActionResult LogOut()
        {
            //delogare: https://www.youtube.com/watch?v=0y_HeholX4I

            // int userId = (int) Session["UserID"];
            Session.Abandon();
            return RedirectToAction("LogIn");
        }

        public ActionResult Delete()
        {
            //delogare: https://www.youtube.com/watch?v=0y_HeholX4I

            int userId = int.Parse(Session["UserID"].ToString());

            var res = userDbCtx.Users.SingleOrDefault(e => e.Id == userId);
            var keyRes = encryptionDbContext.Encryptions.SingleOrDefault(e => e.Id == userId);

            if (res == null)
            {
                ModelState.AddModelError("", "The current user does not exist in the database anymore");
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Current User Does Not Exist Anymore");

                return RedirectToAction("LogIn");
            }

            userDbCtx.Users.Remove(res);
            userDbCtx.SaveChanges();

            encryptionDbContext.Encryptions.Remove(keyRes);
            encryptionDbContext.SaveChanges();

            Session.Abandon();
            return RedirectToAction("LogIn");
        }

        public ActionResult Delete()
        {
            //delogare: https://www.youtube.com/watch?v=0y_HeholX4I

            int userId = int.Parse(Session["UserID"].ToString());

            var res = userDbCtx.Users.SingleOrDefault(e => e.Id == userId);
            var keyRes = encryptionDbContext.Encryptions.SingleOrDefault(e => e.Id == userId);

            if (res == null)
            {
                ModelState.AddModelError("", "The current user does not exist in the database anymore");
                Trace.WriteLine(DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm:ss tt") + ": Current User Does Not Exist Anymore");

                return RedirectToAction("LogIn");
            }

            userDbCtx.Users.Remove(res);
            userDbCtx.SaveChanges();

            encryptionDbContext.Encryptions.Remove(keyRes);
            encryptionDbContext.SaveChanges();

            Session.Abandon();
            return RedirectToAction("LogIn");
        }
    }
}