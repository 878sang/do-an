using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;

namespace Do_an_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ChatController : ControllerBase
    {
        private readonly FashionStoreDbContext _context;
        private readonly ILogger<ChatController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        // Gemini API Configuration
        private const string GEMINI_API_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        
        public ChatController(
            FashionStoreDbContext context, 
            ILogger<ChatController> logger, 
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Get chat history (guest or user)
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            try
            {
                await CleanOldMessagesAsync();

                var userId = GetUserId();
                var guestToken = GetGuestToken();

                IQueryable<TbChatMessage> query = _context.TbChatMessages;

                if (userId.HasValue)
                {
                    query = query.Where(m => m.UserId == userId);
                }
                else if (!string.IsNullOrEmpty(guestToken))
                {
                    query = query.Where(m => m.GuestToken == guestToken);
                }
                else
                {
                    return Ok(new List<object>());
                }

                var messages = await query
                    .OrderBy(m => m.CreatedDate)
                    .Select(m => new
                    {
                        m.MessageId,
                        m.UserId,
                        m.GuestToken,
                        m.Sender,
                        m.Message,
                        m.CreatedDate
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử chat");
                return StatusCode(500, new { error = "Lỗi server khi lấy lịch sử chat" });
            }
        }

        // Send message with real Gemini AI integration
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Tin nhắn không được để trống" });
                }

                var userId = GetUserId();
                var guestToken = GetOrCreateGuestToken();

                // Save user message
                var userMsg = await SaveUserMessage(userId, guestToken, request.Message);

                // Check for special queries first
                var specialResponse = await HandleSpecialQueries(request.Message.ToLower());
                if (specialResponse != null)
                {
                    var botReply = await SaveBotMessage(userId, guestToken, specialResponse.Message);
                    return Ok(BuildResponse(userMsg, botReply, specialResponse.Data));
                }

                // Get AI response with context
                var aiResponse = await GetAIResponse(userId, guestToken, request.Message);
                
                // Save bot message
                var botMsg = await SaveBotMessage(userId, guestToken, aiResponse);

                // Detect and attach structured data (categories/products)
                var structuredData = await DetectAndFetchStructuredData(request.Message.ToLower());
                
                return Ok(BuildResponse(userMsg, botMsg, structuredData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi tin nhắn: {Message}", ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { error = $"Lỗi server khi xử lý tin nhắn: {ex.Message}" });
            }
        }

        // Real Gemini AI Integration
        private async Task<string> GetAIResponse(int? userId, string guestToken, string userMessage)
        {
            try
            {
                // Get system context
                var systemContext = await BuildSystemContext();
                
                // Get chat history
                var chatHistory = await GetChatHistory(userId, guestToken, 10);
                
                // Build Gemini API request
                var geminiRequest = new
                {
                    contents = BuildGeminiContents(systemContext, chatHistory, userMessage),
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 800,
                        topP = 0.8,
                        topK = 40
                    },
                    safetySettings = new[]
                    {
                        new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                    }
                };

                // Get API Key from configuration
                var apiKey = _configuration["GeminiAI:ApiKey"] ?? "YOUR_API_KEY_HERE";
                var requestUrl = $"{GEMINI_API_ENDPOINT}?key={apiKey}";

                // Call Gemini API
                var jsonRequest = JsonSerializer.Serialize(geminiRequest);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(requestUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API returned {StatusCode}", response.StatusCode);
                    return GetFallbackResponse(userMessage);
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // Extract AI text
                var aiText = geminiResponse
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? GetFallbackResponse(userMessage);

                // Clean up response
                return CleanAIResponse(aiText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini API");
                return GetFallbackResponse(userMessage);
            }
        }

        // Build Gemini API contents with history
        private List<object> BuildGeminiContents(string systemContext, List<TbChatMessage> history, string currentMessage)
        {
            var contents = new List<object>();
            
            // Add system context as first user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = systemContext } }
            });
            
            contents.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Tôi hiểu. Tôi sẽ hỗ trợ bạn với thông tin về cửa hàng thời trang theo đúng dữ liệu bạn cung cấp." } }
            });

            // Add chat history
            foreach (var msg in history)
            {
                contents.Add(new
                {
                    role = msg.Sender == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Message ?? "" } }
                });
            }

            // Add current message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = currentMessage } }
            });

            return contents;
        }

        // Build system context from database
        private async Task<string> BuildSystemContext()
        {
            var categories = await _context.TbProductCategories
                .Include(c => c.TbProducts.Where(p => p.IsActive == true))
                .Where(c => c.TbProducts.Any(p => p.IsActive == true))
                .Select(c => new
                {
                    c.Title,
                    ProductCount = c.TbProducts.Count(p => p.IsActive == true)
                })
                .Take(10)
                .ToListAsync();

            var sampleProducts = await _context.TbProducts
                .Include(p => p.CategoryProduct)
                .Where(p => p.IsActive == true && p.IsBestSeller == true)
                .OrderByDescending(p => p.Star)
                .Take(5)
                .Select(p => new
                {
                    p.Title,
                    CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Title : "Khác",
                    Price = p.PriceSale ?? p.Price
                })
                .ToListAsync();

            var categoryList = string.Join("\n", categories.Select(c => $"- {c.Title}: {c.ProductCount} sản phẩm"));
            var productList = string.Join("\n", sampleProducts.Select(p => $"- {p.Title} ({p.CategoryName}): {FormatPrice(p.Price)}"));

            return $@"BẠN LÀ TRỢ LÝ BÁN HÀNG CỦA CỬA HÀNG THỜI TRANG NETMARK

**QUY TẮC TRẢLỜI:**
1. Trả lời ngắn gọn, thân thiện và chuyên nghiệp (tối đa 2-3 câu)
2. Chỉ sử dụng thông tin có trong danh sách bên dưới
3. KHÔNG bịa đặt sản phẩm hoặc giá cả
4. Khi khách hỏi về danh mục, CHỈ nói chung chung, KHÔNG liệt kê sản phẩm cụ thể
5. Khuyến khích khách xem danh sách chi tiết trên website
6. Sử dụng emoji phù hợp để tạo cảm giác thân thiện

**DANH MỤC SẢN PHẨM:**
{categoryList}

**SẢN PHẨM NỔI BẬT:**
{productList}

**CÁC TÍNH NĂNG HỖ TRỢ:**
- Tra cứu đơn hàng (khách có thể hỏi về mã đơn hàng)
- Tư vấn sản phẩm theo tiêu chí (giới tính, phong cách, giá)
- Thông tin về danh mục và sản phẩm

Hãy bắt đầu hỗ trợ khách hàng!";
        }

        // Get chat history
        private async Task<List<TbChatMessage>> GetChatHistory(int? userId, string guestToken, int limit)
        {
            var query = _context.TbChatMessages
                .Where(m => userId.HasValue ? m.UserId == userId : m.GuestToken == guestToken)
                .OrderByDescending(m => m.CreatedDate)
                .Take(limit)
                .OrderBy(m => m.CreatedDate);

            return await query.ToListAsync();
        }

        // Handle special queries (categories, product search)
        private async Task<SpecialResponse?> HandleSpecialQueries(string messageLower)
        {
            // Check for category listing
            var categoryKeywords = new[] { "danh mục", "loại", "category", "có những gì", "có gì", "loại nào" };
            if (categoryKeywords.Any(kw => messageLower.Contains(kw)))
            {
                var categories = await FetchCategories();
                return new SpecialResponse
                {
                    Message = "Chúng tôi có các danh mục sản phẩm sau. Bạn có thể xem danh sách chi tiết bên dưới: 📂",
                    Data = new { categories }
                };
            }

            // Check for specific category match
            var matchedCategory = await FindMatchingCategory(messageLower);
            if (matchedCategory != null)
            {
                var productList = await FetchCategoryProducts(matchedCategory.CategoryProductId);
                return new SpecialResponse
                {
                    Message = $"Đây là các sản phẩm trong danh mục {matchedCategory.Title}: 🛍️",
                    Data = new
                    {
                        category_matched = matchedCategory.Title,
                        category_id = matchedCategory.CategoryProductId,
                        product_list = productList.ProductList,
                        top_products = productList.TopProducts,
                        show_category_button = true
                    }
                };
            }

            // Check for product search
            var searchKeywords = ExtractProductKeywords(messageLower);
            if (searchKeywords.Any(kw => kw.Length >= 3))
            {
                var products = await SearchProducts(messageLower, searchKeywords);
                if (products != null && products.Any())
                {
                    return new SpecialResponse
                    {
                        Message = $"Tìm thấy {products.Count()} sản phẩm phù hợp với yêu cầu của bạn: ✨",
                        Data = new
                        {
                            products,
                            search_type = products.Count() == 1 ? "exact_match" : "keyword_match",
                            is_specific_product = true
                        }
                    };
                }
            }

            return null;
        }

        // Detect and fetch structured data
        private async Task<object?> DetectAndFetchStructuredData(string messageLower)
        {
            // This is already handled in HandleSpecialQueries, 
            // but we keep this for additional context detection
            return null;
        }

        // Fetch categories
        private async Task<List<object>> FetchCategories()
        {
            return await _context.TbProductCategories
                .Include(c => c.TbProducts.Where(p => p.IsActive == true))
                .Where(c => c.TbProducts.Any(p => p.IsActive == true))
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    id = c.CategoryProductId,
                    name = c.Title,
                    slug = c.Alias ?? c.Title.ToLower().Replace(" ", "-"),
                    product_count = c.TbProducts.Count(p => p.IsActive == true)
                })
                .ToListAsync<object>();
        }

        // Find matching category
        private async Task<TbProductCategory?> FindMatchingCategory(string messageLower)
        {
            var allCategories = await _context.TbProductCategories.ToListAsync();
            return allCategories.FirstOrDefault(cat =>
            {
                var catName = cat.Title?.ToLower() ?? "";
                return messageLower.Contains(catName);
            });
        }

        // Fetch category products
        private async Task<(List<object> ProductList, List<object> TopProducts)> FetchCategoryProducts(int categoryId)
        {
            var products = await _context.TbProducts
                .Include(p => p.TbOrderDetails)
                .Where(p => p.CategoryProductId == categoryId && p.IsActive == true)
                .OrderByDescending(p => p.TbOrderDetails.Sum(od => od.Quantity ?? 0))
                .ToListAsync();

            var productList = products.Select(p => new
            {
                id = p.ProductId,
                name = p.Title
            }).ToList<object>();

            var topProducts = products.Take(3).Select(p => new
            {
                id = p.ProductId,
                name = p.Title,
                slug = p.Alias ?? p.Title?.ToLower().Replace(" ", "-"),
                price = p.PriceSale ?? p.Price,
                price_formatted = FormatPrice(p.PriceSale ?? p.Price),
                description = TruncateDescription(p.Description, 150),
                image_url = p.Image ?? "/assets/img/default-product.png",
                detail_url = $"/product/{(p.Alias ?? p.ProductId.ToString())}-{p.ProductId}.html"
            }).ToList<object>();

            return (productList, topProducts);
        }

        // Search products
        private async Task<List<object>?> SearchProducts(string cleanMessage, List<string> keywords)
        {
            // Try exact match first
            var exactMatch = await _context.TbProducts
                .Include(p => p.CategoryProduct)
                .Where(p => p.IsActive == true && p.Title != null && 
                           p.Title.ToLower().Contains(cleanMessage))
                .FirstOrDefaultAsync();

            if (exactMatch != null)
            {
                return new List<object>
                {
                    new
                    {
                        id = exactMatch.ProductId,
                        name = exactMatch.Title,
                        slug = exactMatch.Alias ?? exactMatch.Title?.ToLower().Replace(" ", "-"),
                        price = exactMatch.PriceSale ?? exactMatch.Price,
                        price_formatted = FormatPrice(exactMatch.PriceSale ?? exactMatch.Price),
                        description = TruncateDescription(exactMatch.Description, 150),
                        image_url = exactMatch.Image ?? "/assets/img/default-product.png",
                        detail_url = $"/product/{(exactMatch.Alias ?? exactMatch.ProductId.ToString())}-{exactMatch.ProductId}.html"
                    }
                };
            }

            // Keyword search
            var matchedProducts = await _context.TbProducts
                .Include(p => p.CategoryProduct)
                .Include(p => p.TbOrderDetails)
                .Where(p => p.IsActive == true &&
                           keywords.Any(kw => kw.Length >= 3 && 
                           p.Title != null && p.Title.ToLower().Contains(kw)))
                .ToListAsync();

            if (!matchedProducts.Any()) return null;

            return matchedProducts
                .Select(p => new
                {
                    Product = p,
                    Score = keywords.Count(kw => kw.Length >= 3 && 
                            p.Title != null && p.Title.ToLower().Contains(kw)) * 10 +
                            p.TbOrderDetails.Sum(od => od.Quantity ?? 0)
                })
                .OrderByDescending(p => p.Score)
                .Take(3)
                .Select(p => (object)new
                {
                    id = p.Product.ProductId,
                    name = p.Product.Title,
                    slug = p.Product.Alias ?? p.Product.Title?.ToLower().Replace(" ", "-"),
                    price = p.Product.PriceSale ?? p.Product.Price,
                    price_formatted = FormatPrice(p.Product.PriceSale ?? p.Product.Price),
                    description = TruncateDescription(p.Product.Description, 150),
                    image_url = p.Product.Image ?? "/assets/img/default-product.png",
                    detail_url = $"/product/{(p.Product.Alias ?? p.Product.ProductId.ToString())}-{p.Product.ProductId}.html"
                })
                .ToList();
        }

        // Track order
        [HttpPost("track-order")]
        public async Task<IActionResult> TrackOrder([FromBody] TrackOrderRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrderId))
                {
                    return BadRequest(new { error = "Mã đơn hàng không được để trống" });
                }

                var userId = GetUserId();
                var guestToken = GetOrCreateGuestToken();

                var orderIdStr = Regex.Replace(request.OrderId, @"[^0-9]", "");
                
                if (string.IsNullOrEmpty(orderIdStr) || !int.TryParse(orderIdStr, out int orderId))
                {
                    var botMsg = await SaveBotMessage(userId, guestToken, 
                        "Mã đơn hàng không hợp lệ. Vui lòng nhập mã đơn hàng (ví dụ: #123 hoặc 123).");
                    return Ok(new { bot = botMsg });
                }

                var order = await _context.TbOrders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.TbOrderDetails).ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    var botMsg = await SaveBotMessage(userId, guestToken,
                        $"Không tìm thấy đơn hàng #{orderId}. Vui lòng kiểm tra lại mã đơn hàng.");
                    return Ok(new { bot = botMsg });
                }

                // Build order message
                var orderMessage = BuildOrderMessage(order);
                var botMsg2 = await SaveBotMessage(userId, guestToken, orderMessage);

                // Build order data
                var orderData = BuildOrderData(order);

                return Ok(new { bot = botMsg2, order = orderData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tra cứu đơn hàng");
                return StatusCode(500, new { error = "Lỗi server khi tra cứu đơn hàng" });
            }
        }

        // Build order message
        private string BuildOrderMessage(TbOrder order)
        {
            var statusLabel = order.OrderStatus?.Name ?? "Chưa xác định";
            var statusEmoji = GetStatusEmoji(statusLabel);

            var message = $"📦 **Thông tin đơn hàng #{order.OrderId}**\n\n";
            message += $"**Trạng thái:** {statusEmoji} {statusLabel}\n";
            message += $"**Ngày đặt:** {order.CreatedDate:dd/MM/yyyy HH:mm}\n";
            message += $"**Tổng tiền:** {FormatPrice(order.TotalAmount)}\n\n";

            if (!string.IsNullOrEmpty(order.ShippingAddress) && order.Customer != null)
            {
                message += $"**Địa chỉ giao hàng:**\n";
                message += $"{order.Customer.Name} - {order.Customer.Phone}\n";
                message += $"{order.ShippingAddress}\n\n";
            }

            message += "**Sản phẩm:**\n";
            var index = 1;
            foreach (var item in order.TbOrderDetails)
            {
                var productName = item.Product?.Title ?? "Sản phẩm";
                var total = (item.Price ?? 0) * (item.Quantity ?? 0);
                message += $"{index++}. {productName} x{item.Quantity} - {FormatPrice(total)}\n";
            }

            return message;
        }

        // Build order data
        private object BuildOrderData(TbOrder order)
        {
            var statusLabel = order.OrderStatus?.Name ?? "Chưa xác định";
            return new
            {
                id = order.OrderId,
                status = statusLabel,
                status_label = statusLabel,
                status_emoji = GetStatusEmoji(statusLabel),
                created_at = order.CreatedDate?.ToString("dd/MM/yyyy HH:mm"),
                total_price = FormatPrice(order.TotalAmount),
                subtotal = FormatPrice(order.TbOrderDetails.Sum(od => (od.Price ?? 0) * (od.Quantity ?? 0))),
                shipping_fee = "25.000 ₫",
                discount_amount = "0 ₫",
                shipping_address = order.Customer != null ? new
                {
                    full_name = order.Customer.Name,
                    phone = order.Customer.Phone,
                    address = order.ShippingAddress
                } : null,
                items = order.TbOrderDetails.Select(od => new
                {
                    product_name = od.Product?.Title ?? "Sản phẩm",
                    quantity = od.Quantity ?? 0,
                    price = FormatPrice(od.Price),
                    total = FormatPrice((od.Price ?? 0) * (od.Quantity ?? 0))
                }),
                detail_url = $"/order/{order.OrderId}"
            };
        }

        // Perfume Advisor
        [HttpPost("perfume-advisor")]
        public async Task<IActionResult> PerfumeAdvisor([FromBody] PerfumeAdvisorRequest request)
        {
            try
            {
                var userId = GetUserId();
                var guestToken = GetOrCreateGuestToken();

                var query = _context.TbProducts
                    .Include(p => p.CategoryProduct)
                    .Include(p => p.TbOrderDetails)
                    .Where(p => p.IsActive == true)
                    .AsQueryable();

                var searchKeywords = new List<string>();

                // Gender filter
                if (!string.IsNullOrEmpty(request.Gender))
                {
                    var genderMap = new Dictionary<string, string[]>
                    {
                        { "nam", new[] { "nam", "men", "homme", "for him" } },
                        { "nữ", new[] { "nữ", "women", "femme", "for her", "lady" } },
                        { "unisex", new[] { "unisex", "for all", "chung" } }
                    };

                    if (genderMap.TryGetValue(request.Gender.ToLower(), out var keywords))
                    {
                        searchKeywords.AddRange(keywords);
                    }
                }

                if (!string.IsNullOrEmpty(request.Style)) searchKeywords.Add(request.Style);
                if (!string.IsNullOrEmpty(request.Note)) searchKeywords.Add(request.Note);

                // Apply keyword search
                if (searchKeywords.Any())
                {
                    query = query.Where(p =>
                        searchKeywords.Any(keyword =>
                            (p.Title != null && p.Title.ToLower().Contains(keyword)) ||
                            (p.Description != null && p.Description.ToLower().Contains(keyword))
                        )
                    );
                }

                // Price range filter
                if (!string.IsNullOrEmpty(request.PriceRange))
                {
                    var range = request.PriceRange.Split('-');
                    if (range.Length == 2 && 
                        decimal.TryParse(range[0], out decimal minPrice) &&
                        decimal.TryParse(range[1], out decimal maxPrice))
                    {
                        query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                    }
                }

                var products = await query.ToListAsync();

                var sortedProducts = products
                    .OrderByDescending(p => p.TbOrderDetails.Sum(od => od.Quantity ?? 0))
                    .Take(6)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Title,
                        slug = p.Alias ?? p.Title?.ToLower().Replace(" ", "-"),
                        price = p.PriceSale ?? p.Price,
                        price_formatted = FormatPrice(p.PriceSale ?? p.Price),
                        description = TruncateDescription(p.Description, 150),
                        image_url = p.Image ?? "/assets/img/default-product.png",
                        detail_url = $"/product/{(p.Alias ?? p.ProductId.ToString())}-{p.ProductId}.html"
                    })
                    .ToList();

                // Build criteria text
                var criteria = new List<string>();
                if (!string.IsNullOrEmpty(request.Gender)) criteria.Add($"giới tính: {request.Gender}");
                if (!string.IsNullOrEmpty(request.Style)) criteria.Add($"phong cách: {request.Style}");
                if (!string.IsNullOrEmpty(request.Note)) criteria.Add($"nốt hương: {request.Note}");
                if (!string.IsNullOrEmpty(request.PriceRange))
                {
                    var range = request.PriceRange.Split('-');
                    if (range.Length == 2)
                    {
                        var priceText = $"{FormatPrice(decimal.Parse(range[0]))} - {FormatPrice(decimal.Parse(range[1]))}";
                        criteria.Add($"mức giá: {priceText}");
                    }
                }

                var criteriaText = string.Join(", ", criteria);
                var message = sortedProducts.Any()
                    ? $"Dựa trên tiêu chí của bạn ({criteriaText}), chúng tôi gợi ý {sortedProducts.Count} sản phẩm phù hợp nhất: 🌸"
                    : $"Xin lỗi, hiện tại chúng tôi chưa có sản phẩm phù hợp với tiêu chí ({criteriaText}). Bạn có thể thử điều chỉnh lại tiêu chí. 🌸";

                var botMsg = await SaveBotMessage(userId, guestToken, message);

                return Ok(new
                {
                    bot = new
                    {
                        botMsg.MessageId,
                        botMsg.Sender,
                        botMsg.Message,
                        botMsg.CreatedDate
                    },
                    products = sortedProducts,
                    is_specific_product = sortedProducts.Any()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tư vấn sản phẩm");
                return StatusCode(500, new { error = "Lỗi server khi tư vấn sản phẩm" });
            }
        }

        // ========== Helper Methods ==========

        private async Task<TbChatMessage> SaveUserMessage(int? userId, string guestToken, string message)
        {
            try
            {
                var userMsg = new TbChatMessage
                {
                    UserId = userId,
                    GuestToken = userId.HasValue ? null : guestToken,
                    Sender = "user",
                    Message = message.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.TbChatMessages.Add(userMsg);
                await _context.SaveChangesAsync();
                return userMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu tin nhắn người dùng: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<TbChatMessage> SaveBotMessage(int? userId, string guestToken, string message)
        {
            try
            {
                var botMsg = new TbChatMessage
                {
                    UserId = userId,
                    GuestToken = userId.HasValue ? null : guestToken,
                    Sender = "bot",
                    Message = CleanAIResponse(message),
                    CreatedDate = DateTime.Now
                };

                _context.TbChatMessages.Add(botMsg);
                await _context.SaveChangesAsync();
                return botMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu tin nhắn bot: {Message}", ex.Message);
                throw;
            }
        }

        private object BuildResponse(TbChatMessage userMsg, TbChatMessage botMsg, object? additionalData)
        {
            var response = new
            {
                user = new
                {
                    userMsg.MessageId,
                    userMsg.Sender,
                    userMsg.Message,
                    userMsg.CreatedDate
                },
                bot = new
                {
                    botMsg.MessageId,
                    botMsg.Sender,
                    botMsg.Message,
                    botMsg.CreatedDate
                }
            };

            if (additionalData != null)
            {
                // Merge additional data with response
                var dict = new Dictionary<string, object?>
                {
                    ["user"] = response.user,
                    ["bot"] = response.bot
                };

                foreach (var prop in additionalData.GetType().GetProperties())
                {
                    dict[prop.Name] = prop.GetValue(additionalData);
                }

                return dict;
            }

            return response;
        }

        private string CleanAIResponse(string text)
        {
            // Remove markdown bold markers
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
            // Remove bullet points at start of lines
            text = Regex.Replace(text, @"^\s*[-*•]\s+", "", RegexOptions.Multiline);
            // Remove excessive newlines
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }

        private string GetFallbackResponse(string userMessage)
        {
            var lower = userMessage.ToLower();
            
            if (lower.Contains("xin chào") || lower.Contains("hello") || lower.Contains("hi"))
                return "Xin chào! Tôi có thể giúp gì cho bạn về sản phẩm của chúng tôi? 😊";
            
            if (lower.Contains("cảm ơn") || lower.Contains("thank"))
                return "Rất vui được hỗ trợ bạn! Chúc bạn có trải nghiệm mua sắm tuyệt vời! 🎉";
            
            if (lower.Contains("tạm biệt") || lower.Contains("bye"))
                return "Tạm biệt! Hẹn gặp lại bạn sớm! 👋";
            
            return "Tôi có thể giúp bạn tìm hiểu về sản phẩm, danh mục hoặc tra cứu đơn hàng. Bạn cần hỗ trợ gì? 🛍️";
        }

        private int? GetUserId()
        {
            // TODO: Implement authentication and get user ID from claims
            return null;
        }

        private string? GetGuestToken()
        {
            return Request.Cookies["chat_token"];
        }

        private string GetOrCreateGuestToken()
        {
            var token = GetGuestToken();
            if (string.IsNullOrEmpty(token))
            {
                token = "guest_" + Guid.NewGuid().ToString("N");
                Response.Cookies.Append("chat_token", token, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(180),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
            }
            return token;
        }

        private async Task CleanOldMessagesAsync()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-1);
                var oldMessages = await _context.TbChatMessages
                    .Where(m => m.CreatedDate < cutoffDate)
                    .ToListAsync();
                
                _context.TbChatMessages.RemoveRange(oldMessages);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể xóa tin nhắn cũ");
            }
        }

        private List<string> ExtractProductKeywords(string message)
        {
            var stopwords = new[] { 
                "có", "không", "bạn", "tôi", "mình", "cho", "của", "về", "với", "và", "hay",
                "thì", "là", "được", "đã", "sẽ", "đang", "bị", "nào", "gì", "như", "thế", 
                "này", "đó", "kia", "những", "các", "một", "hai", "ba", "muốn", "cần", 
                "tìm", "xem", "mua", "giá", "sản", "phẩm"
            };

            var cleanMessage = Regex.Replace(message, @"[^\p{L}\p{N}\s]", " ");
            var words = cleanMessage.ToLower()
                .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            return words
                .Where(word => word.Length >= 2 && !stopwords.Contains(word))
                .Distinct()
                .ToList();
        }

        private string TruncateDescription(string? description, int maxLength)
        {
            if (string.IsNullOrEmpty(description))
                return "Sản phẩm chất lượng cao";
            
            return description.Length <= maxLength 
                ? description 
                : description.Substring(0, maxLength) + "...";
        }

        private string FormatPrice(decimal? price)
        {
            return price.HasValue 
                ? price.Value.ToString("N0") + " ₫" 
                : "Liên hệ";
        }

        private string GetStatusEmoji(string status)
        {
            return status.ToLower() switch
            {
                "pending" or "chờ xử lý" => "⏳",
                "processing" or "đang xử lý" => "✅",
                "delivered" or "đã giao" => "✅",
                "completed" or "hoàn thành" => "🎉",
                "canceled" or "đã hủy" => "❌",
                _ => "ℹ️"
            };
        }
    }

    // Request/Response Models
    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TrackOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
    }

    public class PerfumeAdvisorRequest
    {
        public string? Gender { get; set; }
        public string? Style { get; set; }
        public string? Note { get; set; }
        public string? PriceRange { get; set; }
    }

    internal class SpecialResponse
    {
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}

