const chatBody = document.querySelector(".chat-body");
const messageInput = document.querySelector(".message-input");
const sendMessageButton = document.querySelector("#send-message");
const fileInput = document.querySelector("#file-input");
const fileUploadWrapper = document.querySelector(".file-upload-wrapper");
const fileCancelButton = document.querySelector("#file-cancel");
const chatbotToggler = document.querySelector("#chatbot-toggler");
const closeChatbot = document.querySelector("#close-chatbot");


// Api setup
const API_KEY = "AIzaSyAhdqyu6kHIyhFyHFKx9TVXFz9mgh8twmA"; // LINK LẤY API KEY: https://aistudio.google.com/apikey
const API_URL = `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=${API_KEY}`;

// Database API endpoints (tách riêng controller chuyên phục vụ truy vấn DB)
const DATABASE_API_BASE = '/api/ChatbotApiDatabase';

// Chat API endpoints (new ChatController)
const CHAT_API_BASE = '/api/Chat';

const userData = {
    message: null,
    file: {
        data: null,
        mime_type: null
    }
};

// const chatHistory = [
//     {
//         role: "model",
//         parts: [{ text: `Đinh Duy Vinh (2005), chàng sinh viên đến từ Quảng Ngãi, hiện đang theo học tại Đại học Duy Tân, Đà Nẵng, là một người trẻ đam mê công nghệ và lập trình. Từ thuở nhỏ, Vinh đã có niềm đam mê mãnh liệt với các thiết bị điện tử và luôn muốn tìm hiểu mọi thứ xung quanh. Chính sự tò mò này đã đưa anh đến với thế giới lập trình ngay từ những năm cấp 3, đặc biệt là trong thời gian giãn cách xã hội do dịch COVID-19. Với thời gian rảnh rỗi, Vinh bắt đầu tự học lập trình web, và rồi từ những dự án nhỏ ban đầu, anh đã phát triển được những sản phẩm hữu ích cho cộng đồng.
// Những dự án mà Vinh thực hiện không chỉ đơn giản là những sản phẩm công nghệ mà còn là minh chứng cho sự sáng tạo và khả năng giải quyết vấn đề của anh. Anh đã tự tay xây dựng một loạt các dự án đa dạng như hệ thống quản lý sinh viên, web game giải trí, website chống lừa đảo, trang web tải ảnh từ Imgur, công cụ tạo mã QR code, dự báo thời tiết trực tuyến, và cả extension Chrome giúp đánh giá nhanh giảng viên của trường Đại học Duy Tân. Không dừng lại ở đó, Vinh còn đắm chìm vào việc khai thác API từ các mạng xã hội như Instagram, Facebook, TikTok và Zalo để lấy thông tin người dùng. Anh cũng đã thử sức với việc tạo module iOS để crack ứng dụng Locket, phát triển API tải video từ TikTok, tạo web chuyển đổi 2FA, và không thể không nhắc đến các bot Telegram mà Vinh viết để tự động hóa các tác vụ một cách hiệu quả.
// Vinh không chỉ giỏi trong việc phát triển các dự án công nghệ mà còn luôn mong muốn chia sẻ những gì mình học được với cộng đồng. Kênh YouTube của anh (YouTube: @duyvinh09) là nơi anh chia sẻ những mẹo, thủ thuật và tiện ích cực kỳ hữu ích mà anh đã tự tìm ra, giúp đỡ mọi người trong hành trình học hỏi công nghệ. Ngoài YouTube, Vinh cũng kết nối và chia sẻ kiến thức qua các nền tảng khác như GitHub (GitHub: duyvinh09) và Facebook (Facebook: duyvinh09), nơi anh luôn sẵn sàng giao lưu, học hỏi từ cộng đồng và giúp đỡ những người có chung niềm đam mê. Đặc biệt, Vinh còn sở hữu một nhóm chat trên Telegram, nơi anh và các bạn có thể trao đổi kiến thức, cùng nhau phát triển và học hỏi từ những người đi trước.
// Với một portfolio đầy ấn tượng tại duyvinh09.github.io và dinhduyvinh.eu.org, Vinh không ngừng khẳng định khả năng của mình qua mỗi dự án. Anh là một chàng trai luôn nỗ lực học hỏi, phát triển và sẵn sàng chia sẻ với cộng đồng những gì anh biết. Với tinh thần sáng tạo không ngừng nghỉ và sự nhiệt huyết trong từng dự án, Đinh Duy Vinh chắc chắn sẽ còn đạt được nhiều thành công và tiếp tục là nguồn cảm hứng cho thế hệ trẻ đam mê công nghệ.` }],
//     },
// ];

const chatHistory = [];

const initialInputHeight = messageInput.scrollHeight;

// Database API functions
const fetchDatabaseData = async (endpoint, params = {}) => {
    try {
        let url = `${DATABASE_API_BASE}${endpoint}`;
        
        // Add query parameters if any
        if (Object.keys(params).length > 0) {
            const searchParams = new URLSearchParams();
            Object.keys(params).forEach(key => {
                if (params[key] !== null && params[key] !== undefined) {
                    searchParams.append(key, params[key]);
                }
            });
            url += `?${searchParams.toString()}`;
        }
        
        console.log('Fetching from URL:', url); // Debug log
        
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const contentType = response.headers.get('content-type') || '';
        const text = await response.text();

        if (!response.ok) {
            try {
                const err = text ? JSON.parse(text) : { error: response.statusText };
                throw new Error(err.error || response.statusText);
            } catch {
                throw new Error(text || response.statusText);
            }
        }

        if (!text) return null;
        if (contentType.includes('application/json')) {
            return JSON.parse(text);
        }
        try { return JSON.parse(text); } catch { return text; }
    } catch (error) {
        console.error('Database API error:', error);
        throw error;
    }
};

