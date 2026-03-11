using cuahangtaphoa.Models;
using System;
using System.Collections.Generic;
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

        [HttpPost]
        public JsonResult DeleteMultiple(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Json(new { success = false });
            }

            var nhanViens = db.NguoiDungs
                              .Where(x => ids.Contains(x.MaNguoiDung))
                              .ToList();

            db.NguoiDungs.RemoveRange(nhanViens);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Edit(NguoiDung nv)
        {
            var data = db.NguoiDungs.Find(nv.MaNguoiDung);

            if (data != null)
            {
                data.HoTen = nv.HoTen;
                data.SoDienThoai = nv.SoDienThoai;
                data.DiaChi = nv.DiaChi;
                
                data.MaVaiTro = nv.MaVaiTro;
                data.TrangThai = nv.TrangThai;
                data.TenDangNhap = nv.TenDangNhap;

                if (!string.IsNullOrEmpty(nv.MatKhau))
                {
                    data.MatKhau = nv.MatKhau;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}