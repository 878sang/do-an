using Do_an_1.Models; // Đảm bảo đúng namespace model của bạn
  // Đảm bảo đúng namespace chứa DbContext (thường là Data)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Do_an_1.Controllers // SỬA: Đổi namespace từ th3 -> Do_an_1
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly FashionStoreDbContext _context; // Kiểm tra lại tên DbContext của bạn
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // Constructor
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
                // SỬA: Đổi thành GeminiAI cho khớp với appsettings.json cũ của bạn
                var apiKey = _configuration["GeminiAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Fallback thử key khác nếu key trên không có
                    apiKey = _configuration["Gemini:ApiKey"];
                    if (string.IsNullOrEmpty(apiKey))
                        return StatusCode(500, "API Key not configured.");
                }

                var userMessage = request.Contents.LastOrDefault()?.Parts.FirstOrDefault()?.Text;
                string contextInfo = "";

                // --- PHẦN RAG: TÌM KIẾM DỮ LIỆU ---
                if (!string.IsNullOrEmpty(userMessage))
                {
                    var messageLower = userMessage.ToLower();
                    var keywords = messageLower.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    var searchTerms = keywords.Where(k => k.Length > 2).ToList();

                    // 1. Kiểm tra hỏi số lượng
                    bool isAskingProductCount = (keywords.Contains("bao") && keywords.Contains("nhiêu")) ||
                                                 keywords.Contains("tổng") ||
                                                 keywords.Contains("số");

                    if (isAskingProductCount && (messageLower.Contains("sản phẩm") || messageLower.Contains("sp")))
                    {
                        var totalProducts = await _context.TbProducts.CountAsync(p => p.IsActive == true);
                        contextInfo += $"[DATABASE INFO] Tổng số sản phẩm đang kinh doanh: {totalProducts}\n";
                    }

                    // 2. Logic liệt kê sản phẩm (Search hoặc List all)
                    bool isAskingForList = (messageLower.Contains("tên") || messageLower.Contains("danh sách") ||
                                           messageLower.Contains("liệt kê") || messageLower.Contains("cho tôi biết")) &&
                                           (messageLower.Contains("sản phẩm") || messageLower.Contains("sp"));

                    List<dynamic> products = new List<dynamic>();

                    if (isAskingForList)
                    {
                        // Lấy tất cả (giới hạn 20 để tránh quá tải token)
                        var list = await _context.TbProducts
                            .Where(p => p.IsActive == true)
                            .OrderBy(p => p.Title)
                            .Take(20)
                            .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.Alias, p.Image, p.Description })
                            .ToListAsync();
                        products.AddRange(list);
                    }
                    else if (searchTerms.Any())
                    {
                        // Tìm theo từ khóa
                        var list = await _context.TbProducts
                            .Where(p => p.IsActive == true && searchTerms.Any(t =>
                                p.Title.ToLower().Contains(t) ||
                                (p.Description != null && p.Description.ToLower().Contains(t))))
                            .Take(5)
                            .Select(p => new { p.ProductId, p.Title, p.PriceSale, p.Quantity, p.Alias, p.Image, p.Description })
                            .ToListAsync();
                        products.AddRange(list);
                    }

                    // Format dữ liệu sản phẩm vào context
                    if (products.Any())
                    {
                        contextInfo += $"\n[DATABASE INFO] Tìm thấy {products.Count} sản phẩm:\n";
                        foreach (var p in products)
                        {
                            var productUrl = $"/product/{p.Alias}-{p.ProductId}.html"; // Đảm bảo route này đúng với web của bạn
                            var imageUrl = !string.IsNullOrEmpty(p.Image) ? p.Image : "/assets/img/logo/nav-log.png"; // Ảnh mặc định nếu null
                            // Xử lý chuỗi description để tránh lỗi JSON
                            var cleanDesc = !string.IsNullOrEmpty(p.Description) ?
                                            p.Description.Replace("\"", "'").Replace("\n", " ").Replace("\r", "") : "";

                            // Thêm vào context dạng JSON line để AI dễ đọc
                            contextInfo += $"{{\"title\":\"{p.Title}\",\"price\":\"{p.PriceSale:N0} VNĐ\",\"stock\":{p.Quantity},\"url\":\"{productUrl}\",\"image\":\"{imageUrl}\",\"description\":\"{cleanDesc}\"}}\n";
                        }
                    }

                    // 3. Tìm danh mục
                    if (searchTerms.Any() && (messageLower.Contains("danh mục") || messageLower.Contains("loại")))
                    {
                        var categories = await _context.TbProductCategories
                           .Where(c => searchTerms.Any(t => c.Title.ToLower().Contains(t)))
                           .Take(5)
                           .Select(c => new { c.Title, c.Description })
                           .ToListAsync();

                        if (categories.Any())
                        {
                            contextInfo += "\n[DATABASE INFO] Danh mục:\n";
                            foreach (var c in categories) contextInfo += $"- {c.Title}\n";
                        }
                    }
                }

                // --- GỬI REQUEST SANG GEMINI ---
                var requestPayload = new Dictionary<string, object>();

                // System Instruction (Chỉ dẫn hệ thống)
                if (!string.IsNullOrEmpty(contextInfo))
                {
                    var systemInstruction = $@"Bạn là trợ lý AI shop thời trang.
DỮ LIỆU TỪ HỆ THỐNG (Sử dụng để trả lời):
{contextInfo}

QUY TẮC:
1. Nếu có dữ liệu JSON sản phẩm ở trên, hãy hiển thị dạng HTML Card grid đẹp mắt:
<div style='display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 10px;'>
  <div style='border:1px solid #ddd; border-radius:5px; padding:10px;'>
     <img src='{{image}}' style='width:100%;height:150px;object-fit:cover;' />
     <h5 style='font-size:14px; margin:5px 0;'>{{title}}</h5>
     <p style='color:red; font-weight:bold;'>{{price}}</p>
     <a href='{{url}}' style='display:block; background:#333; color:#fff; text-align:center; padding:5px; text-decoration:none; border-radius:3px;'>Xem chi tiết</a>
  </div>
</div>
2. Trả lời ngắn gọn, thân thiện bằng tiếng Việt.";

                    requestPayload["systemInstruction"] = new { parts = new[] { new { text = systemInstruction } } };
                }

                if (request.Contents != null && request.Contents.Any())
                {
                    requestPayload["contents"] = request.Contents;
                }

                // URL API (Dùng gemini-1.5-flash hoặc gemini-2.0-flash-exp tùy key của bạn)
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

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
                var count = await _context.TbProducts.CountAsync();
                return Ok(new { Message = "Kết nối thành công", ProductCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    // --- CÁC CLASS MODEL (ĐÃ CHUYỂN RA NGOÀI VÀ XÓA TRÙNG LẶP) ---
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