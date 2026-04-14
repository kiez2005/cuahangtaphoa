using cuahangtaphoa.Filters;
using cuahangtaphoa.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [CustomAuthorize(Roles = "1")]

    public class NhanVienController : TrangChuController
    {
        testEntities db = new testEntities();

        public ActionResult Index()
        {
            var ds = db.NguoiDungs.ToList();
            return View(ds);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(NguoiDung nv)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nv.HoTen))
                {
                    TempData["Error"] = "Họ tên không được để trống";
                    return RedirectToAction("Index");
                }

                if (db.NguoiDungs.Any(x => x.TenDangNhap == nv.TenDangNhap))
                {
                    TempData["Error"] = "Tên đăng nhập đã tồn tại";
                    return RedirectToAction("Index");
                }

                nv.NgayTao = DateTime.Now;

                // mã hoá mật khẩu
                nv.MatKhau = LoginController.MaHoa(nv.MatKhau);

                db.NguoiDungs.Add(nv);
                db.SaveChanges();

                TempData["Success"] = "Thêm thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ================== EDIT ==================
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit(NguoiDung nv)
        {
            var data = db.NguoiDungs.Find(nv.MaNguoiDung);

            if (data == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên";
                return RedirectToAction("Index");
            }

            try
            {
                data.HoTen = nv.HoTen;
                data.SoDienThoai = nv.SoDienThoai;
                data.DiaChi = nv.DiaChi;
                data.MaVaiTro = nv.MaVaiTro;
                data.TrangThai = nv.TrangThai;
                data.TenDangNhap = nv.TenDangNhap;

                if (!string.IsNullOrWhiteSpace(nv.MatKhau))
                {
                    data.MatKhau = LoginController.MaHoa(nv.MatKhau);
                }

                db.SaveChanges();

                TempData["Success"] = "Cập nhật thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult DeleteMultiple(int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                {
                    return Json(new { success = false, message = "Không có dữ liệu" });
                }

                var list = db.NguoiDungs
                             .Where(x => ids.Contains(x.MaNguoiDung))
                             .ToList();

                db.NguoiDungs.RemoveRange(list);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}