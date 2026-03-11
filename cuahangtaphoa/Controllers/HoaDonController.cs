using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Controllers
{
    public class HoaDonController : Controller
    {
        testEntities db = new testEntities();

        public ActionResult Index() => View();

        // =============================================
        // API: Danh sách hóa đơn (có lọc)
        // =============================================
        [HttpGet]
        public JsonResult GetDanhSach(string tuNgay, string denNgay,
                                      string trangThai, string keyword,
                                      int page = 1, int pageSize = 20)
        {
            try
            {
                var query = db.HoaDons.AsQueryable();

                if (!string.IsNullOrEmpty(tuNgay))
                {
                    DateTime from = DateTime.Parse(tuNgay);
                    query = query.Where(h => h.NgayLap >= from);
                }
                if (!string.IsNullOrEmpty(denNgay))
                {
                    DateTime to = DateTime.Parse(denNgay).AddDays(1);
                    query = query.Where(h => h.NgayLap < to);
                }
                if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
                    query = query.Where(h => h.TrangThai == trangThai);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(h => h.MaDon.Contains(keyword));

                int total = query.Count();

                // ToList() trước để xử lý nullable trong C# thay vì LINQ-to-SQL
                var list = query
                    .OrderByDescending(h => h.NgayLap)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var data = list.Select(h => new
                {
                    maHoaDon = h.MaHoaDon,
                    maDon = h.MaDon ?? "",
                    ngayLap = h.NgayLap.HasValue ? h.NgayLap.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    nhanVien = h.NguoiDung != null ? h.NguoiDung.HoTen : "—",
                    tongTien = h.TongTien is decimal tt ? tt : 0m,
                    phanTramGiam = h.PhanTramGiam is decimal ptg ? ptg : 0m,
                    thucThu = h.TienSauGiam is decimal tsg ? tsg : 0m,
                    trangThai = h.TrangThai ?? "—",
                    soMatHang = h.ChiTietHoaDons.Count
                }).ToList();

                return Json(new
                {
                    success = true,
                    data,
                    total,
                    totalPages = (int)Math.Ceiling((double)total / pageSize),
                    page
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // API: Chi tiết 1 hóa đơn
        // =============================================
        [HttpGet]
        public JsonResult GetChiTiet(int id)
        {
            try
            {
                var hd = db.HoaDons.Find(id);
                if (hd == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" }, JsonRequestBehavior.AllowGet);

                var chiTiet = hd.ChiTietHoaDons.ToList().Select(ct => new
                {
                    tenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "—",
                    maVach = ct.SanPham != null ? (ct.SanPham.MaVach ?? "") : "",
                    soLuong = ct.SoLuong is int sl ? sl : 0,
                    giaBan = ct.GiaBan is decimal gb ? gb : 0m,
                    thanhTien = ct.ThanhTien is decimal tht ? tht : 0m
                }).ToList();

                var tt = hd.ThanhToans.FirstOrDefault();

                return Json(new
                {
                    success = true,
                    maHoaDon = hd.MaHoaDon,
                    maDon = hd.MaDon ?? "",
                    ngayLap = hd.NgayLap.HasValue ? hd.NgayLap.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                    nhanVien = hd.NguoiDung != null ? hd.NguoiDung.HoTen : "—",
                    tongTien = hd.TongTien is decimal tong ? tong : 0m,
                    phanTramGiam = hd.PhanTramGiam is decimal ptg ? ptg : 0m,
                    thucThu = hd.TienSauGiam is decimal tsg ? tsg : 0m,
                    trangThai = hd.TrangThai ?? "—",
                    phuongThuc = tt != null ? (tt.PhuongThucThanhToan ?? "—") : "—",
                    tienKhachDua = tt != null ? (tt.SoTienThanhToan is decimal std ? std : 0m) : 0m,
                    chiTiet
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // API: Hủy hóa đơn
        // =============================================
        [HttpPost]
        public JsonResult HuyHoaDon(int id, string lyDo)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var hd = db.HoaDons.Find(id);
                    if (hd == null)
                        return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                    if (hd.TrangThai == "Đã hủy")
                        return Json(new { success = false, message = "Hóa đơn đã được hủy trước đó" });

                    // Hoàn lại tồn kho
                    foreach (var ct in hd.ChiTietHoaDons.ToList())
                    {
                        var sp = db.SanPhams.Find(ct.MaSanPham);
                        if (sp != null)
                            sp.SoLuong += (ct.SoLuong is int sl ? sl : 0);
                    }

                    hd.TrangThai = "Đã hủy";
                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        // =============================================
        // API: Thống kê nhanh (header cards)
        // =============================================
        [HttpGet]
        public JsonResult GetStats(string tuNgay, string denNgay)
        {
            try
            {
                DateTime from = string.IsNullOrEmpty(tuNgay)
                    ? DateTime.Today : DateTime.Parse(tuNgay);
                DateTime to = string.IsNullOrEmpty(denNgay)
                    ? DateTime.Today.AddDays(1) : DateTime.Parse(denNgay).AddDays(1);

                var hds = db.HoaDons
                    .Where(h => h.NgayLap >= from && h.NgayLap < to)
                    .ToList();

                int tong = hds.Count;
                int hoanThanh = hds.Count(h => h.TrangThai == "Hoàn thành");
                int daHuy = hds.Count(h => h.TrangThai == "Đã hủy");
                decimal doanhThu = hds
                    .Where(h => h.TrangThai == "Hoàn thành")
                    .Sum(h => h.TienSauGiam is decimal d ? d : 0m);

                return Json(new
                {
                    success = true,
                    tongHoaDon = tong,
                    hoanThanh,
                    daHuy,
                    doanhThuText = FormatMoney(doanhThu)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string FormatMoney(decimal amount)
        {
            if (amount >= 1_000_000_000) return string.Format("{0:0.##}", amount / 1_000_000_000) + " tỷ";
            if (amount >= 1_000_000) return string.Format("{0:0.##}", amount / 1_000_000) + " tr";
            return string.Format("{0:N0}", amount);
        }
    }
}