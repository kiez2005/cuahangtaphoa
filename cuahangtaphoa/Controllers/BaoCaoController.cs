using cuahangtaphoa.Filters;
using cuahangtaphoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [CustomAuthorize(Roles = "1")]

    public class BaoCaoController : TrangChuController
    {
        testEntities db = new testEntities();

        // GET: /BaoCao/
        public ActionResult Index()
        {
            return View();
        }

        // =============================================
        // API: Tổng hợp số liệu theo khoảng ngày
        // =============================================
        [HttpGet]
        public JsonResult GetTongHop(string tuNgay, string denNgay)
        {
            try
            {
                DateTime from = DateTime.Parse(tuNgay);
                DateTime to = DateTime.Parse(denNgay).AddDays(1); // bao gồm ngày cuối

                // Doanh thu + số hóa đơn
                var hoaDons = db.HoaDons
                    .Where(h => h.NgayLap >= from && h.NgayLap < to
                             && h.TrangThai == "Hoàn thành")
                    .ToList();

                decimal doanhThu = hoaDons.Sum(h => h.TienSauGiam ?? 0);
                int soHoaDon = hoaDons.Count;
                decimal tbHoaDon = soHoaDon > 0 ? doanhThu / soHoaDon : 0;

                // Tổng nhập hàng — dùng ?? 0 để tránh lỗi decimal? -> decimal
                var phieuNhaps = db.PhieuNhaps
                    .Where(p => p.NgayNhap >= from && p.NgayNhap < to)
                    .ToList();

                decimal tongNhap = phieuNhaps.Sum(p => p.TongTien ?? 0);
                int soPhieuNhap = phieuNhaps.Count;

                // Lợi nhuận ước tính
                decimal loiNhuan = doanhThu - tongNhap;

                // Data cho biểu đồ: doanh thu theo từng ngày trong khoảng
                var labels = new List<string>();
                var chartData = new List<decimal>();

                for (DateTime d = from; d < to; d = d.AddDays(1))
                {
                    DateTime next = d.AddDays(1);
                    decimal dt = hoaDons
                        .Where(h => h.NgayLap >= d && h.NgayLap < next)
                        .Sum(h => h.TienSauGiam ?? 0);

                    labels.Add(d.ToString("dd/MM"));
                    chartData.Add(dt);
                }

                return Json(new
                {
                    success = true,
                    doanhThu = doanhThu,
                    doanhThuText = FormatMoney(doanhThu),
                    soHoaDon = soHoaDon,
                    tbHoaDon = tbHoaDon,
                    tbHoaDonText = FormatMoney(tbHoaDon),
                    tongNhap = tongNhap,
                    tongNhapText = FormatMoney(tongNhap),
                    soPhieuNhap = soPhieuNhap,
                    loiNhuan = loiNhuan,
                    loiNhuanText = FormatMoney(Math.Abs(loiNhuan)),
                    chartLabels = labels,
                    chartData = chartData
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // API: Chi tiết hóa đơn trong kỳ
        // =============================================
        [HttpGet]
        public JsonResult GetChiTietHoaDon(string tuNgay, string denNgay)
        {
            try
            {
                DateTime from = DateTime.Parse(tuNgay);
                DateTime to = DateTime.Parse(denNgay).AddDays(1);

                var data = db.HoaDons
                    .Where(h => h.NgayLap >= from && h.NgayLap < to)
                    .OrderByDescending(h => h.NgayLap)
                    .ToList()
                    .Select(h => new
                    {
                        maDon = h.MaDon,
                        ngayLap = h.NgayLap.HasValue ? h.NgayLap.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        nhanVien = h.NguoiDung != null ? h.NguoiDung.HoTen : "—",
                        tongTien = h.TongTien ?? 0,
                        phanTramGiam = h.PhanTramGiam ?? 0,
                        thucThu = h.TienSauGiam ?? 0,
                        trangThai = h.TrangThai ?? "—"
                    })
                    .ToList();

                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // API: Top sản phẩm bán chạy trong kỳ
        // =============================================
        [HttpGet]
        public JsonResult GetTopSanPham(string tuNgay, string denNgay)
        {
            try
            {
                DateTime from = DateTime.Parse(tuNgay);
                DateTime to = DateTime.Parse(denNgay).AddDays(1);

                var data = db.ChiTietHoaDons
                    .Where(ct => ct.HoaDon.NgayLap >= from
                              && ct.HoaDon.NgayLap < to
                              && ct.HoaDon.TrangThai == "Hoàn thành")
                    .GroupBy(ct => new { ct.MaSanPham, ct.SanPham.TenSanPham })
                    .Select(g => new
                    {
                        maSanPham = g.Key.MaSanPham,
                        tenSanPham = g.Key.TenSanPham,
                        tongSoLuong = g.Sum(x => x.SoLuong),
                        tongDoanhThu = g.Sum(x => (decimal?)x.ThanhTien) ?? 0
                    })
                    .OrderByDescending(x => x.tongDoanhThu)
                    .Take(10)
                    .ToList();

                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // API: Thống kê nhập hàng theo nhà cung cấp
        // =============================================
        [HttpGet]
        public JsonResult GetNhapHang(string tuNgay, string denNgay)
        {
            try
            {
                DateTime from = DateTime.Parse(tuNgay);
                DateTime to = DateTime.Parse(denNgay).AddDays(1);

                var data = db.PhieuNhaps
                    .Where(p => p.NgayNhap >= from && p.NgayNhap < to)
                    .GroupBy(p => new { p.MaNhaCungCap, p.NhaCungCap.TenNhaCungCap })
                    .Select(g => new
                    {
                        nhaCungCap = g.Key.TenNhaCungCap,
                        soPhieu = g.Count(),
                        tongTien = g.Sum(x => (decimal?)x.TongTien) ?? 0
                    })
                    .OrderByDescending(x => x.tongTien)
                    .ToList();

                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =============================================
        // HELPER
        // =============================================
        private string FormatMoney(decimal amount)
        {
            if (amount >= 1_000_000_000) return string.Format("{0:0.##}", amount / 1_000_000_000) + " tỷ";
            if (amount >= 1_000_000) return string.Format("{0:0.##}", amount / 1_000_000) + " tr";
            return string.Format("{0:N0}", amount);
        }
    }
}