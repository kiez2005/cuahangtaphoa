using cuahangtaphoa.Controllers;
using cuahangtaphoa.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UnitTestProject
{
    [TestClass]
    public class Test_NhanVien
    {
        NhanVienController controller;

        [TestInitialize]
        public void Setup()
        {
            controller = new NhanVienController();

            // TempData
            controller.TempData = new TempDataDictionary();

            // Fake HttpContext
            var httpContext = new HttpContext(
                new HttpRequest("", "http://localhost/", ""),
                new HttpResponse(new StringWriter())
            );

            var contextWrapper = new HttpContextWrapper(httpContext);

            var routeData = new RouteData();
            routeData.Values.Add("controller", "NhanVien");
            routeData.Values.Add("action", "Index");

            controller.ControllerContext = new ControllerContext(
                contextWrapper,
                routeData,
                controller
            );
        }

        // ================= INDEX =================

        [TestMethod]
        public void Index_ReturnView_NotNull()
        {
            var result = controller.Index() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Model);
        }

        // ================= CREATE =================

        [TestMethod]
        public void Create_ValidNhanVien()
        {
            var nv = new NguoiDung
            {
                HoTen = "NV" + Guid.NewGuid(),
                TenDangNhap = "nv6" + Guid.NewGuid(),
                MatKhau = "123",
                MaVaiTro = 1
            };

            var result = controller.Create(nv) as RedirectToRouteResult;

            Assert.IsNull(controller.TempData["Error"], "Lỗi: " + controller.TempData["Error"]);

            Assert.IsNotNull(result);

            using (var db = new testEntities())
            {
                Assert.IsTrue(db.NguoiDungs.Any(x => x.TenDangNhap == nv.TenDangNhap));
            }
        }

        [TestMethod]
        public void Create_EmptyHoTen()
        {
            var nv = new NguoiDung
            {
                HoTen = "",
                TenDangNhap = "test123",
                MatKhau = "123"
            };

            controller.Create(nv);

            Assert.IsTrue(controller.TempData.ContainsKey("Error"));
        }

        [TestMethod]
        public void Create_TrungTenDangNhap()
        {
            var username = "duplicate_test_" + Guid.NewGuid();

            var nv1 = new NguoiDung
            {
                HoTen = "NV1",
                TenDangNhap = username,
                MatKhau = "123"
            };
            controller.Create(nv1);

            var nv2 = new NguoiDung
            {
                HoTen = "NV2",
                TenDangNhap = username,
                MatKhau = "123"
            };

            controller.Create(nv2);

            Assert.IsNotNull(controller.TempData["Error"]);
        }

        // ================= EDIT =================

        [TestMethod]
        public void Edit_NotFound()
        {
            var nv = new NguoiDung
            {
                MaNguoiDung = 999999
            };

            controller.Edit(nv);

            Assert.IsNotNull(controller.TempData["Error"]);
        }

        [TestMethod]
        public void Edit_UpdateSuccess()
        {
            int id;

            using (var db = new testEntities())
            {
                var user = new NguoiDung
                {
                    HoTen = "Noname" + Guid.NewGuid(),
                    TenDangNhap = "nv6" + Guid.NewGuid(),
                    MatKhau = "123456",
                    NgayTao = DateTime.Now,
                    MaVaiTro = 1
                };

                db.NguoiDungs.Add(user);
                db.SaveChanges();

                id = user.MaNguoiDung;
            }

            var nv = new NguoiDung
            {
                MaNguoiDung = id,
                HoTen = "Noname" + Guid.NewGuid(),
                TenDangNhap = "nv6" + Guid.NewGuid(),
                MatKhau = "123456"
            };

            var result = controller.Edit(nv) as RedirectToRouteResult;

            Assert.IsNotNull(result);
        }

        // ================= DELETE =================

        [TestMethod]
        public void Delete_NullIds()
        {
            var result = controller.DeleteMultiple(null) as JsonResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Delete_ValidIds()
        {
            int id;

            using (var db = new testEntities())
            {
                var user = new NguoiDung
                {
                    HoTen = "Noname" + Guid.NewGuid(),
                    TenDangNhap = "nv6" + Guid.NewGuid(),
                    MatKhau = "123456",
                    NgayTao = DateTime.Now,
                    MaVaiTro = 1
                };

                db.NguoiDungs.Add(user);
                db.SaveChanges();

                id = user.MaNguoiDung;
            }

            var result = controller.DeleteMultiple(new int[] { id }) as JsonResult;

            Assert.IsNotNull(result);

            using (var db = new testEntities())
            {
                Assert.IsFalse(db.NguoiDungs.Any(x => x.MaNguoiDung == id));
            }
        }
    }
}