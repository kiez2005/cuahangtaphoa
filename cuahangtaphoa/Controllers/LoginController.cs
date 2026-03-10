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
    public ActionResult Index(string TenDangNhap, string MatKhau)
    {
        var user = db.NguoiDungs
                     .FirstOrDefault(x => x.TenDangNhap == TenDangNhap
                                       && x.MatKhau == MatKhau
                                       && x.TrangThai == true);

        if (user != null)
        {
            Session["user"] = user;
            return RedirectToAction("Index", "TrangChu");
        }

        ViewBag.error = "Sai tài khoản hoặc mật khẩu";
        return View();
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index");
    }
}