const searchDatabaseData = async (query) => {
    try {
        console.log('Searching for:', query); // Debug log
        
        const response = await fetch(`${DATABASE_API_BASE}/search`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ query: query })
        });
        
        const contentType = response.headers.get('content-type') || '';
        const text = await response.text();

        if (!response.ok) {
            try {
                const err = text ? JSON.parse(text) : { error: response.statusText };
                throw new Error(err.error || response.statusText);
            } catch {
                throw new Error(text || response.statusText);
            }
        }

        if (!text) return null;
        if (contentType.includes('application/json')) {
            return JSON.parse(text);
        }
        try { return JSON.parse(text); } catch { return text; }
    } catch (error) {
        console.error('Search API error:', error);
        throw error;
    }
};

// Test API connection
const testApiConnection = async () => {
    try {
        const response = await fetch(`${DATABASE_API_BASE}/test`);
        const text = await response.text();
        if (response.ok) {
            let data;
            try { data = text ? JSON.parse(text) : null; } catch { data = text; }
            console.log('API connection successful:', data);
            return true;
        } else {
            console.error('API connection failed:', response.status);
            return false;
        }
    } catch (error) {
        console.error('API connection test failed:', error);
        return false;
    }
};

// Function to detect if user is asking about database data
const isDatabaseQuery = (message) => {
    const dbKeywords = [
        'sản phẩm', 'product', 'danh mục', 'category', 'đơn hàng', 'order',
        'khách hàng', 'customer', 'blog', 'tin tức', 'news', 'thống kê', 'statistics',
        'tìm kiếm', 'search', 'có bao nhiêu', 'số lượng', 'giá', 'price',
        'mới nhất', 'bán chạy', 'best seller', 'đánh giá', 'review'
    ];
    
    const lowerMessage = message.toLowerCase();
    return dbKeywords.some(keyword => lowerMessage.includes(keyword));
};

// Helper function to create product card HTML
const createProductCardHTML = (product) => {
    const productUrl = product.alias 
        ? `/product/${product.alias}-${product.productId}.html`
        : `/product/details?id=${product.productId}`;
    const imageUrl = product.image || '/assets/img/default-product.png';
    const displayPrice = product.priceSale && product.priceSale < product.price 
        ? product.priceSale 
        : product.price;
    const originalPrice = product.priceSale && product.priceSale < product.price 
        ? product.price 
        : null;
    const shortDescription = product.description 
        ? (product.description.length > 100 ? product.description.substring(0, 100) + '...' : product.description)
        : 'Không có mô tả';
    
    return `
        <div class="chatbot-product-card">
            <div class="product-card-image">
                <img src="${imageUrl}" alt="${product.title || 'Sản phẩm'}" onerror="this.src='/assets/img/default-product.png'">
            </div>
            <div class="product-card-content">
                <h4 class="product-card-title">${product.title || 'Sản phẩm'}</h4>
                <p class="product-card-description">${shortDescription}</p>
                <div class="product-card-price">
                    ${originalPrice 
                        ? `<span class="original-price">${originalPrice.toLocaleString('vi-VN')} ₫</span>` 
                        : ''}
                    <span class="current-price">${displayPrice ? displayPrice.toLocaleString('vi-VN') + ' ₫' : 'Liên hệ'}</span>
                </div>
                <a href="${productUrl}" class="product-card-button" target="_blank">Xem chi tiết</a>
            </div>
        </div>
    `;
};

