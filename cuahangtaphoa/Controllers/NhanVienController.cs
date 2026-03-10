using cuahangtaphoa.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    public class NhanVienController : Controller
    {
        testEntities db = new testEntities();

        public ActionResult Index()
        {
            var ds = db.NguoiDungs.ToList();
            return View(ds);
        }

        [HttpPost]
        public ActionResult Create(NguoiDung nv)
        {
            nv.NgayTao = DateTime.Now;

            db.NguoiDungs.Add(nv);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}