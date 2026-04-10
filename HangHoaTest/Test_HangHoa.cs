using Microsoft.VisualStudio.TestTools.UnitTesting;
using cuahangtaphoa.Controllers;
using cuahangtaphoa.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HangHoaTest
{
    [TestClass]
    public class HangHoaTest
    {
        HangHoaController controller;
        testEntities db;

        [TestInitialize]
        public void Setup()
        {
            controller = new HangHoaController();
            db = new testEntities();
        }

        // ================= INDEX =================
        [TestMethod]
        public void Index_ReturnView()
        {
            var result = controller.Index("") as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Model);
        }

        // ================= CREATE =================
        [TestMethod]
        public void Create_ValidSanPham()
        {
            var sp = new SanPham
            {
                TenSanPham = "TestSP_" + Guid.NewGuid(),
                MaVach = "123456",
                GiaBan = 10000
            };

            var result = controller.Create(sp) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            using (var checkDb = new testEntities())
            {
                Assert.IsTrue(checkDb.SanPhams.Any(x => x.TenSanPham == sp.TenSanPham));
            }
        }

        // ================= CREATE INVALID =================
        [TestMethod]
        public void Create_InvalidModel_ReturnView()
        {
            controller.ModelState.AddModelError("TenSanPham", "Required");

            var sp = new SanPham();

            var result = controller.Create(sp) as ViewResult;

            Assert.IsNotNull(result);
        }

        // ================= EDIT =================
        [TestMethod]
        public void Edit_UpdateSuccess()
        {
            int id;

            using (var dbContext = new testEntities())
            {
                var sp = new SanPham
                {
                    TenSanPham = "Old_" + Guid.NewGuid(),
                    GiaBan = 5000
                };

                dbContext.SanPhams.Add(sp);
                dbContext.SaveChanges();

                id = sp.MaSanPham;
            }

            var update = new SanPham
            {
                MaSanPham = id,
                TenSanPham = "New_" + Guid.NewGuid(),
                GiaBan = 9999
            };

            var result = controller.Edit(update) as RedirectToRouteResult;

            Assert.IsNotNull(result);
        }

        // ================= DELETE =================
        [TestMethod]
        public void Delete_Success()
        {
            int id;

            using (var dbContext = new testEntities())
            {
                var sp = new SanPham
                {
                    TenSanPham = "Delete_" + Guid.NewGuid(),
                    GiaBan = 1000
                };

                dbContext.SanPhams.Add(sp);
                dbContext.SaveChanges();

                id = sp.MaSanPham;
            }

            var result = controller.Delete(id) as RedirectToRouteResult;

            Assert.IsNotNull(result);

            using (var checkDb = new testEntities())
            {
                Assert.IsFalse(checkDb.SanPhams.Any(x => x.MaSanPham == id));
            }
        }

        // ================= SET PRICE =================
        [TestMethod]
        public void SetPrice_UpdateSuccess()
        {
            int id;

            using (var dbContext = new testEntities())
            {
                var sp = new SanPham
                {
                    TenSanPham = "Price_" + Guid.NewGuid(),
                    GiaNhap = 1000,
                    GiaBan = 2000
                };

                dbContext.SanPhams.Add(sp);
                dbContext.SaveChanges();

                id = sp.MaSanPham;
            }

            controller.SetPrice(id, 3000, 6000);

            using (var checkDb = new testEntities())
            {
                var sp = checkDb.SanPhams.Find(id);

                Assert.AreEqual(3000, sp.GiaNhap);
                Assert.AreEqual(6000, sp.GiaBan);
            }
        }
    }
}