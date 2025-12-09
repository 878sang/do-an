using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace th3.Controllers
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly FashionStoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatbotController(FashionStoreDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return StatusCode(500, "API Key not configured.");
                }

                var userMessage = request.Contents.LastOrDefault()?.Parts.FirstOrDefault()?.Text;
                string contextInfo = "";

                // Enhanced RAG: Search database to answer user questions
                if (!string.IsNullOrEmpty(userMessage))
                {
                    var messageLower = userMessage.ToLower();
                    var keywords = messageLower.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    var searchTerms = keywords.Where(k => k.Length > 2).ToList();

                    // Check for "how many products" questions
                    bool isAskingProductCount = (keywords.Contains("bao") && keywords.Contains("nhiêu")) ||
                                                 keywords.Contains("tổng") ||
                                                 keywords.Contains("số");

                    if (isAskingProductCount && (messageLower.Contains("sản phẩm") || messageLower.Contains("sp")))
                    {
                        var totalProducts = await _context.TbProducts.CountAsync(p => p.IsActive == true);
                        Console.WriteLine($"[CHATBOT DEBUG] Total active products in DB: {totalProducts}");
                        contextInfo += $"[DATABASE INFO] Tổng số sản phẩm đang kinh doanh: {totalProducts}\n";

                        // Also show the list of products with links
                        var allProducts = await _context.TbProducts
                            .Where(p => p.IsActive == true)
                            .OrderBy(p => p.Title)
                            .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.Alias, p.Image, p.Description })
                            .ToListAsync();

                        if (allProducts.Any())
                        {
                            contextInfo += $"\n[DATABASE INFO] Danh sách {allProducts.Count} sản phẩm (với hình ảnh và link):\n";
                            foreach (var p in allProducts)
                            {
                                var productUrl = $"/san-pham/{p.Alias}-{p.ProductId}";
                                var imageUrl = !string.IsNullOrEmpty(p.Image) ? p.Image : "/images/no-image.jpg";
                                var description = !string.IsNullOrEmpty(p.Description) ? p.Description.Replace("\"", "'").Replace("\n", " ").Replace("\r", "") : "";
                                contextInfo += $"{{\"title\":\"{p.Title}\",\"price\":\"{p.PriceSale:N0} VNĐ\",\"stock\":{p.Quantity},\"url\":\"{productUrl}\",\"image\":\"{imageUrl}\",\"description\":\"{description}\"}}\n";
                            }
                        }
                    }

                    // Check for category count questions
                    if (isAskingProductCount && (messageLower.Contains("danh mục") || messageLower.Contains("loại")))
                    {
                        var totalCategories = await _context.TbProductCategories.CountAsync();
                        contextInfo += $"[DATABASE INFO] Tổng số danh mục: {totalCategories}\n";
                    }

                    // Check if user wants to see ALL products (list request)
                    bool isAskingForList = (messageLower.Contains("tên") || messageLower.Contains("danh sách") ||
                                           messageLower.Contains("liệt kê") || messageLower.Contains("cho tôi biết") ||
                                           messageLower.Contains("có những") || messageLower.Contains("có gì")) &&
                                          (messageLower.Contains("sản phẩm") || messageLower.Contains("sp"));

                    if (isAskingForList)
                    {
                        // Return ALL products, not just keyword matches
                        var allProducts = await _context.TbProducts
                            .Where(p => p.IsActive == true)
                            .OrderBy(p => p.Title)
                            .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.Alias, p.Image, p.Description })
                            .ToListAsync();

                        if (allProducts.Any())
                        {
                            Console.WriteLine($"[CHATBOT DEBUG] Listing ALL {allProducts.Count} active products");
                            contextInfo += $"\n[DATABASE INFO] Danh sách tất cả {allProducts.Count} sản phẩm (với hình ảnh và link):\n";
                            foreach (var p in allProducts)
                            {
                                var productUrl = $"/san-pham/{p.Alias}-{p.ProductId}";
                                var imageUrl = !string.IsNullOrEmpty(p.Image) ? p.Image : "/images/no-image.jpg";
                                var description = !string.IsNullOrEmpty(p.Description) ? p.Description.Replace("\"", "'").Replace("\n", " ").Replace("\r", "") : "";
                                Console.WriteLine($"  - {p.Title}: {p.PriceSale:N0} VNĐ (Stock: {p.Quantity}) - {productUrl}");
                                contextInfo += $"{{\"title\":\"{p.Title}\",\"price\":\"{p.PriceSale:N0} VNĐ\",\"stock\":{p.Quantity},\"url\":\"{productUrl}\",\"image\":\"{imageUrl}\",\"description\":\"{description}\"}}\n";
                            }
                        }
                    }
                    // Search Products by keywords (only if NOT asking for full list)
                    else if (searchTerms.Any())
                    {
                        var products = await _context.TbProducts
                            .Where(p => p.IsActive == true && searchTerms.Any(t =>
                                p.Title.ToLower().Contains(t) ||
                                (p.Description != null && p.Description.ToLower().Contains(t))))
                            .Take(5)
                            .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.Alias, p.Image, p.Description })
                            .ToListAsync();

                        if (products.Any())
                        {
                            Console.WriteLine($"[CHATBOT DEBUG] Found {products.Count} products matching keywords: {string.Join(", ", searchTerms)}");
                            contextInfo += "\n[DATABASE INFO] Sản phẩm tìm thấy (với hình ảnh và link):\n";
                            foreach (var p in products)
                            {
                                var productUrl = $"/product/{p.Alias}-{p.ProductId}.html";
                                var imageUrl = !string.IsNullOrEmpty(p.Image) ? p.Image : "/images/no-image.jpg";
                                var description = !string.IsNullOrEmpty(p.Description) ? p.Description.Replace("\"", "'").Replace("\n", " ").Replace("\r", "") : "";
                                Console.WriteLine($"  - {p.Title}: {p.PriceSale:N0} VNĐ (Stock: {p.Quantity}) - {productUrl}");
                                contextInfo += $"{{\"title\":\"{p.Title}\",\"price\":\"{p.PriceSale:N0} VNĐ\",\"stock\":{p.Quantity},\"url\":\"{productUrl}\",\"image\":\"{imageUrl}\",\"description\":\"{description}\"}}\n";
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[CHATBOT DEBUG] No products found for keywords: {string.Join(", ", searchTerms)}");
                        }
                    }

                    // Search Categories by keywords
                    if (searchTerms.Any() && (messageLower.Contains("danh mục") || messageLower.Contains("loại") || messageLower.Contains("category")))
                    {
                        var categories = await _context.TbProductCategories
                                                    .Where(c => searchTerms.Any(t =>
                                                        c.Title.ToLower().Contains(t) ||
                                                        (c.Description != null && c.Description.ToLower().Contains(t))))
                                                    .Take(5)
                                                    .Select(c => new { c.Title, c.Description })
                                                    .ToListAsync();

                        if (categories.Any())
                        {
                            contextInfo += "\n[DATABASE INFO] Danh mục tìm thấy:\n";
                            foreach (var c in categories)
                            {
                                contextInfo += $"- {c.Title}: {c.Description}\n";
                            }
                        }
                        else
                        {
                            // If no specific match, show all categories
                            var allCategories = await _context.TbProductCategories
                                .Take(10)
                                .Select(c => new { c.Title })
                                .ToListAsync();

                            if (allCategories.Any())
                            {
                                contextInfo += "\n[DATABASE INFO] Các danh mục có sẵn:\n";
                                foreach (var c in allCategories)
                                {
                                    contextInfo += $"- {c.Title}\n";
                                }
                            }
                        }
                    }

                    // Search Blogs by keywords
                    if (searchTerms.Any() && (messageLower.Contains("blog") || messageLower.Contains("bài viết") || messageLower.Contains("tin tức")))
                    {
                        var blogs = await _context.TbBlogs
                            .Where(b => b.IsActive == true && searchTerms.Any(t =>
                                b.Title.ToLower().Contains(t) ||
                                (b.Description != null && b.Description.ToLower().Contains(t)) ||
                                (b.Detail != null && b.Detail.ToLower().Contains(t))))
                            .Take(5)
                            .Select(b => new { b.Title, b.Description, b.Detail })
                            .ToListAsync();

                        if (blogs.Any())
                        {
                            contextInfo += "\n[DATABASE INFO] Bài viết tìm thấy:\n";
                            foreach (var b in blogs)
                            {
                                contextInfo += $"- {b.Title}: {b.Description}\n";
                            }
                        }
                    }

                    // Log context for debugging
                    if (!string.IsNullOrEmpty(contextInfo))
                    {
                        Console.WriteLine($"[CHATBOT DEBUG] Context being sent:\n{contextInfo}");
                    }
                }

                // Construct the payload for Gemini with system instruction
                var requestPayload = new Dictionary<string, object>();

                // Add system instruction if we have context
                if (!string.IsNullOrEmpty(contextInfo))
                {
                    var systemInstruction = $@"Bạn là trợ lý AI cho một cửa hàng thương mại điện tử. 

QUAN TRỌNG: Bạn PHẢI sử dụng thông tin sau đây từ database để trả lời câu hỏi của người dùng. Đây là dữ liệu THỰC TẾ và CHÍNH XÁC từ hệ thống:

{contextInfo}

Khi người dùng hỏi về số lượng, sản phẩm, danh mục, hoặc bất kỳ thông tin nào có trong DATABASE INFO ở trên, bạn PHẢI trả lời dựa trên dữ liệu đó. KHÔNG hỏi lại người dùng để làm rõ nếu thông tin đã có sẵn trong database.

QUAN TRỌNG VỀ HIỂN THỊ SẢN PHẨM:
Khi có dữ liệu sản phẩm dạng JSON ({{""title"":...,""price"":...,""stock"":...,""url"":...,""image"":...,""description"":...}}), bạn PHẢI format thành HTML card đẹp mắt như sau:

<div style=""display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 15px; margin: 15px 0;"">
  <div style=""border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; transition: transform 0.2s; cursor: pointer; background: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"" onclick=""window.location.href='{{url}}'"">
    <img src=""{{image}}"" alt=""{{title}}"" style=""width: 100%; height: 150px; object-fit: cover;"">
    <div style=""padding: 12px;"">
      <h4 style=""margin: 0 0 8px 0; font-size: 14px; font-weight: 600; color: #333; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;"">{{title}}</h4>
      <p style=""margin: 0 0 8px 0; font-size: 12px; color: #666; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;"">{{description}}</p>
      <p style=""margin: 0; color: #e74c3c; font-size: 16px; font-weight: bold;"">{{price}}</p>
      <p style=""margin: 5px 0 0 0; font-size: 12px; color: #7f8c8d;"">Còn {{stock}} sản phẩm</p>
    </div>
  </div>
</div>

                    requestPayload["systemInstruction"] = new
                    {
                        parts = new[] { new { text = systemInstruction } }
                    }
                    ;


                    // Add conversation contents
                    if (request.Contents != null && request.Contents.Any())
                    {
                        requestPayload["contents"] = request.Contents;
                    }

                    var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:gener...{apiKey}";

                    var jsonContent = JsonSerializer.Serialize(requestPayload);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(apiUrl, httpContent);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, responseString);
                    }

                    return Content(responseString, "application/json");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("TestDatabase")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var totalProducts = await _context.TbProducts.CountAsync(p => p.IsActive == true);
                var totalCategories = await _context.TbProductCategories.CountAsync();
                var totalBlogs = await _context.TbBlogs.CountAsync(b => b.IsActive == true);

                var sampleProducts = await _context.TbProducts
                    .Where(p => p.IsActive == true)
                    .Take(5)
                    .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.IsActive })
                    .ToListAsync();

                var result = new
                {
                    TotalActiveProducts = totalProducts,
                    TotalCategories = totalCategories,
                    TotalActiveBlogs = totalBlogs,
                    SampleProducts = sampleProducts,
                    Message = "Database connection successful!"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        public class ChatRequest
        {
            [JsonPropertyName("contents")]
            public List<Content> Contents { get; set; }
        }

        public class Content
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("parts")]
            public List<Part> Parts { get; set; }
        }

        public class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("inline_data")]
            public object InlineData { get; set; }
        }
    }
}


public class ChatRequest
{
    [JsonPropertyName("contents")]
    public List<Content> Contents { get; set; }
}

public class Content
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("inline_data")]
    public object InlineData { get; set; }
}