// Function to format database data for display
const formatDatabaseResponse = (data, dataType) => {
    switch (dataType) {
        case 'products':
            if (data.length === 0) return { type: 'text', content: 'Không tìm thấy sản phẩm nào.' };
            
            let productHTML = '<div class="chatbot-products-container">';
            data.forEach((product) => {
                productHTML += createProductCardHTML(product);
            });
            productHTML += '</div>';
            return { type: 'html', content: productHTML };
            
        case 'categories':
            if (data.length === 0) return { type: 'text', content: 'Không có danh mục nào.' };
            
            let categoryText = 'Danh sách danh mục:\n';
            data.forEach((category, index) => {
                categoryText += `${index + 1}. ${category.title}\n`;
                if (category.description) {
                    categoryText += `   - Mô tả: ${category.description}\n`;
                }
                categoryText += '\n';
            });
            return { type: 'text', content: categoryText };
            
        case 'orders':
            if (data.length === 0) return { type: 'text', content: 'Không có đơn hàng nào.' };
            
            let orderText = 'Danh sách đơn hàng:\n';
            data.forEach((order, index) => {
                orderText += `${index + 1}. Đơn hàng ${order.code}\n`;
                orderText += `   - Khách hàng: ${order.customerName}\n`;
                orderText += `   - Số điện thoại: ${order.phone}\n`;
                orderText += `   - Tổng tiền: ${order.totalAmount ? order.totalAmount.toLocaleString('vi-VN') + ' VNĐ' : 'N/A'}\n`;
                orderText += `   - Trạng thái: ${order.statusName}\n`;
                orderText += `   - Ngày tạo: ${new Date(order.createdDate).toLocaleDateString('vi-VN')}\n`;
                orderText += '\n';
            });
            return { type: 'text', content: orderText };
            
        case 'blogs':
            if (data.length === 0) return { type: 'text', content: 'Không có bài blog nào.' };
            
            let blogText = 'Danh sách blog:\n';
            data.forEach((blog, index) => {
                blogText += `${index + 1}. ${blog.title}\n`;
                blogText += `   - Tác giả: ${blog.authorName}\n`;
                blogText += `   - Danh mục: ${blog.categoryName}\n`;
                blogText += `   - Ngày tạo: ${new Date(blog.createdDate).toLocaleDateString('vi-VN')}\n`;
                if (blog.description) {
                    blogText += `   - Mô tả: ${blog.description.substring(0, 100)}...\n`;
                }
                blogText += '\n';
            });
            return { type: 'text', content: blogText };
            
        case 'news':
            if (data.length === 0) return { type: 'text', content: 'Không có tin tức nào.' };
            
            let newsText = 'Danh sách tin tức:\n';
            data.forEach((news, index) => {
                newsText += `${index + 1}. ${news.title}\n`;
                newsText += `   - Danh mục: ${news.categoryName}\n`;
                newsText += `   - Ngày tạo: ${new Date(news.createdDate).toLocaleDateString('vi-VN')}\n`;
                if (news.description) {
                    newsText += `   - Mô tả: ${news.description.substring(0, 100)}...\n`;
                }
                newsText += '\n';
            });
            return { type: 'text', content: newsText };
            
        case 'statistics':
            return { 
                type: 'text', 
                content: `Thống kê hệ thống:
- Tổng số sản phẩm: ${data.totalProducts}
- Sản phẩm mới: ${data.newProducts}
- Sản phẩm bán chạy: ${data.bestSellerProducts}
- Tổng số đơn hàng: ${data.totalOrders}
- Tổng số khách hàng: ${data.totalCustomers}
- Tổng số blog: ${data.totalBlogs}
- Tổng số tin tức: ${data.totalNews}`
            };
            
        case 'search':
            // If search has products, render as HTML cards
            if (data.products && data.products.length > 0) {
                let productHTML = '<div class="chatbot-products-container">';
                if (data.blogs?.length || data.news?.length) {
                    productHTML += '<h5 style="margin-bottom: 10px; font-size: 14px; font-weight: 600;">SẢN PHẨM:</h5>';
                }
                data.products.forEach((product) => {
                    productHTML += createProductCardHTML(product);
                });
                productHTML += '</div>';
                
                // Add blogs and news as text if they exist
                let additionalText = '';
                if (data.blogs && data.blogs.length > 0) {
                    additionalText += '\n\nBLOG:\n';
                    data.blogs.forEach((blog, index) => {
                        additionalText += `${index + 1}. ${blog.title}\n`;
                    });
                }
                if (data.news && data.news.length > 0) {
                    additionalText += '\n\nTIN TỨC:\n';
                    data.news.forEach((news, index) => {
                        additionalText += `${index + 1}. ${news.title}\n`;
                    });
                }
                
                return { 
                    type: 'html', 
                    content: productHTML + (additionalText ? `<div style="margin-top: 15px; white-space: pre-line;">${additionalText}</div>` : '')
                };
            }
            
            // No products, return as text
            let searchText = 'Kết quả tìm kiếm:\n\n';
            
            if (data.blogs && data.blogs.length > 0) {
                searchText += 'BLOG:\n';
                data.blogs.forEach((blog, index) => {
                    searchText += `${index + 1}. ${blog.title}\n`;
                });
                searchText += '\n';
            }
            
            if (data.news && data.news.length > 0) {
                searchText += 'TIN TỨC:\n';
                data.news.forEach((news, index) => {
                    searchText += `${index + 1}. ${news.title}\n`;
                });
                searchText += '\n';
            }
            
            if (!data.products?.length && !data.blogs?.length && !data.news?.length) {
                searchText = 'Không tìm thấy kết quả nào phù hợp.';
            }
            
            return { type: 'text', content: searchText };
            
        default:
            return { type: 'text', content: JSON.stringify(data, null, 2) };
    }
};

// Create message element with dynamic classes and return it
const createMessageElement = (content, ...classes) => {
    const div = document.createElement("div");
    div.classList.add("message", ...classes);
    div.innerHTML = content;
    return div;
};

