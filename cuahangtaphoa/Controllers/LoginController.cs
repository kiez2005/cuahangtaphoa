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

    public static string MaHoa(string input)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = md5.ComputeHash(bytes);
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
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

        string mk = MaHoa(MatKhau.Trim());

        if (user == null || user.MatKhau.Trim().ToLower() != mk.ToLower())
        {
            return Json(new { success = false, message = "Vui lòng nhập lại tên đăng nhập hoặc mật khẩu" });
        }

        // lưu session
        Session["user"] = user;

        return Json(new { success = true, message = "Đăng nhập thành công" });
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index");
    }
}