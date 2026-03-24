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
    public ActionResult Index(NguoiDung model)
    {
        // 1. Kiểm tra nhập thiếu
        if (string.IsNullOrWhiteSpace(model.TenDangNhap))
        {
            ViewBag.error = "Vui lòng nhập tên đăng nhập";
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.MatKhau))
        {
            ViewBag.error = "Vui lòng nhập mật khẩu";
            return View(model);
        }

        // 2. Sai tài khoản
        var user = db.NguoiDungs
                     .FirstOrDefault(x => x.TenDangNhap == model.TenDangNhap);

        if (user == null)
        {
            ViewBag.error = "Vui lòng nhập lại tài khoản hoặc mật khẩu";
            return View(model);
        }

        // 3. Kiểm tra mật khẩu
        if (user.MatKhau != model.MatKhau)
        {
            ViewBag.error = "Vui lòng nhập lại tài khoản hoặc mật khẩu";
            return View(model);
        }

        // 4. Đăng nhập thành công
        Session["user"] = user;
        return RedirectToAction("Index", "TrangChu");
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index");
    }
}