// Generate bot response using API
const generateBotResponse = async (incomingMessageDiv) => {
    const messageElement = incomingMessageDiv.querySelector(".message-text");

    try {
        // Check if user is asking about database data
        if (isDatabaseQuery(userData.message)) {
            let databaseResponse = '';
            let dataType = '';
            
            const lowerMessage = userData.message.toLowerCase();
            
            // Determine what data to fetch based on user query
            if (lowerMessage.includes('sản phẩm') || lowerMessage.includes('product')) {
                if (lowerMessage.includes('tìm kiếm') || lowerMessage.includes('search')) {
                    // Extract search term
                    const searchTerm = userData.message.replace(/.*?(?:tìm kiếm|search|tìm).*?([a-zA-ZÀ-ỹ0-9\s]+).*/i, '$1').trim();
                    if (searchTerm && searchTerm !== userData.message) {
                        const searchData = await searchDatabaseData(searchTerm);
                        databaseResponse = formatDatabaseResponse(searchData, 'search');
                    } else {
                        const products = await fetchDatabaseData('/products');
                        databaseResponse = formatDatabaseResponse(products, 'products');
                    }
                } else {
                    const products = await fetchDatabaseData('/products');
                    databaseResponse = formatDatabaseResponse(products, 'products');
                }
                dataType = 'products';
            } else if (lowerMessage.includes('danh mục') || lowerMessage.includes('category')) {
                const categories = await fetchDatabaseData('/categories');
                databaseResponse = formatDatabaseResponse(categories, 'categories');
                dataType = 'categories';
            } else if (lowerMessage.includes('đơn hàng') || lowerMessage.includes('order')) {
                const orders = await fetchDatabaseData('/orders');
                databaseResponse = formatDatabaseResponse(orders, 'orders');
                dataType = 'orders';
            } else if (lowerMessage.includes('blog')) {
                const blogs = await fetchDatabaseData('/blogs');
                databaseResponse = formatDatabaseResponse(blogs, 'blogs');
                dataType = 'blogs';
            } else if (lowerMessage.includes('tin tức') || lowerMessage.includes('news')) {
                const news = await fetchDatabaseData('/news');
                databaseResponse = formatDatabaseResponse(news, 'news');
                dataType = 'news';
            } else if (lowerMessage.includes('thống kê') || lowerMessage.includes('statistics') || 
                      lowerMessage.includes('có bao nhiêu') || lowerMessage.includes('số lượng')) {
                const statistics = await fetchDatabaseData('/statistics');
                databaseResponse = formatDatabaseResponse(statistics, 'statistics');
                dataType = 'statistics';
            } else {
                // General search
                const searchData = await searchDatabaseData(userData.message);
                databaseResponse = formatDatabaseResponse(searchData, 'search');
                dataType = 'search';
            }
            
            // Display database response
            if (typeof databaseResponse === 'object' && databaseResponse.type) {
                if (databaseResponse.type === 'html') {
                    messageElement.innerHTML = databaseResponse.content;
                } else {
                    messageElement.innerText = databaseResponse.content;
                }
            } else {
                // Fallback for old format
                messageElement.innerText = databaseResponse;
            }
            
            // Add to chat history
            chatHistory.push({
                role: "user",
                parts: [{ text: userData.message }],
            });
            
            // Store text version for chat history
            const historyText = typeof databaseResponse === 'object' && databaseResponse.type 
                ? (databaseResponse.type === 'html' 
                    ? 'Đã hiển thị danh sách sản phẩm' 
                    : databaseResponse.content)
                : databaseResponse;
            
            chatHistory.push({
                role: "model",
                parts: [{ text: historyText }]
            });
            
        } else {
            // Regular AI response using Gemini API
            chatHistory.push({
                role: "user",
                parts: [{ text: userData.message }, ...(userData.file.data ? [{ inline_data: userData.file }] : [])],
            });

            // API request options
            const requestOptions = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    contents: chatHistory
                })
            }

            // Fetch bot response from API
            const response = await fetch(API_URL, requestOptions);
            const data = await response.json();
            if (!response.ok) throw new Error(data.error.message);

            // Extract and display bot's response text
            const apiResponseText = data.candidates[0].content.parts[0].text.replace(/\*\*(.*?)\*\*/g, "$1").trim();
            messageElement.innerText = apiResponseText;
            chatHistory.push({
                role: "model",
                parts: [{ text: apiResponseText }]
            });
        }
        
    } catch (error) {
        console.error('Error generating response:', error);
        messageElement.innerText = `Xin lỗi, đã có lỗi xảy ra: ${error.message}`;
        messageElement.style.color = "#ff0000";
    } finally {
        userData.file = {};
        incomingMessageDiv.classList.remove("thinking");
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
    }
};

// Load chat history from ChatController
async function loadChatHistory() {
    try {
        const response = await fetch(`${CHAT_API_BASE}/messages`);
        if (!response.ok) return;
        
        const messages = await response.json();
        if (!messages || messages.length === 0) return;
        
        // Clear existing messages except welcome
        const welcomeMsg = chatBody.querySelector('.message.bot-message');
        chatBody.innerHTML = '';
        if (welcomeMsg) {
            chatBody.appendChild(welcomeMsg);
        }
        
        messages.forEach(msg => {
            const messageDiv = createMessageElement(
                `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">${escapeHtml(msg.message || '')}</div>`,
                msg.sender === 'user' ? 'user-message' : 'bot-message'
            );
            chatBody.appendChild(messageDiv);
        });
        
        chatBody.scrollTop = chatBody.scrollHeight;
    } catch (error) {
        console.error('Error loading chat history:', error);
    }
}

// Helper function to escape HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
}

