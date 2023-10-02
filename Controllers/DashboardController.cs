﻿using Microsoft.AspNetCore.Mvc;
using SWP391_Group3_FinalProject.DAOs;
using SWP391_Group3_FinalProject.Models;
using System.Drawing.Drawing2D;

namespace SWP391_Group3_FinalProject.Controllers
{
    public class DashboardController : Controller
    {
        //Trang chủ của dashboard
        public IActionResult Index()
        {
            return View();
        }

        //Trang để cho admin thêm sản phẩm để bán
        public IActionResult ImportProduct()
        {
            return View();
        }

        //Trang để coi giỏ hàng
        public IActionResult ProductPage()
        {
            ProductDAO dao = new ProductDAO();
            List<Brand> BrandList = dao.GetAllBrand();
            
            //ViewBag
            ViewBag.BrandList = BrandList;

            return View();
        }

        //Statistic page
        public IActionResult Statistic()
        {
            return View();
        }

        //Coi đơn hàng của khách hàng
        public IActionResult OrderRecieptPage()
        {
            return View();
        }



        //Edit Brand
        [HttpPost]
        public IActionResult GetBrandInfo(int brand_id)
        {
            ProductDAO dao = new ProductDAO();
            Brand brand = new Brand();
            //----------------------------------
            brand = dao.GetBrandByID(brand_id);


            return Json(brand);
        }
    }
}
