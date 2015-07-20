using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookKeeperWeb.Models;
using System.Web.Routing;

namespace BookKeeperWeb.Controllers
{

    public class CheckAccountAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // var t = filterContext.RequestContext.HttpContext.Session["Account"];
            if (filterContext.RequestContext.HttpContext.Session["Account"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {{ "Controller", "Accounts" },
                                      { "Action", "index" } });
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class AccountsController : Controller
    {

        public int getCID()
        {
            int CID = 0;
            try
            {
                if (Url.RequestContext.HttpContext.Session["Account"] != null)
                {
                    int.TryParse(Url.RequestContext.HttpContext.Session["Account"].ToString(), out CID);
                }

            }
            catch (Exception)
            {

            }

            return CID;
        }
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    filterContext.RouteData.Values.Add("test", "TESTING");
        //    base.OnActionExecuting(filterContext);
        //}

        private BookKeeperEntities db = new BookKeeperEntities();

        // GET: Accounts

        public ActionResult Index()
        {
            // var t = RouteData.Values["test"];

            ViewBag.CurrentAcc = getCID();
            return View(db.Accounts.ToList());
        }


        public ActionResult LayoutAccount()
        {
            int CID = getCID();
            Account account;
            if (CID != 0)
            {
                account = db.Accounts.Find(CID);
            }
            else
            {
                account = new Account();
            }

            return PartialView(account);
        }


        public ActionResult SetActiveAccount(string AccID)
        {
            Url.RequestContext.HttpContext.Session.Add("Account", AccID);
            return RedirectToAction("Index");
        }

        // GET: Accounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: Accounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Desc")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(account);
        }

        // GET: Accounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Desc")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