// Send message to ChatController
async function sendMessageToChat(message) {
    try {
        showTyping();
        
        const response = await fetch(`${CHAT_API_BASE}/send`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ message: message })
        });
        
        if (!response.ok) {
            let errorMessage = 'Failed to send message';
            try {
                const errorData = await response.json();
                errorMessage = errorData.error || errorMessage;
            } catch {
                errorMessage = `Server error: ${response.status} ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
        const data = await response.json();
        hideTyping();
        
        // Show bot message
        if (data.bot) {
            const botMsg = createMessageElement(
                `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">${escapeHtml(data.bot.message || '')}</div>`,
                'bot-message'
            );
            chatBody.appendChild(botMsg);
            
            // Show categories if available
            if (data.categories && data.categories.length > 0) {
                appendCategories(data.categories);
            }
            
            // Show products if available
            if (data.products && data.products.length > 0) {
                appendProductCards(data.products);
            }
            
            // Show product list if category matched
            if (data.show_category_button && data.product_list && data.product_list.length > 0) {
                appendProductList(data.product_list, data.category_matched || 'Danh mục', data.top_products || []);
            }
            
            // Show order details if tracking
            if (data.order) {
                appendOrderDetails(data.order);
            }
        }
        
        chatBody.scrollTop = chatBody.scrollHeight;
    } catch (error) {
        hideTyping();
        console.error('Error sending message:', error);
        const errorDetail = error.message || 'Không xác định được lỗi';
        const errorMsg = createMessageElement(
            `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
            </svg>
            <div class="message-text">Lỗi: không gửi được tin nhắn. ${escapeHtml(errorDetail)}</div>`,
            'bot-message'
        );
        chatBody.appendChild(errorMsg);
        chatBody.scrollTop = chatBody.scrollHeight;
    }
}

// Append categories
function appendCategories(categories) {
    const items = categories.map(cat => `
        <li class="category-item" data-category="${cat.name}">
            <div class="category-name">
                <i class="fas fa-folder"></i>
                ${escapeHtml(cat.name)}
            </div>
            <span class="category-count">${cat.product_count} SP</span>
        </li>
    `).join('');
    
    const html = `
        <div class="categories-container">
            <div class="categories-title">📂 Danh mục sản phẩm</div>
            <ul class="categories-list">${items}</ul>
        </div>
    `;
    
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
}

// Append product cards
function appendProductCards(products) {
    const cards = products.map(product => {
        const imageUrl = product.image_url || '/assets/img/default-product.png';
        const price = product.price_formatted || formatProductPrice(product.price);
        
        return `
            <div class="chatbot-product-card">
                <div class="product-card-image">
                    <img src="${imageUrl}" alt="${product.name}" onerror="this.src='/assets/img/default-product.png'">
                </div>
                <div class="product-card-content">
                    <h4 class="product-card-title">${escapeHtml(product.name)}</h4>
                    <p class="product-card-description">${escapeHtml(product.description || 'Sản phẩm chất lượng cao')}</p>
                    <div class="product-card-price">
                        <span class="current-price">${escapeHtml(price)}</span>
                    </div>
                    <a href="${product.detail_url || `/product/${product.slug || product.id}.html`}" class="product-card-button" target="_blank">Xem chi tiết</a>
                </div>
            </div>
        `;
    }).join('');
    
    const html = `<div class="chatbot-products-container">${cards}</div>`;
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
}

