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

            return View(sanpham.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }

        // xử lý thêm dữ liệu
        [HttpPost]
        public ActionResult Create(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                sp.NgayTao = DateTime.Now;
                db.SanPhams.Add(sp);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sp);
        }
        public ActionResult Edit(int id)
        {
            var sp = db.SanPhams.Find(id);
            return View(sp);
        }

        // cập nhật dữ liệu
        [HttpPost]
        public ActionResult Edit(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sp);
        }
        public ActionResult Delete(int id)
        {
            var sp = db.SanPhams.Find(id);

            db.SanPhams.Remove(sp);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult SetPrice(int id)
        {
            var sp = db.SanPhams.Find(id);
            return View(sp);
        }
        [HttpPost]
        public ActionResult SetPrice(int id, decimal GiaNhap, decimal GiaBan)
        {
            var sp = db.SanPhams.Find(id);

            sp.GiaNhap = GiaNhap;
            sp.GiaBan = GiaBan;

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult PrintBarcode(int id)
        {
            var sp = db.SanPhams.Find(id);
            return View(sp);
        }
    }
}
