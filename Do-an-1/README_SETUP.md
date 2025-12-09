# Hướng dẫn Setup Chatbot với Gemini AI

## Bảo mật API Key

⚠️ **QUAN TRỌNG**: API key đã được di chuyển ra khỏi file JavaScript và lưu trong `appsettings.json` để bảo mật.

## Các bước setup:

### 1. Tạo file appsettings.json

File `appsettings.json` đã được thêm vào `.gitignore` để không commit lên GitHub.

**Nếu bạn clone project mới**, hãy:

1. Copy file `appsettings.Example.json` thành `appsettings.json`:
   ```bash
   cp appsettings.Example.json appsettings.json
   ```

2. Mở `appsettings.json` và điền thông tin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
  },
  "GeminiAI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "ModelName": "gemini-1.5-flash"
  },
  ...
}
```

### 2. Lấy Gemini API Key

1. Truy cập: https://aistudio.google.com/apikey
2. Tạo API key mới hoặc sử dụng key hiện có
3. Copy API key vào `appsettings.json`:
   ```json
   "GeminiAI": {
     "ApiKey": "AIzaSy...",  // Paste API key của bạn vào đây
     "ModelName": "gemini-1.5-flash"
   }
   ```

### 3. Chọn Model

Các model khả dụng:
- `gemini-1.5-flash` - **Khuyến nghị**: Free tier, nhanh, phù hợp cho chatbot
- `gemini-1.5-pro` - Free tier, mạnh hơn nhưng chậm hơn
- `gemini-2.0-flash` - Có thể không có trong free tier
- `gemini-3-pro-preview` - Preview, có thể không có trong free tier

Cập nhật `ModelName` trong `appsettings.json`:
```json
"GeminiAI": {
  "ApiKey": "YOUR_API_KEY",
  "ModelName": "gemini-1.5-flash"  // Đổi model ở đây
}
```

### 4. Kiểm tra Quota

Nếu gặp lỗi quota:
- Kiểm tra tại: https://ai.dev/usage?tab=rate-limit
- Đợi quota reset (thường reset theo ngày/tháng)
- Hoặc nâng cấp plan tại Google Cloud Console

## Cấu trúc Code

### Frontend (`chat.js`)
- **KHÔNG** chứa API key
- Gọi endpoint backend: `/api/Chat/send`
- Xử lý hiển thị và UI

### Backend (`ChatController.cs`)
- Đọc API key từ `appsettings.json`
- Gọi Gemini API từ server-side
- Xử lý logic và lưu lịch sử chat

## Lưu ý khi commit lên GitHub

✅ **ĐƯỢC commit:**
- `appsettings.Example.json` (template)
- `chat.js` (không có API key)
- `ChatController.cs`

❌ **KHÔNG commit:**
- `appsettings.json` (đã có trong `.gitignore`)
- Bất kỳ file nào chứa API key thật

## Troubleshooting

### Lỗi: "GeminiAI:ApiKey không được cấu hình"
- Kiểm tra `appsettings.json` có tồn tại không
- Kiểm tra key `GeminiAI:ApiKey` có giá trị không

### Lỗi: "Quota exceeded"
- Kiểm tra quota tại https://ai.dev/usage
- Đổi sang model khác (ví dụ: `gemini-1.5-flash`)
- Đợi quota reset

### Chatbot không hoạt động
- Kiểm tra Console (F12) để xem lỗi
- Kiểm tra endpoint `/api/Chat/send` có hoạt động không
- Kiểm tra API key có đúng không