function formatProductPrice(price) {
    if (!price) return 'Liên hệ';
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// Append product list
function appendProductList(products, categoryName, topProducts) {
    if (!products || products.length === 0) return;
    
    const items = products.map((product, index) => `
        <li class="product-list-item clickable-product" data-product-id="${product.id}">
            <span class="product-number">${index + 1}</span>
            <span class="product-list-name">${escapeHtml(product.name)}</span>
        </li>
    `).join('');
    
    const html = `
        <div class="product-list-container">
            <div class="product-list-header">
                <i class="fas fa-list"></i> Danh sách sản phẩm ${escapeHtml(categoryName)}
            </div>
            <ol class="product-list">${items}</ol>
            <div class="product-list-footer">
                <button class="btn-show-category-products" data-products='${JSON.stringify(topProducts).replace(/'/g, "&#39;")}'>
                    <i class="fas fa-star"></i> Xem 3 sản phẩm nổi bật
                </button>
            </div>
        </div>
    `;
    
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
}

// Append order details
function appendOrderDetails(order) {
    const statusColors = {
        'pending': '#ffc107',
        'processing': '#17a2b8',
        'delivered': '#28a745',
        'completed': '#28a745',
        'canceled': '#dc3545'
    };
    
    const statusColor = statusColors[order.status?.toLowerCase()] || '#6c757d';
    
    let itemsHtml = order.items.map((item, index) => `
        <tr>
            <td style="padding: 8px; border-bottom: 1px solid #f1f5f9;">${index + 1}. ${escapeHtml(item.product_name)}</td>
            <td style="padding: 8px; border-bottom: 1px solid #f1f5f9; text-align: center;">x${item.quantity}</td>
            <td style="padding: 8px; border-bottom: 1px solid #f1f5f9; text-align: right;">${escapeHtml(item.total)}</td>
        </tr>
    `).join('');
    
    const html = `
        <div class="order-details-card">
            <div class="order-card-header">
                <div class="order-card-title">
                    <i class="fas fa-receipt"></i> Đơn hàng #${order.id}
                </div>
                <div class="order-status-badge" style="background-color: ${statusColor};">
                    ${order.status_emoji} ${escapeHtml(order.status_label)}
                </div>
            </div>
            <div class="order-card-body">
                <div class="order-info-row">
                    <span class="info-label">Ngày đặt:</span>
                    <span class="info-value">${escapeHtml(order.created_at)}</span>
                </div>
                ${order.shipping_address ? `
                <div class="order-section">
                    <h4 style="margin: 0 0 8px 0; font-size: 13px; color: #374151;">📍 Địa chỉ giao hàng</h4>
                    <p style="margin: 0; font-size: 12px; color: #6b7280; line-height: 1.5;">
                        ${escapeHtml(order.shipping_address.full_name)} - ${escapeHtml(order.shipping_address.phone)}<br>
                        ${escapeHtml(order.shipping_address.address)}
                    </p>
                </div>
                ` : ''}
                <div class="order-section">
                    <h4 style="margin: 0 0 8px 0; font-size: 13px; color: #374151;">🛍️ Sản phẩm</h4>
                    <table style="width: 100%; font-size: 12px; border-collapse: collapse;">
                        <tbody>
                            ${itemsHtml}
                            <tr>
                                <td colspan="2" style="padding: 8px; text-align: right; font-weight: 600;">Tạm tính:</td>
                                <td style="padding: 8px; text-align: right;">${escapeHtml(order.subtotal)}</td>
                            </tr>
                            <tr>
                                <td colspan="2" style="padding: 8px; text-align: right; font-weight: 600;">Phí vận chuyển:</td>
                                <td style="padding: 8px; text-align: right;">${escapeHtml(order.shipping_fee)}</td>
                            </tr>
                            <tr style="background-color: #fef3c7;">
                                <td colspan="2" style="padding: 10px; text-align: right; font-weight: 700; font-size: 14px;">Tổng cộng:</td>
                                <td style="padding: 10px; text-align: right; font-weight: 700; font-size: 14px; color: #dc2626;">${escapeHtml(order.total_price)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="text-align: center; margin-top: 15px;">
                    <a href="${order.detail_url}" target="_blank" class="product-card-button">
                        <i class="fas fa-external-link-alt"></i> Xem chi tiết đầy đủ
                    </a>
                </div>
            </div>
        </div>
    `;
    
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
}

// Show typing indicator
function showTyping() {
    if (chatBody.querySelector('.typing-row')) return;
    const typingDiv = createMessageElement(
        `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
            <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
        </svg>
        <div class="message-text">
            <div class="thinking-indicator">
                <div class="dot"></div>
                <div class="dot"></div>
                <div class="dot"></div>
            </div>
        </div>`,
        'bot-message', 'typing-row'
    );
    chatBody.appendChild(typingDiv);
    chatBody.scrollTop = chatBody.scrollHeight;
}

function hideTyping() {
    const typingRow = chatBody.querySelector('.typing-row');
    if (typingRow) typingRow.remove();
}

// Track order
async function trackOrder(orderId) {
    try {
        showTyping();
        
        const response = await fetch(`${CHAT_API_BASE}/track-order`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ orderId: orderId })
        });
        
        if (!response.ok) throw new Error('Failed to track order');
        
        const data = await response.json();
        hideTyping();
        
        if (data.bot) {
            const botMsg = createMessageElement(
                `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">${escapeHtml(data.bot.message || '')}</div>`,
                'bot-message'
            );
            chatBody.appendChild(botMsg);
        }
        
        if (data.order) {
            appendOrderDetails(data.order);
        }
        
        chatBody.scrollTop = chatBody.scrollHeight;
    } catch (error) {
        hideTyping();
        console.error('Error tracking order:', error);
    }
}

// Show order tracking form
function showOrderTrackingForm() {
    const html = `
        <div class="order-tracking-form">
            <div class="tracking-form-header">
                <i class="fas fa-search"></i> Tra cứu đơn hàng
            </div>
            <div class="tracking-form-body">
                <p style="margin: 0 0 15px 0; color: #6b7280; font-size: 13px;">
                    Nhập mã đơn hàng để xem thông tin và trạng thái đơn hàng của bạn.
                </p>
                <div class="form-group">
                    <label>Mã đơn hàng <span class="required">*</span></label>
                    <input type="text" id="tracking-order-id" class="form-control" placeholder="Nhập mã đơn hàng (VD: #123 hoặc 123)">
                </div>
                <div class="tracking-form-actions">
                    <button class="btn-tracking-submit product-card-button">
                        <i class="fas fa-search"></i> Tra cứu
                    </button>
                    <button class="btn-tracking-cancel" style="background: #dc3545; margin-left: 10px; padding: 6px 12px; border-radius: 4px; color: white; border: none; cursor: pointer;">
                        <i class="fas fa-times"></i> Hủy
                    </button>
                </div>
            </div>
        </div>
    `;
    
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
    document.getElementById('tracking-order-id')?.focus();
}

