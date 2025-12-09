using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountsController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public AccountsController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // Hàm mã hóa MD5
        public static string ToMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sbHash = new StringBuilder();
            foreach (byte b in bHash) sbHash.Append(String.Format("{0:x2}", b));
            return sbHash.ToString();
        }

        // GET: Trang đăng nhập Admin
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập Admin rồi thì vào thẳng trang chủ Admin
            if (HttpContext.Session.GetString("AdminId") != null)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return View();
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                string passHash = ToMD5(password); // Mã hóa mật khẩu nhập vào

                // Tìm trong bảng TbAccount (Admin)
                // Lưu ý: So sánh cả Email hoặc Username đều được
                var account = _context.TbAccounts.FirstOrDefault(x => (x.Email == email || x.Username == email) && x.Password == passHash);

                if (account != null)
                {
                    if (account.IsActive == false)
                    {
                        ViewBag.Error = "Tài khoản quản trị này đã bị khóa!";
                        return View();
                    }

                    // Lưu Session dành riêng cho Admin
                    HttpContext.Session.SetString("AdminId", account.AccountId.ToString());
                    HttpContext.Session.SetString("AdminName", account.FullName ?? account.Username);
                    HttpContext.Session.SetString("RoleId", account.RoleId.ToString() ?? "0");

                    // Cập nhật thời gian đăng nhập
                    account.LastLogin = DateTime.Now;
                    _context.SaveChanges();

                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else
                {
                    ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
                }
            }
            return View();
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            // Xóa Session Admin
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminName");
            HttpContext.Session.Remove("RoleId");

            return RedirectToAction("Login", "Accounts", new { area = "Admin" });
        }
    }
}