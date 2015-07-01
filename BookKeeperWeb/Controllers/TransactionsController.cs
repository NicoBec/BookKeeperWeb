using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookKeeperWeb.Models;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;

namespace BookKeeperWeb.Controllers
{
     [CheckAccount]
    public class TransactionsController : Controller
    {
        private BookKeeperEntities db = new BookKeeperEntities();

          public int getCID(){
             int CID = 0;
             int.TryParse(Url.RequestContext.HttpContext.Session["Account"].ToString(), out CID);

             return CID;
         }

        // GET: Transactions
        public ActionResult Index()
        {
            int tmp = getCID();
            var transactions = db.Transactions.Include(t => t.Category).Include(t => t.Type1).Where(x => x.CID == tmp);
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
            int CID = getCID();
            ViewBag.Cat = new SelectList(db.Categories.Where(x => x.CID == CID), "ID", "Desc");
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
                transaction.CID = getCID();
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



        //public ActionResult SwopTran(int From, int To)
        //{

        //    while (From > To + 1)
        //    {
        //        Transaction FromTran = db.Transactions.Where(item => item.ID == From).SingleOrDefault();
        //        Transaction ToTran = db.Transactions.Where(item => item.ID == From - 1).SingleOrDefault();
        //        Transaction TempTran = new Transaction();
        //        TempTran.Amount = FromTran.Amount;
        //        TempTran.Cat = FromTran.Cat;
        //        TempTran.Date = FromTran.Date;
        //        TempTran.DescTxt = FromTran.DescTxt;
        //        TempTran.Type = FromTran.Type;


        //        swoptranatt(ref FromTran, ref ToTran);
        //        swoptranatt(ref ToTran, ref TempTran);
        //        db.SaveChanges();
        //        From--;
        //    }

        //    return RedirectToAction("Index");
        //}
        private void swoptranatt(ref Transaction ToTran, ref Transaction FromTran)
        {
            ToTran.Amount = FromTran.Amount;
            ToTran.Cat = FromTran.Cat;
            ToTran.Date = FromTran.Date;
            ToTran.DescTxt = FromTran.DescTxt;
            ToTran.Type = FromTran.Type;
        }

        public JsonResult AutoDesc(string like)
        {
            List<string> list = db.Database.SqlQuery<string>(@" SELECT distinct [DescTxt]
                                        FROM [BookKeeper].[dbo].[Transaction]
                                        Where [DescTxt] like '%" + like + "%' ").ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }



        // GET: Transactions
        public ActionResult GetTransopsedViewExpence()
        {

            var ds = new DataSet();

            using (var conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetTransposedViewExpence";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("AccountID", getCID().ToString()));
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }
            return View("GetTransopsedView", ds);
        }


        public ActionResult GetTransopsedViewIncome()
        {

            var ds = new DataSet();

            using (var conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetTransposedViewIncome";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("AccountID", getCID().ToString()));
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }
            return View("GetTransopsedView", ds);
        }


        public ActionResult MonthlyTotals()
        {
            List<GetYearTotals_Result> t = db.GetYearTotals(getCID()).OrderBy(x => x.dttm).ToList();

            MonthTotalReport totals = new MonthTotalReport(t);

            return View(totals);
        }


        public JsonResult GetTranByDesc(string Desc)
        {
            int CID = getCID();
            Transaction trn = db.Transactions.OrderBy(item => item.ID).Where(item => item.DescTxt == Desc && item.CID == CID).ToList()[0];
            dynamic tmp = new { Cat = trn.Cat, Date = trn.Date.ToString("yyyy/MM/dd"), type = trn.Type, Amount = trn.Amount };

            return Json(tmp, JsonRequestBehavior.AllowGet);
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

    public class MonthTotalReportItem
    {
        public string Month { get; set; }
        public string TypeDescInc { get; set; }
        public string TypeDescExp { get; set; }
        public double? Income { get; set; }
        public double? Expence { get; set; }
        public double? Total
        {
            get
            {
                return Math.Round((Income - Expence).GetValueOrDefault(), 2); 
            }
            set{}
        }
        public string LossGain
        {
            get
            {
                if ((Income - Expence) > 0)
                {
                    return "Surplus";
                }
                else
                {
                    return "Loss";
                }
            }
            set{}
        }
        public void addValues(GetYearTotals_Result Item)
        {
            Month = Item.Month;
            if (Item.Type == 3)
            {
                Income = Math.Round(Item.Total.GetValueOrDefault(), 2);
                TypeDescInc = Item.TypeDesc;
            }
            else if (Item.Type == 4)
            {
                Expence = Math.Round(Item.Total.GetValueOrDefault(), 2);
                TypeDescExp = Item.TypeDesc;
            }
        }

    }
    public class MonthTotalReport
    {
       public List<MonthTotalReportItem> Items = new List<MonthTotalReportItem>();

        public MonthTotalReport()
        {

        }

        public MonthTotalReport(List<GetYearTotals_Result> Totals)
        {
            foreach (GetYearTotals_Result Item in Totals)
            {
                if (Items.Count == 0)
                {
                    MonthTotalReportItem newItem = new MonthTotalReportItem();
                    newItem.addValues(Item);
                    Items.Add(newItem);
                }
                else
                {
                    if (Items.Count(x => x.Month == Item.Month) > 0)
                    {
                        Items.Where(x => x.Month == Item.Month).SingleOrDefault().addValues(Item);
                    }
                    else
                    {
                        MonthTotalReportItem newItem = new MonthTotalReportItem();
                        newItem.addValues(Item);
                        Items.Add(newItem);

                    }
                }
            }
        }


    }

}