// Show perfume advisor form
function showPerfumeAdvisorForm() {
    const html = `
        <div class="perfume-advisor-form">
            <div class="advisor-form-header">
                <i class="fas fa-magic"></i> Tư vấn sản phẩm phù hợp
            </div>
            <div class="advisor-form-body">
                <div class="form-group">
                    <label>Giới tính</label>
                    <select id="advisor-gender" class="form-control">
                        <option value="">-- Chọn giới tính --</option>
                        <option value="nam">Nam</option>
                        <option value="nữ">Nữ</option>
                        <option value="unisex">Unisex</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label>Phong cách</label>
                    <select id="advisor-style" class="form-control">
                        <option value="">-- Chọn phong cách --</option>
                        <option value="ngọt">Ngọt ngào</option>
                        <option value="quyến rũ">Quyến rũ</option>
                        <option value="năng động">Năng động</option>
                        <option value="thanh lịch">Thanh lịch</option>
                        <option value="tươi mát">Tươi mát</option>
                        <option value="gợi cảm">Gợi cảm</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label>Nốt hương yêu thích (tùy chọn)</label>
                    <input type="text" id="advisor-note" class="form-control" placeholder="VD: Hoa hồng, Hương gỗ, Cam bergamot, xạ hương...">
                </div>
                
                <div class="form-group">
                    <label>Mức giá</label>
                    <select id="advisor-price" class="form-control">
                        <option value="">-- Tất cả --</option>
                        <option value="0-500000">Dưới 500.000 ₫</option>
                        <option value="500000-1000000">500.000 - 1.000.000 ₫</option>
                        <option value="1000000-2000000">1.000.000 - 2.000.000 ₫</option>
                        <option value="2000000-5000000">2.000.000 - 5.000.000 ₫</option>
                        <option value="5000000-999999999">Trên 5.000.000 ₫</option>
                    </select>
                </div>
                
                <div class="advisor-form-actions">
                    <button class="btn-advisor-submit product-card-button">
                        <i class="fas fa-search"></i> Tìm sản phẩm phù hợp
                    </button>
                    <button class="btn-advisor-cancel" style="background: #dc3545; margin-left: 10px; padding: 6px 12px; border-radius: 4px; color: white; border: none; cursor: pointer;">
                        <i class="fas fa-times"></i> Hủy
                    </button>
                </div>
            </div>
        </div>
    `;
    
    chatBody.insertAdjacentHTML('beforeend', html);
    chatBody.scrollTop = chatBody.scrollHeight;
}

// Perfume Advisor submit
async function submitPerfumeAdvisor() {
    const gender = document.getElementById('advisor-gender')?.value || '';
    const style = document.getElementById('advisor-style')?.value || '';
    const note = document.getElementById('advisor-note')?.value || '';
    const priceRange = document.getElementById('advisor-price')?.value || '';
    
    if (!gender && !style) {
        alert('Vui lòng chọn ít nhất giới tính hoặc phong cách!');
        return;
    }
    
    // Hide form and show loading
    document.querySelector('.perfume-advisor-form')?.remove();
    showTyping();
    
    try {
        const response = await fetch(`${CHAT_API_BASE}/perfume-advisor`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                gender: gender,
                style: style,
                note: note,
                price_range: priceRange
            })
        });
        
        if (!response.ok) throw new Error('Failed to get advisor');
        
        const data = await response.json();
        hideTyping();
        
        if (data.bot) {
            const botMsg = createMessageElement(
                `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">${escapeHtml(data.bot.message || '')}</div>`,
                'bot-message'
            );
            chatBody.appendChild(botMsg);
        }
        
        if (data.products && data.products.length > 0) {
            appendProductCards(data.products);
        } else {
            const noProductsMsg = createMessageElement(
                `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">Xin lỗi, hiện tại chúng tôi chưa có sản phẩm phù hợp với tiêu chí của bạn. Bạn có thể thử điều chỉnh lại tiêu chí hoặc liên hệ trực tiếp với chúng tôi để được tư vấn chi tiết hơn.</div>`,
                'bot-message'
            );
            chatBody.appendChild(noProductsMsg);
        }
        
        chatBody.scrollTop = chatBody.scrollHeight;
    } catch (error) {
        hideTyping();
        console.error('Error getting advisor:', error);
        const errorMsg = createMessageElement(
            `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
            </svg>
            <div class="message-text">Xin lỗi, có lỗi xảy ra. Vui lòng thử lại sau.</div>`,
            'bot-message'
        );
        chatBody.appendChild(errorMsg);
        chatBody.scrollTop = chatBody.scrollHeight;
    }
}

// Handle outgoing user message
const handleOutgoingMessage = (e) => {
    e.preventDefault();
    userData.message = messageInput.value.trim();
    if (!userData.message) return;
    
    messageInput.value = "";
    fileUploadWrapper.classList.remove("file-uploaded");
    messageInput.dispatchEvent(new Event("input"));

    // Create and display user message
    const messageContent = `<div class="message-text"></div>
                            ${userData.file.data ? `<img src="data:${userData.file.mime_type};base64,${userData.file.data}" class="attachment" />` : ""}`;

    const outgoingMessageDiv = createMessageElement(messageContent, "user-message");
    outgoingMessageDiv.querySelector(".message-text").innerText = userData.message;
    chatBody.appendChild(outgoingMessageDiv);
    chatBody.scrollTop = chatBody.scrollHeight;

    // Send to ChatController instead of old logic
    sendMessageToChat(userData.message);
    
    userData.file = {};
};

// Handle Enter key press for sending messages
messageInput.addEventListener("keydown", (e) => {
    const userMessage = e.target.value.trim();
    if (e.key === "Enter" && userMessage && !e.shiftKey && window.innerWidth > 768) {
        handleOutgoingMessage(e);
    }
});

messageInput.addEventListener("input", (e) => {
    messageInput.style.height = `${initialInputHeight}px`;
    messageInput.style.height = `${messageInput.scrollHeight}px`;
    document.querySelector(".chat-form").style.boderRadius = messageInput.scrollHeight > initialInputHeight ? "15px" : "32px";
});

// Handle file input change event
fileInput.addEventListener("change", (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e) => {
        fileUploadWrapper.querySelector("img").src = e.target.result;
        fileUploadWrapper.classList.add("file-uploaded");
        const base64String = e.target.result.split(",")[1];

        // Store file data in userData
        userData.file = {
            data: base64String,
            mime_type: file.type
        };

        fileInput.value = "";
    };

    reader.readAsDataURL(file);
});

