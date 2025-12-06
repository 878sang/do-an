using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class ContactController : Controller
    {
        
            private readonly FashionStoreDbContext _context;

            public ContactController(FashionStoreDbContext context)
            {
                _context = context;
            }
            public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Send(string name, string phone, string email, string message)
        {
            if (ModelState.IsValid)
            {
             
                TbContact contact = new TbContact();

               
                contact.Name = name;
                contact.Phone = phone; 
                contact.Email = email;
                contact.Message = message;

               
                contact.IsRead = false;          
                contact.CreatedDate = DateTime.Now; 
                contact.CreatedBy = "Khách hàng";   
                contact.ModifiedDate = DateTime.Now;
                contact.ModifiedBy = "System";

           
                _context.Add(contact);
                _context.SaveChanges();

              
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
