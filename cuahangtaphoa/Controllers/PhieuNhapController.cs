using cuahangtaphoa.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    public class PhieuNhapController : Controller
    {
        testEntities db = new testEntities();  
        public ActionResult Index(string search)
        {
            var phieu = db.PhieuNhaps.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                phieu = phieu.Where(p =>
                    p.MaPhieu.Contains(search) ||
                    p.MaPhieuNhap.ToString().Contains(search));
            }

            return View(phieu.OrderByDescending(p => p.NgayNhap).ToList());
        }
        public ActionResult Create()
        {
            ViewBag.MaNhaCungCap = new SelectList(db.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNguoiDung = new SelectList(db.NguoiDungs, "MaNguoiDung", "HoTen");

            ViewBag.SanPham = new SelectList(db.SanPhams, "MaSanPham", "TenSanPham");

            return View();
        }
        [HttpPost]
        public ActionResult Create(PhieuNhap pn, int[] SanPhamId, int[] SoLuong, decimal[] GiaNhap)
        {
            if (ModelState.IsValid)
            {
                pn.NgayNhap = DateTime.Now;
                pn.MaPhieu = "PN" + DateTime.Now.Ticks.ToString().Substring(10);

                db.PhieuNhaps.Add(pn);
                db.SaveChanges();
                for (int i = 0; i < SanPhamId.Length; i++)
                {
                    var ct = new ChiTietPhieuNhap
                    {
                        MaPhieuNhap = pn.MaPhieuNhap,
                        MaSanPham = SanPhamId[i],
                        SoLuong = SoLuong[i],
                        GiaNhap = GiaNhap[i]
                    };

                    db.ChiTietPhieuNhaps.Add(ct);
                    var sp = db.SanPhams.Find(SanPhamId[i]);
                    if (sp != null)
                    {
                        sp.SoLuong += SoLuong[i];
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.MaNhaCungCap = new SelectList(db.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", pn.MaNhaCungCap);
            ViewBag.MaNguoiDung = new SelectList(db.NguoiDungs, "MaNguoiDung", "HoTen", pn.MaNguoiDung);
            ViewBag.SanPham = new SelectList(db.SanPhams, "MaSanPham", "TenSanPham");

            return View(pn);
        }
        public ActionResult Edit(int id)
        {
            var pn = db.PhieuNhaps.Find(id);

            ViewBag.MaNhaCungCap = new SelectList(db.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", pn.MaNhaCungCap);
            ViewBag.MaNguoiDung = new SelectList(db.NguoiDungs, "MaNguoiDung", "HoTen", pn.MaNguoiDung);

            return View(pn);
        }

        // POST: Edit
        [HttpPost]
        public ActionResult Edit(PhieuNhap pn)
        {
            if (ModelState.IsValid)
            {
                var old = db.PhieuNhaps.Find(pn.MaPhieuNhap);

                if (old != null)
                {
                    old.MaNhaCungCap = pn.MaNhaCungCap;
                    old.MaNguoiDung = pn.MaNguoiDung;

                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(pn);
        }
        public ActionResult Delete(int id)
        {
            var pn = db.PhieuNhaps.Find(id);
            var chitiets = db.ChiTietPhieuNhaps
                             .Where(x => x.MaPhieuNhap == id)
                             .ToList();

            foreach (var ct in chitiets)
            {
                var sp = db.SanPhams.Find(ct.MaSanPham);
                if (sp != null)
                {
                    sp.SoLuong -= ct.SoLuong;
                }
                db.ChiTietPhieuNhaps.Remove(ct);
            }
            db.PhieuNhaps.Remove(pn);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var data = db.PhieuNhaps.ToList();

            string csv = "MaPhieuNhap,MaPhieu,NgayNhap,TongTien\n";

            foreach (var item in data)
            {
                csv += item.MaPhieuNhap + ","
                    + item.MaPhieu + ","
                    + item.NgayNhap + ","
                    + item.TongTien + "\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv),
                        "text/csv",
                        "PhieuNhap.csv");
        }
        public JsonResult GetGiaNhap(int id)
        {
            var sp = db.SanPhams.Find(id);
            return Json(sp.GiaNhap, JsonRequestBehavior.AllowGet);
        }
    }
}