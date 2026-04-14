using cuahangtaphoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    public class HangHoaController : TrangChuController
    {
        // GET: SanPham
        testEntities db = new testEntities();

        public ActionResult Index(string search)
        {
            var sanpham = db.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                sanpham = sanpham.Where(s =>
                    s.TenSanPham.Contains(search) ||
                    s.MaVach.Contains(search));
            }
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs, "MaDanhMuc", "TenDanhMuc");
            ViewBag.MaNhaCungCap = new SelectList(db.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap");

            return View(sanpham.ToList());
        }
        // xử lý thêm dữ liệu
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(SanPham sp, HttpPostedFileBase fileAnh)
        {
            if (!ModelState.IsValid)
            {
                var sanpham = db.SanPhams.ToList();

                ViewBag.MaDanhMuc = new SelectList(db.DanhMucs, "MaDanhMuc", "TenDanhMuc", sp.MaDanhMuc);
                ViewBag.MaNhaCungCap = new SelectList(db.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", sp.MaNhaCungCap);

                return View("Index", sanpham);
            }

            if (fileAnh != null && fileAnh.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(fileAnh.FileName);
                string path = Server.MapPath("~/Content/images/" + fileName);

                fileAnh.SaveAs(path);

                sp.HinhAnh = "/Content/images/" + fileName;
            }

            sp.NgayTao = DateTime.Now;
            db.SanPhams.Add(sp);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            return View(sp);
        }

        // cập nhật dữ liệu
        [HttpPost]
        public ActionResult Edit(SanPham sp, HttpPostedFileBase fileAnh)
        {
            if (sp == null || sp.MaSanPham <= 0)
                return HttpNotFound();

            var old = db.SanPhams.FirstOrDefault(x => x.MaSanPham == sp.MaSanPham);

            if (old == null)
                return HttpNotFound();

            old.TenSanPham = sp.TenSanPham ?? old.TenSanPham;
            old.MaVach = sp.MaVach ?? old.MaVach;
            old.SoLuong = sp.SoLuong;
            old.HanSuDung = sp.HanSuDung;
            old.MaDanhMuc = sp.MaDanhMuc ?? old.MaDanhMuc;
            old.MaNhaCungCap = sp.MaNhaCungCap ?? old.MaNhaCungCap;

            var giaNhapRaw = Request?.Form["GiaNhap"];
            var giaBanRaw = Request?.Form["GiaBan"];

            if (decimal.TryParse(giaNhapRaw, out decimal gn))
                old.GiaNhap = gn;

            if (decimal.TryParse(giaBanRaw, out decimal gb))
                old.GiaBan = gb;

            if (fileAnh != null && fileAnh.ContentLength > 0)
            {
                var fileName = System.IO.Path.GetFileName(fileAnh.FileName);
                var path = Server.MapPath("~/Content/images/" + fileName);
                fileAnh.SaveAs(path);

                old.HinhAnh = "/Content/images/" + fileName;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult Delete(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp != null)
            {
                db.SanPhams.Remove(sp);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        public ActionResult SetPrice(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            return View(sp);
        }
        [HttpPost]
        public ActionResult SetPrice(int id, decimal GiaNhap, decimal GiaBan)
        {
            var sp = db.SanPhams.FirstOrDefault(x => x.MaSanPham == id);

            if (sp == null)
                return HttpNotFound();

            sp.GiaNhap = GiaNhap;
            sp.GiaBan = GiaBan;

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult PrintBarcode(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            return View(sp);
        }
    }
}