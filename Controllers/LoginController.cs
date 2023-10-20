﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_Group3_FinalProject.DAOs;
using SWP391_Group3_FinalProject.Models;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Principal;
using System.Web;

namespace SWP391_Group3_FinalProject.Controllers
{
    public class LoginController : Controller
    {

        private readonly IHttpContextAccessor _contx;
        public LoginController(IHttpContextAccessor contx)
        {
            _contx = contx;
        }


        [HttpGet("/Login")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/SignUp")]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpGet("/ForgetPassword")]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Verify(string username, string password, string isRem)
        {
            AccountDAO dao = new AccountDAO(); // goi ham dao

            Customer cus = dao.GetCustomer(username, password);

            Manager manager = dao.GetManager(username, password);

            if (cus != null)
            {
                if (isRem != null)
                {
                    HttpContext.Response.Cookies.Append("username", cus.username, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3),
                    });
                    HttpContext.Response.Cookies.Append("role", "0", new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3),
                    });
                }

                List<Addresses> list = dao.GetCustomerAddress(username);
                if (list.Count() > 0)
                {
                    cus.addresses.AddRange(list);
                }

                _contx.HttpContext.Session.SetString("Session", JsonConvert.SerializeObject(cus)); //Store information customer to Session
                _contx.HttpContext.Session.SetString("action", JsonConvert.SerializeObject("0")); //Store action filter about manager is 0
                return RedirectToAction("Index", "Home");
            }
            else if (manager != null)
            {
                if(manager.isAdmin == true)
                {
                    if (isRem != null)
                    {
                            
                        HttpContext.Response.Cookies.Append("username", manager.username, new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(3),
                        });
                        HttpContext.Response.Cookies.Append("role", "1", new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(3),
                        });
                    }
                    _contx.HttpContext.Session.SetString("Session", JsonConvert.SerializeObject(manager));
                    _contx.HttpContext.Session.SetString("action", JsonConvert.SerializeObject("1"));
                    return RedirectToAction("Index", "Dashboard");
                } else
                {
                    if (isRem != null)
                    {
                        HttpContext.Response.Cookies.Append("username", manager.username, new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(3),
                        });
                        HttpContext.Response.Cookies.Append("role", "1", new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(3),
                        });
                    }
                    _contx.HttpContext.Session.SetString("Session", JsonConvert.SerializeObject(manager));
                    _contx.HttpContext.Session.SetString("action", JsonConvert.SerializeObject("1"));
                    return RedirectToAction("Index", "Dashboard");
                }


            }
            _contx.HttpContext.Session.SetString("ErrorLogin", JsonConvert.SerializeObject("Username or password is incorrect"));
            return RedirectToAction("Index", "Login");

        }

        [HttpGet("/Logout")]
        public ActionResult Logout()
        {
            try
            {
                int cookievalue = int.Parse(_contx.HttpContext.Request.Cookies["role"]);
                if (cookievalue != null)
                {
                    Response.Cookies.Delete("username");
                    Response.Cookies.Delete("role");
                }
            }
            catch (Exception ex)
            {

            }
            _contx.HttpContext.Session.Remove("Session");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register(Customer customer)
        {
            AccountDAO dao = new AccountDAO();
            dao.AddCustomer(customer);
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public IActionResult CheckUsername(string username)
        {
            AccountDAO dao = new AccountDAO();
            Customer cus = dao.GetCustomerByUsername(username);

            Manager manager = dao.GetManagerByUsername(username);

            if (cus.username != null || manager != null)
            {
                return Content("true");
            }
            return Content("false");
        }

        [HttpPost]
        public IActionResult CheckEmail(string email)
        {
            AccountDAO dao = new AccountDAO();
            int kq = dao.CheckEmail(email);

            if (kq == 1)
            {
                return Content("true");
            }
            return Content("false");
        }
    }
}
