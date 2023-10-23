﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_Group3_FinalProject.DAOs;
using SWP391_Group3_FinalProject.Filter;
using SWP391_Group3_FinalProject.Models;
using System;
using System.Collections.Generic;

namespace SWP391_Group3_FinalProject.Controllers
{
    [ServiceFilter(typeof(LoginFilter))]
    [ServiceFilter(typeof(ManagerFilter))]
    public class DashboardController : Controller
    {
        //Trang chủ của dashboard
        private readonly IHttpContextAccessor _contx;
        private readonly IWebHostEnvironment _environment;

        public DashboardController(IHttpContextAccessor contx, IWebHostEnvironment environment)
        {
            _contx = contx;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ProductDAO dao = new ProductDAO();
            List<Product> list = dao.GetAllProduct();
            var filteredProducts = list.Where(product => product.pro_quan == 0 || product.pro_quan == 1).ToList();
            ViewBag.LowQuantityProduct = filteredProducts;

            return View();
        }

        
        
        public IActionResult StaffList()
        {

            return View();
        }

        
        public IActionResult CreateAccount()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateAccount( Manager manager)
        {
            //Check if the account created is admin
            bool isAdmin = Request.Form["isAdmin"] == "on";

            //---------------Code Here----------------
            //trước tiên check là username có tồn tại chưa, nếu có đưa error về trang create account
            //nếu không thì lưu account vào database và đưa qua trang stafflist

            //----------------------------------------
            return RedirectToAction("StaffList", "Dashboard");
        }

        //Trang để coi đơn nhập hàng
        
        public IActionResult ImportReceipts()
        {
            ImportRecieptDAO IRdao = new ImportRecieptDAO();
            List<Import_Reciept> IRList = IRdao.GetAllImportReceipt();


            //ViewBag
            ViewBag.IRList = IRList;
            return View();
        }

        //Logging out for Admin And Staff
        
