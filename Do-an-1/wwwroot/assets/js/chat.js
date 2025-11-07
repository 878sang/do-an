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

// Function to format database data for display
const formatDatabaseResponse = (data, dataType) => {
    switch (dataType) {
        case 'products':
            if (data.length === 0) return 'Không tìm thấy sản phẩm nào.';
            
            let productText = 'Danh sách sản phẩm:\n';
            data.forEach((product, index) => {
                productText += `${index + 1}. ${product.title}\n`;
                productText += `   - Giá: ${product.price ? product.price.toLocaleString('vi-VN') + ' VNĐ' : 'Liên hệ'}\n`;
                if (product.priceSale && product.priceSale !== product.price) {
                    productText += `   - Giá khuyến mãi: ${product.priceSale.toLocaleString('vi-VN')} VNĐ\n`;
                }
                productText += `   - Danh mục: ${product.categoryName}\n`;
                if (product.description) {
                    productText += `   - Mô tả: ${product.description.substring(0, 100)}...\n`;
                }
                productText += '\n';
            });
            return productText;
            
        case 'categories':
            if (data.length === 0) return 'Không có danh mục nào.';
            
            let categoryText = 'Danh sách danh mục:\n';
            data.forEach((category, index) => {
                categoryText += `${index + 1}. ${category.title}\n`;
                if (category.description) {
                    categoryText += `   - Mô tả: ${category.description}\n`;
                }
                categoryText += '\n';
            });
            return categoryText;
            
        case 'orders':
            if (data.length === 0) return 'Không có đơn hàng nào.';
            
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
            return orderText;
            
        case 'blogs':
            if (data.length === 0) return 'Không có bài blog nào.';
            
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
            return blogText;
            
        case 'news':
            if (data.length === 0) return 'Không có tin tức nào.';
            
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
            return newsText;
            
        case 'statistics':
            return `Thống kê hệ thống:
- Tổng số sản phẩm: ${data.totalProducts}
- Sản phẩm mới: ${data.newProducts}
- Sản phẩm bán chạy: ${data.bestSellerProducts}
- Tổng số đơn hàng: ${data.totalOrders}
- Tổng số khách hàng: ${data.totalCustomers}
- Tổng số blog: ${data.totalBlogs}
- Tổng số tin tức: ${data.totalNews}`;
            
        case 'search':
            let searchText = 'Kết quả tìm kiếm:\n\n';
            
            if (data.products && data.products.length > 0) {
                searchText += 'SẢN PHẨM:\n';
                data.products.forEach((product, index) => {
                    searchText += `${index + 1}. ${product.title} - ${product.price ? product.price.toLocaleString('vi-VN') + ' VNĐ' : 'Liên hệ'}\n`;
                });
                searchText += '\n';
            }
            
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
            
            return searchText;
            
        default:
            return JSON.stringify(data, null, 2);
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
            messageElement.innerText = databaseResponse;
            
            // Add to chat history
            chatHistory.push({
                role: "user",
                parts: [{ text: userData.message }],
            });
            
            chatHistory.push({
                role: "model",
                parts: [{ text: databaseResponse }]
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

// Handle outgoing user message
const handleOutgoingMessage = (e) => {
    e.preventDefault();
    userData.message = messageInput.value.trim();
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

    // Simulate bot response with thinking indicator after a delay
    setTimeout(() => {
        const messageContent = `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">
                    <div class="thinking-indicator">
                        <div class="dot"></div>
                        <div class="dot"></div>
                        <div class="dot"></div>
                    </div>
                </div>`;

        const incomingMessageDiv = createMessageElement(messageContent, "bot-message", "thinking");
        chatBody.appendChild(incomingMessageDiv);
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
        generateBotResponse(incomingMessageDiv);
    }, 600);
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
document.querySelector("#file-upload").addEventListener("click", (e) => fileInput.click());
chatbotToggler.addEventListener("click", () => document.body.classList.toggle("show-chatbot"));
closeChatbot.addEventListener("click", () => document.body.classList.remove("show-chatbot"));

// Test API connection when page loads
document.addEventListener('DOMContentLoaded', () => {
    testApiConnection();
});