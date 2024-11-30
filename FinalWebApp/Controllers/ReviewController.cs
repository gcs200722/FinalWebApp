using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.Controllers
{
    public class ReviewController : Controller
    {
        private readonly FinalWebDbContext _context;

        public ReviewController(FinalWebDbContext context)
        {
            _context = context;
        }
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý form khi khách hàng gửi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(CustomerReview review)
        {
            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;
                _context.Add(review);
                _context.SaveChanges();
                return RedirectToAction("ThankYou");
            }

            return View("Create", review);
        }

        // Trang cảm ơn sau khi gửi đánh giá
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
