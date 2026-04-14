using cuahangtaphoa.Filters;
using cuahangtaphoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [CustomAuthorize(Roles = "1,2")]
    public class NhaCungCapController : TrangChuController
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
        public ActionResult Edit(NhaCungCap model)
        {
            var ncc = db.NhaCungCaps.Find(model.MaNhaCungCap);

            if (ncc != null)
            {
                ncc.TenNhaCungCap = model.TenNhaCungCap;
                ncc.SoDienThoai = model.SoDienThoai;
                ncc.Email = model.Email;
                ncc.DiaChi = model.DiaChi;

                // ❌ KHÔNG đụng vào NgayTao
                // ncc.NgayTao = model.NgayTao;

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}