using System.Linq;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Controllers
{
    public class NhanVienController : Controller
    {
        testEntities db = new testEntities();

        // Hiển thị danh sách nhân viên
        public ActionResult Index()
        {
            var ds = db.NguoiDungs.ToList();
            return View(ds);
        }

        // Trang thêm nhân viên
        public ActionResult Create()
        {
            return View();
        }

        // Lưu nhân viên
        [HttpPost]
        public ActionResult Create(NguoiDung nv)
        {
            db.NguoiDungs.Add(nv);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}