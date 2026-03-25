using cuahangtaphoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    public class NhaCungCapController : Controller
    {
        testEntities db = new testEntities();

        // GET: NhaCungCap
        public ActionResult Index()
        {
            var ds = db.NhaCungCaps.ToList();
            return View(ds);
        }

        [HttpPost]
        public ActionResult Create(NhaCungCap ncc)
        {
            try
            {
                ncc.NgayTao = DateTime.Now;

                db.NhaCungCaps.Add(ncc);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult DeleteMultiple(int[] ids)
        {
            try
            {
                var list = db.NhaCungCaps
                             .Where(x => ids.Contains(x.MaNhaCungCap))
                             .ToList();

                db.NhaCungCaps.RemoveRange(list);
                db.SaveChanges();

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(NhaCungCap ncc)
        {
            var data = db.NhaCungCaps.Find(ncc.MaNhaCungCap);

            if (data != null)
            {
                data.TenNhaCungCap = ncc.TenNhaCungCap;
                data.SoDienThoai = ncc.SoDienThoai;
                data.Email = ncc.Email;
                data.DiaChi = ncc.DiaChi;
                data.NgayTao = ncc.NgayTao;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}