using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Controllers
{
    public class ThongTinCaNhanController : TrangChuController

    {
        testEntities db = new testEntities();

        // GET: Thông tin cá nhân
        public ActionResult Index()
        {
            var sessionUser = Session["user"] as NguoiDung;

            if (sessionUser == null)
                return RedirectToAction("Login", "Login");

            var user = db.NguoiDungs.Find(sessionUser.MaNguoiDung);

            return View(user);
        }

        // POST: Cập nhật
        [HttpPost]
        public ActionResult Index(NguoiDung model)
        {
            var sessionUser = Session["user"] as NguoiDung;

            if (sessionUser == null)
                return RedirectToAction("Login", "Login");

            var user = db.NguoiDungs.Find(sessionUser.MaNguoiDung);

            if (user != null)
            {
                user.HoTen = model.HoTen;
                user.SoDienThoai = model.SoDienThoai;
                user.DiaChi = model.DiaChi;

                db.SaveChanges();

                // cập nhật lại session
                Session["user"] = user;

                ViewBag.Message = "Cập nhật thành công!";
            }

            return View(user);
        }
    }
}