using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Controllers
{
    public class BanHangController : TrangChuController
    {
        testEntities db = new testEntities();

        // GET: /BanHang/
        public ActionResult Index()
        {
            return View();
        }

        // API: Tìm sản phẩm theo tên hoặc mã vạch
        [HttpGet]
        public JsonResult TimSanPham(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            // Dùng .ToList() trước rồi mới Select để tránh lỗi EF không map được nullable
            var list = db.SanPhams
                .Where(s => s.TenSanPham.Contains(keyword) || s.MaVach.Contains(keyword))
                .Take(10)
                .ToList();

            var result = list.Select(s => new {
                MaSanPham = s.MaSanPham,
                TenSanPham = s.TenSanPham,
                GiaBan = s.GiaBan,
                SoLuong = s.SoLuong,
                MaVach = s.MaVach ?? "",
                HinhAnh = s.HinhAnh ?? ""
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // API: Thanh toán
        [HttpPost]
        public JsonResult ThanhToan(HoaDonRequest request)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tạo hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaDon = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        MaNguoiDung = 1, // TODO: lấy từ Session sau
                        NgayLap = DateTime.Now,
                        PhanTramGiam = request.PhanTramGiam,
                        TongTien = request.TongTien,
                        TienSauGiam = request.TienSauGiam,
                        TrangThai = "Hoàn thành"
                    };
                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();

                    // 2. Lưu chi tiết + trừ tồn kho
                    foreach (var item in request.ChiTiet)
                    {
                        db.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            MaHoaDon = hoaDon.MaHoaDon,
                            MaSanPham = item.MaSanPham,
                            SoLuong = item.SoLuong,
                            GiaBan = item.GiaBan
                        });

                        var sp = db.SanPhams.Find(item.MaSanPham);
                        if (sp != null)
                            sp.SoLuong -= item.SoLuong;
                    }

                    // 3. Lưu thanh toán
                    db.ThanhToans.Add(new ThanhToan
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        PhuongThucThanhToan = "Tiền mặt",
                        SoTienThanhToan = request.TienKhachDua,
                        NgayThanhToan = DateTime.Now
                    });

                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, maDon = hoaDon.MaDon });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }
    }

    public class HoaDonRequest
    {
        public decimal PhanTramGiam { get; set; }
        public decimal TongTien { get; set; }
        public decimal TienSauGiam { get; set; }
        public decimal TienKhachDua { get; set; }
        public List<ChiTietRequest> ChiTiet { get; set; }
    }

    public class ChiTietRequest
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaBan { get; set; }
    }
}