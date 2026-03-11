using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Controllers
{
    public class DashboardController : Controller
    {
        testEntities db = new testEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStats()
        {
            try
            {
                DateTime homNay = DateTime.Today;
                DateTime ngayMai = homNay.AddDays(1);

                decimal doanhThu = db.HoaDons
                    .Where(h => h.NgayLap >= homNay && h.NgayLap < ngayMai
                             && h.TrangThai == "Hoàn thành")
                    .Sum(h => (decimal?)h.TienSauGiam) ?? 0;

                int soHoaDon = db.HoaDons
                    .Where(h => h.NgayLap >= homNay && h.NgayLap < ngayMai)
                    .Count();

                int sapHet = db.SanPhams
                    .Where(s => s.SoLuong <= s.SoLuongToiThieu)
                    .Count();

                return Json(new
                {
                    success = true,
                    doanhThu = doanhThu,
                    doanhThuText = FormatMoney(doanhThu),
                    traHang = 0,
                    soHoaDon = soHoaDon,
                    sapHet = sapHet
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetDoanhThu(string filter = "month")
        {
            try
            {
                DateTime now = DateTime.Now;
                var labels = new List<string>();
                var data = new List<decimal>();
                decimal tongDoanhThu = 0;

                if (filter == "week")
                {
                    for (int i = 6; i >= 0; i--)
                    {
                        DateTime ngay = now.Date.AddDays(-i);
                        DateTime ngayKe = ngay.AddDays(1);

                        decimal dt = db.HoaDons
                            .Where(h => h.NgayLap >= ngay && h.NgayLap < ngayKe
                                     && h.TrangThai == "Hoàn thành")
                            .Sum(h => (decimal?)h.TienSauGiam) ?? 0;

                        labels.Add(ngay.ToString("dd/MM"));
                        data.Add(dt);
                        tongDoanhThu += dt;
                    }
                }
                else if (filter == "year")
                {
                    string[] tenThang = { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };
                    for (int i = 1; i <= 12; i++)
                    {
                        DateTime dauThang = new DateTime(now.Year, i, 1);
                        DateTime cuoiThang = dauThang.AddMonths(1);

                        decimal dt = db.HoaDons
                            .Where(h => h.NgayLap >= dauThang && h.NgayLap < cuoiThang
                                     && h.TrangThai == "Hoàn thành")
                            .Sum(h => (decimal?)h.TienSauGiam) ?? 0;

                        labels.Add(tenThang[i - 1]);
                        data.Add(dt);
                        tongDoanhThu += dt;
                    }
                }
                else
                {
                    int soNgay = DateTime.DaysInMonth(now.Year, now.Month);
                    for (int i = 1; i <= soNgay; i++)
                    {
                        DateTime ngay = new DateTime(now.Year, now.Month, i);
                        DateTime ngayKe = ngay.AddDays(1);

                        decimal dt = db.HoaDons
                            .Where(h => h.NgayLap >= ngay && h.NgayLap < ngayKe
                                     && h.TrangThai == "Hoàn thành")
                            .Sum(h => (decimal?)h.TienSauGiam) ?? 0;

                        labels.Add(i < 10 ? "0" + i : i.ToString());
                        data.Add(dt);
                        tongDoanhThu += dt;
                    }
                }

                return Json(new
                {
                    success = true,
                    labels = labels,
                    data = data,
                    tongDoanhThu = tongDoanhThu,
                    tongDoanhThuText = FormatMoney(tongDoanhThu)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetTopSanPham(string sortBy = "revenue", string filter = "month")
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime tuNgay = filter == "week"
                    ? now.Date.AddDays(-7)
                    : new DateTime(now.Year, now.Month, 1);

                var query = db.ChiTietHoaDons
                    .Where(ct => ct.HoaDon.NgayLap >= tuNgay
                              && ct.HoaDon.TrangThai == "Hoàn thành")
                    .GroupBy(ct => new { ct.MaSanPham, ct.SanPham.TenSanPham })
                    .Select(g => new
                    {
                        MaSanPham = g.Key.MaSanPham,
                        TenSanPham = g.Key.TenSanPham,
                        TongSoLuong = g.Sum(x => x.SoLuong),
                        TongDoanhThu = g.Sum(x => (decimal?)x.ThanhTien) ?? 0
                    });

                var result = (sortBy == "quantity"
                    ? query.OrderByDescending(x => x.TongSoLuong)
                    : query.OrderByDescending(x => x.TongDoanhThu))
                    .Take(10)
                    .Select(x => new
                    {
                        x.MaSanPham,
                        x.TenSanPham,
                        x.TongSoLuong,
                        x.TongDoanhThu,
                        TongDoanhThuText = x.TongDoanhThu.ToString("N0")
                    })
                    .ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetHoatDong()
        {
            try
            {
                var hoaDons = db.HoaDons
                    .OrderByDescending(h => h.NgayLap)
                    .Take(10)
                    .ToList()
                    .Select(h => new
                    {
                        NguoiDung = h.NguoiDung != null ? h.NguoiDung.HoTen : "Nhân viên",
                        HanhDong = "bán đơn hàng",
                        GiaTri = (decimal)(h.TienSauGiam ?? 0),
                        ThoiGian = h.NgayLap
                    });

                var phieuNhaps = db.PhieuNhaps
                    .OrderByDescending(p => p.NgayNhap)
                    .Take(10)
                    .ToList()
                    .Select(p => new
                    {
                        NguoiDung = p.NguoiDung != null ? p.NguoiDung.HoTen : "Nhân viên",
                        HanhDong = "nhập hàng",
                        GiaTri = (decimal)(p.TongTien ?? 0),
                        ThoiGian = p.NgayNhap
                    });

                var tatCa = hoaDons
                    .Concat(phieuNhaps)
                    .OrderByDescending(x => x.ThoiGian)
                    .Take(20)
                    .Select(x => new
                    {
                        nguoiDung = x.NguoiDung,
                        hanhDong = x.HanhDong,
                        giaTri = x.GiaTri.ToString("N0"),
                        thoiGian = FormatTime(x.ThoiGian)
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = tatCa,
                    total = tatCa.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string FormatMoney(decimal amount)
        {
            if (amount >= 1000000000)
                return string.Format("{0:0.##}", amount / 1000000000) + " tỷ";
            if (amount >= 1000000)
                return string.Format("{0:0.##}", amount / 1000000) + " tr";
            return string.Format("{0:N0}", amount);
        }

        private string FormatTime(DateTime? dt)
        {
            if (dt == null) return "";
            TimeSpan diff = DateTime.Now - dt.Value;

            if (diff.TotalMinutes < 1) return "vừa xong";
            if (diff.TotalMinutes < 60) return (int)diff.TotalMinutes + " phút trước";
            if (diff.TotalHours < 24) return (int)diff.TotalHours + " giờ trước";
            if (diff.TotalDays < 2) return "một ngày trước";
            if (diff.TotalDays < 7) return (int)diff.TotalDays + " ngày trước";
            if (diff.TotalDays < 30) return (int)(diff.TotalDays / 7) + " tuần trước";
            return dt.Value.ToString("dd/MM/yyyy");
        }
    }
}