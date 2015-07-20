using Rotativa.Core;
using Rotativa.Core.Options;
using Rotativa.MVC;
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
using System.Globalization;


namespace BookKeeperWeb.Controllers
{
    [CheckAccount]
    public class TransactionsController : Controller
    {
        private BookKeeperEntities db = new BookKeeperEntities();

        public int getCID()
        {
            int CID = 0;
            int.TryParse(Url.RequestContext.HttpContext.Session["Account"].ToString(), out CID);
            return CID;
        }

        // GET: Transactions
        public ActionResult Index()
        {
            int tmp = getCID();
            ViewBag.StartBalance = db.Accounts.Where(x => x.ID == tmp).Single().StartBalance;
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

        public ActionResult ReportItemisedMonthly2(int month = 5)
        {
            var transactions = db.Transactions.Where(item => item.Date.Month == month);


            return View();
        }


        //PDF
        public ActionResult ReportItemisedMonthlyPDF(string sdate = "2014-03-01", string ndate = "2014-04-01")
        {
            var ds = new DataSet();

            using (var conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetTransposedViewExpence";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("AccountID", getCID().ToString()));
                    cmd.Parameters.Add(new SqlParameter("StartDate", sdate));
                    cmd.Parameters.Add(new SqlParameter("EndDate", ndate));
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }

            var newDate = DateTime.ParseExact(sdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var month = newDate.ToString("MMMM", CultureInfo.InvariantCulture);

            ViewBag.Title = "Kasboek uitgawes   " + month + " " + sdate.Substring(0, 4);

            // return View("GetTransopsedView", ds);

            return new ViewAsPdf("TransopsedSum")
            {
                FileName = "Test.pdf",
                RotativaOptions = { PageOrientation = Orientation.Landscape, PageSize = Size.A3 }
            };
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
        public ActionResult GetTransopsedViewExpence(string sdate = "2014-03-01")
        {

            var ds = new DataSet();

            using (var conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetTransposedViewExpence";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("AccountID", getCID().ToString()));
                    cmd.Parameters.Add(new SqlParameter("StartDate", sdate));
                    cmd.Parameters.Add(new SqlParameter("EndDate", getNextDateString(sdate)));
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }

            var newDate = DateTime.ParseExact(sdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var month = newDate.ToString("MMMM", CultureInfo.InvariantCulture);

            ViewBag.Title = "Kasboek uitgawes   " + month + " " + sdate.Substring(0, 4);

            return View("TransPartial", ds);
        }


        public string getNextDateString(string sdate)
        {
            DateTime Start = DateTime.ParseExact(sdate,
                                        "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None);
            DateTime end = Start.AddMonths(1);

            return end.ToString("yyyy-MM-dd");
        }


        public ActionResult GetTransopsedViewIncome(string sdate = "2014-03-01")
        {

            var ds = new DataSet();

            using (var conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetTransposedViewIncome";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("AccountID", getCID().ToString()));
                    cmd.Parameters.Add(new SqlParameter("StartDate", sdate));
                    cmd.Parameters.Add(new SqlParameter("EndDate", getNextDateString(sdate)));
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }
            var newDate = DateTime.ParseExact(sdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var month = newDate.ToString("MMMM", CultureInfo.InvariantCulture);

            ViewBag.Title = "Kasboek Inkomste    " + month + " " + sdate.Substring(0, 4);

            return View("TransPartial", ds);
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
            set { }
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
            set { }
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
    public static class extentions
    {
        public static string ToRand(this double value)
        {
            String s = String.Format("{0:#,##0.00}", value);

            //NumberFormatInfo nfi = (NumberFormatInfo)
            //CultureInfo.InvariantCulture.NumberFormat.Clone();
            //nfi.NumberGroupSeparator = " ";

            NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalDigits = 2 };
            value.ToString("n", nfi); // 12 345.00


            return "R" + value.ToString("n", nfi); // 12 345.00
        }

        public static string ToMyString(this DateTime Item)
        {
            Item.ToString("yyyy-MM-dd");

            return Item.ToString("yyyy-MM-dd");
        }


    }
}