        public IActionResult Logout()
        {
            //Delete Session
            string ManagerInfo, role;          
                _contx.HttpContext.Session.Remove("Session");
                _contx.HttpContext.Session.Remove("action");
            int cookievalue = 0;
            if(_contx.HttpContext.Request.Cookies["role"] != null)
            {
                cookievalue = int.Parse(_contx.HttpContext.Request.Cookies["role"]);
            }
            if (cookievalue != null)
            {
                Response.Cookies.Delete("username");
                Response.Cookies.Delete("role");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult GetIRInfo(int ID)
        {
            try
            {
                // Assuming you have a data access layer (ProductDAO) to retrieve brand information
                ImportRecieptDAO dao = new ImportRecieptDAO();
                Import_Reciept importReciept =  dao.GetImportReceiptByID(ID);

                if (importReciept != null)
                {
                    Console.WriteLine(importReciept);
                    return Ok(importReciept); // Return a 200 OK response with JSON data
                }
                else
                {
                    return NotFound("IR not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult GetRPInfo(int ID)
        {
            try
            {
                // Assuming you have a data access layer (ProductDAO) to retrieve brand information
                ImportRecieptDAO dao = new ImportRecieptDAO();
                List<Receipt_Product> list = dao.GetRPByID(ID);

                if (list != null)
                {
                    Console.WriteLine(list);
                    return Ok(list); // Return a 200 OK response with JSON data
                }
                else
                {
                    return NotFound("IR not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //Trang để cho admin thêm sản phẩm để bán
        
        public IActionResult ImportProduct()
        {

            ProductDAO dao = new ProductDAO();
            List<Brand> BrandList = dao.GetAllBrand();
            List<Category> CategoryList = dao.GetAllCategory();
            List<Product> list = dao.GetAllProduct();

            // Use LINQ to filter products with quantity equal to 0 or 1
            var filteredProducts = list.Where(product => product.pro_quan == 0 || product.pro_quan == 1).ToList();
            // Use LINQ to filter products with quantity greater than 1
            var filteredProductsGreaterThanOne = list
                .Where(product => product.pro_quan > 1)
                .ToList();
            //View Bag
            ViewBag.CategoryList = CategoryList;
            ViewBag.BrandList = BrandList;
            ViewBag.LowQuantityProduct = filteredProducts;
            ViewBag.NormalProduct = filteredProductsGreaterThanOne;


            return View();
        }


        //Do Post của Import Product
        [HttpPost]
        public IActionResult GetImportProduct(int totalCartPriceNumber)
        {

            var ProductIDs = Request.Form["pro_id"];
            var ProductNames = Request.Form["pro_name"];
            var Quantities = Request.Form["amount"];
            var Prices = Request.Form["price"];

            var ProductImported = new List<Receipt_Product>();

            //Get all the product into recieptProduct
            for (int i = 0; i < ProductIDs.Count; i++)
            {
                var recieptProduct = new Receipt_Product
                {
                    pro_id = ProductIDs[i].ToString().Trim(),
                    pro_name = ProductNames[i].ToString().Trim(),
                    amount = Convert.ToInt32(Quantities[i].ToString().Trim()),
                    price = Convert.ToInt32(Prices[i].ToString().Trim())

                };
                ProductImported.Add(recieptProduct);
            }

            //Create an Import_Reciept
            Import_Reciept IR = new Import_Reciept
            {
                Date_Import = DateTime.Now,
                Person_In_Charge = "Nguyen Huu Duy",
                Payment = totalCartPriceNumber
            };
            ImportRecieptDAO dao = new ImportRecieptDAO();
            dao.CreateOrderReciept(IR, ProductImported);


            return RedirectToAction("ProductPage", "Dashboard");
        }


        //Trang để coi giỏ hàng
        
        public IActionResult ProductPage()
        {
            ProductDAO dao = new ProductDAO();

            //Get List for Page
            List<Brand> BrandList = dao.GetAllBrand();
            List<Category> CategoryList = dao.GetAllCategory();
            List<Product> ProductList = dao.GetAllProduct();


            //ViewBag
            ViewBag.BrandList = BrandList.Where(brand => brand.isAvailable == true).ToList();
            ViewBag.CategoryList = CategoryList.Where(cate => cate.isAvailable == true).ToList();
            ViewBag.ProductList = ProductList.Where(pro => pro.isAvailable == true).ToList();
            ViewBag.ProductListDisable = ProductList.Where(pro => pro.isAvailable == false).ToList();
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


        //Get Brand Info
        [HttpPost]
        public IActionResult GetBrandInfo(int brand_id)
        {

            try
            {
                // Assuming you have a data access layer (ProductDAO) to retrieve brand information
                ProductDAO dao = new ProductDAO();
                Brand brand = dao.GetBrandByID(brand_id);

                if (brand != null)
                {
                    Console.WriteLine(brand);
                    return Ok(brand); // Return a 200 OK response with JSON data
                }
                else
                {
                    return NotFound("Brand not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        //Get Category Info
        [HttpPost]
        public IActionResult GetCategoryInfo(int cate_id)
        {

            try
            {
                // Assuming you have a data access layer (ProductDAO) to retrieve brand information
                ProductDAO dao = new ProductDAO();
                Category category = dao.GetCatByID(cate_id);

                if (category != null)
                {
                    Console.WriteLine(category);
                    return Ok(category); // Return a 200 OK response with JSON data
                }
                else
                {
                    return NotFound("Brand not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //Update Brand
        public IActionResult UpdateBrand(Brand brand, IFormFile BrandLogo)
        {

            ProductDAO dao = new ProductDAO();
            if (BrandLogo != null && BrandLogo.Length > 0)
            {
                try
                {
                    var uniqueFileName = brand.brand_id + "_Logo" + Path.GetExtension(BrandLogo.FileName); ;

                    // Define the path where the file will be saved on the server.
                    var webRootPath = _environment.WebRootPath;
                    var uploadPath = Path.Combine(webRootPath, "source_img", "brand_logo");
                    var filePath = Path.Combine(_environment.WebRootPath, "source_img", "brand_logo", uniqueFileName);


                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }


                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        // Copy the uploaded file's content to the stream.
                        BrandLogo.CopyTo(stream);
                    }

                    // Create a URL to access the saved file.
                    var imageUrl = "\\source_img\\brand_logo\\" + uniqueFileName;

                    // Now, imageUrl can be used as the source in your HTML.
                    brand.brand_img = imageUrl;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during file upload or processing.
                    // You can add logging or return an error response to the client.
                }
                dao.EditBrand(brand);
            }
            else
            {
                dao.EditBrandWithoutImage(brand);
            }


            return RedirectToAction("ProductPage", "Dashboard");
        }

        //Update Category
        public IActionResult UpdateCategory(Category category)
        {
            ProductDAO dao = new ProductDAO();
            dao.EditCategory(category);
            return RedirectToAction("ProductPage", "Dashboard");

        }

        //AddProduct
        [HttpPost]
        public IActionResult AddProduct(Product pro, List<IFormFile> imgFile, List<string> feature, List<string> description)
        {
            ProductDAO dao = new ProductDAO();
            Category cate = dao.GetCatByID(pro.cate_id);
            string folder = cate.cate_name.Trim();
            var webRootPath = _environment.WebRootPath;
            var uploadPath = Path.Combine(webRootPath, "source_img", "product_image", folder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            int index = 1;
            foreach (var image in imgFile)
            {
                if (image != null && image.Length > 0)
                {
                    
                    string fileName = pro.pro_id + "_" + index + Path.GetExtension(image.FileName);
                    string PathIntoDatabase = Path.Combine("\\" + "source_img", "product_image", folder, fileName);
                    string filePath = Path.Combine(_environment.WebRootPath, "source_img", "product_image", folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    pro.pro_img.Add(PathIntoDatabase);
                    index++;
                }
            }

            int count = feature.Count();
            for (int i = 0; i < count; i++)
            {
                pro.pro_attribute[feature[i]] = description[i];
            }
            
            dao.AddProductWithDetails(pro);

            return RedirectToAction("ProductPage", "Dashboard");
        }

        //Add Brand
        public IActionResult AddBrand(Brand brand, IFormFile Brand_Logo)
        {
            ProductDAO dao = new ProductDAO();

            List<Brand> BrandList = dao.GetAllBrand();
            int highestBrandId = BrandList.Max(b => b.brand_id);
            int nextBrandID = highestBrandId + 1;

            if (Brand_Logo != null && Brand_Logo.Length > 0)
            {
                try
                {
                    var uniqueFileName = nextBrandID + "_Logo" + Path.GetExtension(Brand_Logo.FileName); ;

                    // Define the path where the file will be saved on the server.
                    var webRootPath = _environment.WebRootPath;
                    var uploadPath = Path.Combine(webRootPath, "source_img", "brand_logo");
                    var filePath = Path.Combine(_environment.WebRootPath, "source_img", "brand_logo", uniqueFileName);


                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }


                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        // Copy the uploaded file's content to the stream.
                        Brand_Logo.CopyTo(stream);
                    }

                    // Create a URL to access the saved file.
                    var imageUrl = "\\source_img\\brand_logo\\" + uniqueFileName;

                    // Now, imageUrl can be used as the source in your HTML.
                    brand.brand_img = imageUrl;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during file upload or processing.
                    // You can add logging or return an error response to the client.
                }
                dao.AddBrand(brand);
            }



            return RedirectToAction("ProductPage", "Dashboard");
        }

        //Add Category
        public IActionResult AddCategory(Category category)
        {
            ProductDAO dao = new ProductDAO();
            dao.AddCategory(category);
            return RedirectToAction("ProductPage", "Dashboard");
        }

        //Show Product Details
        public IActionResult ProductDetailPage(string ID)
        {
            ProductDAO dao = new ProductDAO();
            Product product = dao.GetProductById(ID);
            List<Brand> BrandList = dao.GetAllBrand();
            List<Category> CategoryList = dao.GetAllCategory();

            //ViewBag
            ViewBag.ProductAttribute = product.pro_attribute;
            ViewBag.ProductImage = product.pro_img;
            ViewBag.CategoryList = CategoryList.Where(c => c.isAvailable == true).ToList();
            ViewBag.BrandList = BrandList.Where(b => b.isAvailable == true).ToList();
            return View(product);
        }



        //Get new product Id
        public IActionResult GetNewProductID(int cate_id)
        {
            ProductDAO dao = new ProductDAO();
            string newID = dao.GetNewProductID(cate_id);
            return Content(newID);
        }

        [HttpPost]
        public IActionResult EditProduct(List<int> Image_ID, Product pro,  List<string> feature, List<string> description, List<IFormFile> imgFile, string selectedImages)
        {
            ProductDAO dao = new ProductDAO();
            if (selectedImages != null)
            {
                List<string> selectedImageList = JsonConvert.DeserializeObject<List<string>>(selectedImages);
                foreach (var path in selectedImageList)
                {
                    var webRootPath = _environment.WebRootPath;
                    string filePath = webRootPath  + path;
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                            dao.DeleteImageByPath(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Content("Error: " + ex.Message);
                    }

                }
            }
           

            
            Category cate = dao.GetCatByID(pro.cate_id);
            string folder = cate.cate_name.Trim();
            List<IFormFile> file = new List<IFormFile>();
            List<String> name = new List<String>();


            int count = dao.countProductImage(pro.pro_id);
            for (int i =0; i<count; i++)
            {
                var Images = Request.Form.Files[pro.pro_id + "_" + Image_ID[i]];
                name.Add(pro.pro_id + "_" + Image_ID[i]);
                file.Add(Images);
                
            }

            
            for (int i = 0; i <= count - 1; i++)
            {
                var Image = file[i];
                if (Image != null && Image.Length > 0)
                {
                    try
                    {
                        var uniqueFileName = name[i] + Path.GetExtension(file[i].FileName);
                        var webRootPath = _environment.WebRootPath;
                        var uploadPath = Path.Combine(_environment.WebRootPath, "source_img", "product_image", folder, uniqueFileName);

                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            // Copy the uploaded file's content to the stream.
                            Image.CopyTo(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that may occur during file upload or processing.
                        // You can add logging or return an error response to the client.
                    }
                }

            }

            int index;

            if (Image_ID.Any()) // Check if the sequence contains any elements
            {
                index = Image_ID.Max() + 1;
            }
            else
            {
                // The sequence is empty, so set index to 1 or any other default value as needed
                index = 1;
            }
            foreach (var image in imgFile)
            {
                if (image != null && image.Length > 0)
                {

                    string fileName = pro.pro_id + "_" + index + Path.GetExtension(image.FileName);
                    string PathIntoDatabase = Path.Combine("\\" + "source_img", "product_image", folder, fileName);
                    string filePath = Path.Combine(_environment.WebRootPath, "source_img", "product_image", folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    pro.pro_img.Add(PathIntoDatabase);
                    index++;
                }
            }

            int counter = feature.Count();
            for (int i = 0; i < counter; i++)
            {
                pro.pro_attribute[feature[i]] = description[i];
            }


            dao.DeleteAttributeByID(pro.pro_id);
            dao.UpdateProductWithDetails(pro);
            return RedirectToAction("ProductPage", "Dashboard");

        }

        public IActionResult DisableProduct(string ID)
        {
            ProductDAO dao = new ProductDAO();
            dao.DisableProduct(ID);
            return RedirectToAction("ProductPage", "Dashboard");
        }

        public IActionResult EnableProduct(string ID)
        {
            ProductDAO dao = new ProductDAO();
            dao.EnableProduct(ID);
            return RedirectToAction("ProductPage", "Dashboard");
        }


        public IActionResult DisableBrand(int ID)
        {
            ProductDAO dao = new ProductDAO();
            dao.DisableBrand(ID);
            return RedirectToAction("ProductPage", "Dashboard");
        }

        public IActionResult DisableCategory(int ID)
        {
            ProductDAO dao = new ProductDAO();
            dao.DisableCategory(ID);
            return RedirectToAction("ProductPage", "Dashboard");
        }
    }

}
