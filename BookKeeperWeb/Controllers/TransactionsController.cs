using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookKeeperWeb.Models;

namespace BookKeeperWeb.Controllers
{
    public class TransactionsController : Controller
    {
        private BookKeeperEntities db = new BookKeeperEntities();

        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = db.Transactions.Include(t => t.Category).Include(t => t.Type1);
            return View(transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            ViewBag.Cat = new SelectList(db.Categories, "ID", "Desc");
            ViewBag.Type = new SelectList(db.Types, "ID", "DescTxt");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,DescTxt,Cat,Date,Type,Amount")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Cat = new SelectList(db.Categories, "ID", "Desc", transaction.Cat);
            ViewBag.Type = new SelectList(db.Types, "ID", "DescTxt", transaction.Type);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.Cat = new SelectList(db.Categories, "ID", "Desc", transaction.Cat);
            ViewBag.Type = new SelectList(db.Types, "ID", "DescTxt", transaction.Type);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,DescTxt,Cat,Date,Type,Amount")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Cat = new SelectList(db.Categories, "ID", "Desc", transaction.Cat);
            ViewBag.Type = new SelectList(db.Types, "ID", "DescTxt", transaction.Type);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ReportItemisedMonthly(int month = 5)
        {
            var transactions = db.Transactions.Where(item => item.Date.Month == month);


            return View("Index", transactions.ToList());
        }



        public ActionResult SwopTran(int From, int To)
        {

            while (From > To + 1)
            {
                Transaction FromTran = db.Transactions.Where(item => item.ID == From).SingleOrDefault();
                Transaction ToTran = db.Transactions.Where(item => item.ID == From - 1).SingleOrDefault();
                Transaction TempTran = new Transaction();
                TempTran.Amount = FromTran.Amount;
                TempTran.Cat = FromTran.Cat;
                TempTran.Date = FromTran.Date;
                TempTran.DescTxt = FromTran.DescTxt;
                TempTran.Type = FromTran.Type;


                swoptranatt(ref FromTran, ref ToTran);
                swoptranatt(ref ToTran, ref TempTran);
                db.SaveChanges();
                From--;
            }

            return RedirectToAction("Index");
        }
        private void swoptranatt(ref Transaction ToTran, ref Transaction FromTran)
        {
            ToTran.Amount = FromTran.Amount;
            ToTran.Cat = FromTran.Cat;
            ToTran.Date = FromTran.Date;
            ToTran.DescTxt = FromTran.DescTxt;
            ToTran.Type = FromTran.Type;
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
