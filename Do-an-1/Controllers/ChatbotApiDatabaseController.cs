using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Do_an_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ChatbotApiDatabaseController : ControllerBase
    {
        private readonly FashionStoreDbContext _context;
        private readonly ILogger<ChatbotApiDatabaseController> _logger;

        public ChatbotApiDatabaseController(FashionStoreDbContext context, ILogger<ChatbotApiDatabaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Chatbot DB API is working!", timestamp = DateTime.Now });
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] string? search = null, [FromQuery] int? categoryId = null)
        {
            try
            {
                var query = _context.TbProducts
                    .Include(p => p.CategoryProduct)
                    .Where(p => p.IsActive == true);

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(p => (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                                             (p.Description != null && p.Description.ToLower().Contains(searchLower)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryProductId == categoryId);
                }

                var products = await query
                    .OrderByDescending(p => p.IsBestSeller == true)
                    .ThenByDescending(p => p.CreatedDate)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Title,
                        p.Description,
                        p.Price,
                        p.PriceSale,
                        p.Image,
                        p.IsNew,
                        p.IsBestSeller,
                        p.Star,
                        CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Title : null
                    })
                    .Take(20)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy dữ liệu sản phẩm" });
            }
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.TbProducts
                    .Include(p => p.CategoryProduct)
                    .Include(p => p.TbProductReviews)
                    .Where(p => p.ProductId == id && p.IsActive == true)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Title,
                        p.Description,
                        p.Detail,
                        p.Price,
                        p.PriceSale,
                        p.Image,
                        p.IsNew,
                        p.IsBestSeller,
                        p.Star,
                        p.Quantity,
                        CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Title : null,
                        Reviews = p.TbProductReviews
                            .OrderByDescending(r => r.CreatedDate)
                            .Select(r => new { r.Name, r.Detail, r.CreatedDate })
                            .Take(5)
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new { error = "Không tìm thấy sản phẩm" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sản phẩm {ProductId} (DB API)", id);
                return StatusCode(500, new { error = "Lỗi server khi lấy thông tin sản phẩm" });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.TbProductCategories
                    .Select(c => new { c.CategoryProductId, c.Title, c.Description, c.Icon })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy dữ liệu danh mục" });
            }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string? code = null, [FromQuery] string? phone = null)
        {
            try
            {
                var query = _context.TbOrders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.TbOrderDetails).ThenInclude(od => od.Product)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(code))
                {
                    query = query.Where(o => o.Code != null && o.Code.Equals(code));
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    query = query.Where(o => o.Customer != null && o.Customer.Phone != null && o.Customer.Phone.Contains(phone));
                }

                // Nếu không truyền tham số, trả về 5 đơn gần nhất (friendly cho chatbot demo)
                query = query.OrderByDescending(o => o.CreatedDate);

                var orders = await query
                    .Take(10)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.Code,
                        CustomerName = o.Customer != null ? o.Customer.Name : null,
                        Phone = o.Customer != null ? o.Customer.Phone : null,
                        Address = o.ShippingAddress,
                        o.TotalAmount,
                        o.CreatedDate,
                        StatusName = o.OrderStatus != null ? o.OrderStatus.Name : null,
                        OrderDetails = o.TbOrderDetails.Select(od => new
                        {
                            Title = od.Product != null ? od.Product.Title : null,
                            od.Price,
                            od.Quantity
                        })
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy dữ liệu đơn hàng" });
            }
        }

        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.TbBlogs
                    .Include(b => b.BlogCategory)
                    .Include(b => b.Account)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(b => (b.Title != null && b.Title.ToLower().Contains(searchLower)) ||
                                             (b.Description != null && b.Description.ToLower().Contains(searchLower)));
                }

                var blogs = await query
                    .OrderByDescending(b => b.CreatedDate)
                    .Select(b => new
                    {
                        b.BlogId,
                        b.Title,
                        b.Description,
                        b.Image,
                        b.CreatedDate,
                        CategoryName = b.BlogCategory != null ? b.BlogCategory.Title : null,
                        AuthorName = b.Account != null ? b.Account.FullName : null
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách blog (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy dữ liệu blog" });
            }
        }

        [HttpGet("news")]
        public async Task<IActionResult> GetNews([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.TbNews
                    .Include(n => n.NewsCategory)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(searchLower)) ||
                                             (n.Description != null && n.Description.ToLower().Contains(searchLower)));
                }

                var news = await query
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new
                    {
                        n.NewsId,
                        n.Title,
                        n.Description,
                        n.Image,
                        n.CreatedDate,
                        CategoryName = n.NewsCategory != null ? n.NewsCategory.Title : null
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tin tức (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy dữ liệu tin tức" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalProducts = await _context.TbProducts.CountAsync(p => p.IsActive == true);
                var totalOrders = await _context.TbOrders.CountAsync();
                var totalCustomers = await _context.TbCustomers.CountAsync();
                var totalBlogs = await _context.TbBlogs.CountAsync();
                var totalNews = await _context.TbNews.CountAsync();

                var newProducts = await _context.TbProducts.CountAsync(p => p.IsNew == true && p.IsActive == true);
                var bestSellerProducts = await _context.TbProducts.CountAsync(p => p.IsBestSeller == true && p.IsActive == true);

                return Ok(new
                {
                    totalProducts,
                    totalOrders,
                    totalCustomers,
                    totalBlogs,
                    totalNews,
                    newProducts,
                    bestSellerProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi lấy thống kê" });
            }
        }

        public class SearchRequest
        {
            public string Query { get; set; } = string.Empty;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchData([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new { error = "Truy vấn không được để trống" });
                }

                var q = request.Query.ToLower();

                var productsTask = _context.TbProducts
                    .Include(p => p.CategoryProduct)
                    .Where(p => p.IsActive == true &&
                                ((p.Title != null && p.Title.ToLower().Contains(q)) ||
                                 (p.Description != null && p.Description.ToLower().Contains(q))))
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Title,
                        p.Description,
                        p.Price,
                        p.PriceSale,
                        p.Image,
                        CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Title : null
                    })
                    .Take(5)
                    .ToListAsync();

                var blogsTask = _context.TbBlogs
                    .Include(b => b.BlogCategory)
                    .Where(b => (b.Title != null && b.Title.ToLower().Contains(q)) ||
                                (b.Description != null && b.Description.ToLower().Contains(q)))
                    .Select(b => new
                    {
                        b.BlogId,
                        b.Title,
                        b.Description,
                        b.Image,
                        CategoryName = b.BlogCategory != null ? b.BlogCategory.Title : null
                    })
                    .Take(3)
                    .ToListAsync();

                var newsTask = _context.TbNews
                    .Include(n => n.NewsCategory)
                    .Where(n => (n.Title != null && n.Title.ToLower().Contains(q)) ||
                                (n.Description != null && n.Description.ToLower().Contains(q)))
                    .Select(n => new
                    {
                        n.NewsId,
                        n.Title,
                        n.Description,
                        n.Image,
                        CategoryName = n.NewsCategory != null ? n.NewsCategory.Title : null
                    })
                    .Take(3)
                    .ToListAsync();

                await Task.WhenAll(productsTask, blogsTask, newsTask);

                return Ok(new
                {
                    products = productsTask.Result,
                    blogs = blogsTask.Result,
                    news = newsTask.Result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm dữ liệu (DB API)");
                return StatusCode(500, new { error = "Lỗi server khi tìm kiếm" });
            }
        }
    }
}


