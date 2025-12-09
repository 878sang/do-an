using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Do_an_1.Controllers
{
    // Đổi tên class thành CustomerController
    public class CustomerController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public CustomerController(FashionStoreDbContext context)
        {
            _context = context;
        }

        public static string ToMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sbHash = new StringBuilder();
            foreach (byte b in bHash) sbHash.Append(String.Format("{0:x2}", b));
            return sbHash.ToString();
        }

        // GET: /Customer/Index
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("CustomerId") != null) return RedirectToAction("Dashboard");

            ViewBag.ActiveTab = "login";
            return View();
        }

        // POST: /Customer/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                string passHash = ToMD5(password);
                var customer = _context.TbCustomers.FirstOrDefault(x => x.Email == email && x.Password == passHash);

                if (customer != null)
                {
                    if (customer.IsActive == false)
                    {
                        ViewBag.Error = "Tài khoản đang bị khóa!";
                        ViewBag.ActiveTab = "login";
                        return View("Index");
                    }

                    customer.LastLogin = DateTime.Now;
                    _context.SaveChanges();

                    HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
                    HttpContext.Session.SetString("Name", customer.Name ?? "Khách hàng");
                    HttpContext.Session.SetString("Email", customer.Email);

                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Email hoặc Mật khẩu không đúng!";
            ViewBag.ActiveTab = "login";
            return View("Index");
        }

        // POST: /Customer/Register
        [HttpPost]
        public IActionResult Register(string name, string email, string password, string confirmPassword)
        {
            try
            {
                if (password != confirmPassword)
                {
                    ViewBag.RegisterError = "Mật khẩu xác nhận không khớp!";
                    ViewBag.ActiveTab = "register";
                    return View("Index");
                }

                var checkEmail = _context.TbCustomers.FirstOrDefault(x => x.Email == email);
                if (checkEmail != null)
                {
                    ViewBag.RegisterError = "Email này đã được sử dụng!";
                    ViewBag.ActiveTab = "register";
                    return View("Index");
                }

                TbCustomer user = new TbCustomer();
                user.Name = name;
                user.Email = email;
                user.Password = ToMD5(password);
                user.IsActive = true;
                user.LastLogin = DateTime.Now;

                _context.Add(user);
                _context.SaveChanges();

                ViewBag.Success = "Đăng ký thành công! Hãy đăng nhập.";
                ViewBag.ActiveTab = "login";
                return View("Index");
            }
            catch
            {
                ViewBag.RegisterError = "Đăng ký thất bại.";
                ViewBag.ActiveTab = "register";
                return View("Index");
            }
        }

        // GET: /Customer/Dashboard
        public IActionResult Dashboard()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Index"); // Quay lại trang đăng nhập
            }

            var customer = _context.TbCustomers.Find(int.Parse(customerId));
            if (customer == null) return RedirectToAction("Index");

            return View(customer);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}