fileCancelButton.addEventListener("click", (e) => {
    userData.file = {};
    fileUploadWrapper.classList.remove("file-uploaded");
});

const picker = new EmojiMart.Picker({
    theme: "light",
    showSkinTones: "none",
    previewPosition: "none",
    onEmojiSelect: (emoji) => {
        const { selectionStart: start, selectionEnd: end } = messageInput;
        messageInput.setRangeText(emoji.native, start, end, "end");
        messageInput.focus();
    },
    onClickOutside: (e) => {
        if (e.target.id === "emoji-picker") {
            document.body.classList.toggle("show-emoji-picker");
        } else {
            document.body.classList.remove("show-emoji-picker");
        }
    },
});

document.querySelector(".chat-form").appendChild(picker);

fileInput.addEventListener("change", async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const validImageTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!validImageTypes.includes(file.type)) {
        await Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WEBP)',
            confirmButtonText: 'OK'
        });
        resetFileInput();
        return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
        fileUploadWrapper.querySelector("img").src = e.target.result;
        fileUploadWrapper.classList.add("file-uploaded");
        const base64String = e.target.result.split(",")[1];
        userData.file = {
            data: base64String,
            mime_type: file.type
        };
    };
    reader.readAsDataURL(file);
});

function resetFileInput() {
    fileInput.value = "";
    fileUploadWrapper.classList.remove("file-uploaded");
    fileUploadWrapper.querySelector("img").src = "#";
    userData.file = { data: null, mime_type: null };
    document.querySelector(".chat-form").reset();
}

sendMessageButton.addEventListener("click", (e) => handleOutgoingMessage(e));
document.querySelector("#file-upload")?.addEventListener("click", (e) => fileInput.click());

// Chatbot toggler - load history when opened
chatbotToggler?.addEventListener("click", () => {
    const isOpen = document.body.classList.toggle("show-chatbot");
    if (isOpen) {
        loadChatHistory();
    }
});

closeChatbot?.addEventListener("click", () => {
    document.body.classList.remove("show-chatbot");
});

// Quick replies toggle
document.addEventListener('click', (e) => {
    if (e.target.closest('#quick-replies-toggle-btn')) {
        const btn = e.target.closest('#quick-replies-toggle-btn');
        const dropdown = document.getElementById('quick-replies-dropdown');
        if (dropdown) {
            dropdown.classList.toggle('show');
            btn.classList.toggle('active');
        }
    }
    
    // Quick reply item click
    if (e.target.closest('.quick-reply-item')) {
        const item = e.target.closest('.quick-reply-item');
        const action = item.dataset.action;
        const text = item.dataset.text;
        
        // Close dropdown
        const dropdown = document.getElementById('quick-replies-dropdown');
        const btn = document.getElementById('quick-replies-toggle-btn');
        if (dropdown) dropdown.classList.remove('show');
        if (btn) btn.classList.remove('active');
        
        if (action === 'track-order') {
            showOrderTrackingForm();
        } else if (action === 'advisor') {
            showPerfumeAdvisorForm();
        } else if (text && messageInput) {
            messageInput.value = text;
            handleOutgoingMessage(e);
        }
    }
    
    // Category item click
    if (e.target.closest('.category-item')) {
        const category = e.target.closest('.category-item').dataset.category;
        if (messageInput && category) {
            messageInput.value = category;
            handleOutgoingMessage(e);
        }
    }
    
    // Show category products button
    if (e.target.closest('.btn-show-category-products')) {
        const btn = e.target.closest('.btn-show-category-products');
        const productsJson = btn.dataset.products;
        if (productsJson) {
            try {
                const products = JSON.parse(productsJson.replace(/&#39;/g, "'"));
                // Hide product list
                btn.closest('.product-list-container')?.remove();
                // Show bot message
                const botMsg = createMessageElement(
                    `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                        <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                    </svg>
                    <div class="message-text">Đây là 3 sản phẩm bán chạy nhất mà chúng tôi gợi ý cho bạn:</div>`,
                    'bot-message'
                );
                chatBody.appendChild(botMsg);
                // Show product cards
                appendProductCards(products);
            } catch (err) {
                console.error('Error parsing products:', err);
            }
        }
    }
    
    // Tracking form submit
    if (e.target.closest('.btn-tracking-submit')) {
        const orderIdInput = document.getElementById('tracking-order-id');
        if (orderIdInput) {
            const orderId = orderIdInput.value.trim();
            if (!orderId) {
                alert('Vui lòng nhập mã đơn hàng!');
                return;
            }
            document.querySelector('.order-tracking-form')?.remove();
            trackOrder(orderId);
        }
    }
    
    // Tracking form cancel
    if (e.target.closest('.btn-tracking-cancel')) {
        document.querySelector('.order-tracking-form')?.remove();
    }
    
    // Advisor form submit
    if (e.target.closest('.btn-advisor-submit')) {
        submitPerfumeAdvisor();
    }
    
    // Advisor form cancel
    if (e.target.closest('.btn-advisor-cancel')) {
        document.querySelector('.perfume-advisor-form')?.remove();
    }
});

// Test API connection when page loads
document.addEventListener('DOMContentLoaded', () => {
    testApiConnection();
});