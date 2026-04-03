using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

public class LoginController : Controller
{
    testEntities db = new testEntities();

    public ActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public JsonResult Login(string TenDangNhap, string MatKhau)
    {
        if (string.IsNullOrWhiteSpace(TenDangNhap))
        {
            return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập" });
        }

        if (string.IsNullOrWhiteSpace(MatKhau))
        {
            return Json(new { success = false, message = "Vui lòng nhập mật khẩu" });
        }

        var user = db.NguoiDungs
                     .FirstOrDefault(x => x.TenDangNhap == TenDangNhap);

        if (user == null || user.MatKhau != MatKhau)
        {
            return Json(new { success = false, message = "Sai tài khoản hoặc mật khẩu" });
        }

        return Json(new { success = true, message = "Đăng nhập thành công" });
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index");
    }
}