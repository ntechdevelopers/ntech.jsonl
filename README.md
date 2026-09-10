# Ntech.JsonL - Streaming JSON Lines (JSONL/NDJSON) in .NET

Dự án mẫu C# Console Application triển khai và minh họa cơ chế **JSON Lines (JSONL / NDJSON)** trong .NET (.NET 9 / .NET 10), dựa trên bài viết "Streaming JSON Lines (JSONL/NDJSON) in .NET: Architecture, Use Cases, and Implementation" của tác giả Jinal Shah.

---

## 1. Khái niệm JSON Lines (NDJSON)

Thông thường, REST API gửi danh sách dữ liệu dưới dạng một mảng JSON:

```json
[
  { "id": 1, "name": "John" },
  { "id": 2, "name": "Jane" }
]
```

Phương pháp mảng JSON truyền thống yêu cầu phía máy chủ truy vấn xong toàn bộ dữ liệu, nạp vào bộ nhớ RAM và hoàn tất đóng gói chuỗi JSON trước khi truyền tải xuống client.

**JSON Lines (NDJSON - Newline Delimited JSON):**
Mỗi đối tượng JSON nằm trên một dòng riêng biệt, phân tách bằng ký tự xuống dòng `\n`:

```ndjson
{"id":1,"name":"John"}
{"id":2,"name":"Jane"}
{"id":3,"name":"Bob"}
```

**Ưu điểm chính:**
- Phản hồi dữ liệu dạng stream ngay khi có sẵn từng bản ghi (giảm thời gian chờ TTFB).
- Tối ưu bộ nhớ RAM phía server, không cần nạp toàn bộ danh sách bản ghi vào bộ nhớ.
- Phía client có thể parse và xử lý ngay từng dòng dữ liệu mà không cần đợi tải toàn bộ file.

---

## 2. Cấu trúc dự án

```text
Ntech.JsonL/
├── Models/
│   ├── AiStreamChunk.cs        # Model dữ liệu cho stream LLM/AI (Use Case 1)
│   ├── CustomerRecord.cs       # Model dữ liệu xuất báo cáo (Use Case 2)
│   ├── DomainEvent.cs          # Model dữ liệu event bus (Use Case 3)
│   └── PersonDto.cs            # Model DTO cho HTTP API stream
├── Helpers/
│   └── NdjsonStreamHelper.cs   # Thư viện hỗ trợ đọc và ghi stream NDJSON
├── Demos/
│   ├── AiLlmStreamingDemo.cs   # Demo Stream dữ liệu AI/LLM theo token
│   ├── LargeDataExportDemo.cs  # Demo xuất dữ liệu dung lượng lớn & đo dung lượng RAM
│   ├── EventStreamingDemo.cs   # Demo xử lý event thời gian thực
│   └── AspNetCoreEndpointDemo.cs # Demo tích hợp ASP.NET Core API & HttpClient Stream
├── Program.cs                  # Menu điều khiển chính
├── Ntech.JsonL.csproj
└── README.md
```

---

## 3. Nội dung các bài test demo

### Demo 1: AI & LLM Streaming Responses
Mô phỏng cơ chế phản hồi theo từng token của các mô hình ngôn ngữ lớn (LLM). Dữ liệu được ghi ra stream theo định dạng `{"type":"content","text":"..."}` và kết thúc bằng chunk `{"type":"done"}`.

### Demo 2: Large Data Export APIs
Mô phỏng quá trình xuất 50,000 bản ghi dữ liệu khách hàng. So sánh trực tiếp dung lượng RAM tiêu thụ và thời gian phản hồi bản ghi đầu tiên giữa mảng JSON truyền thống và NDJSON stream.

### Demo 3: Real-Time Event Streaming System
Mô phỏng luồng event nhận từ Kafka/RabbitMQ/IoT. Hệ thống ghi nhận event và stream ra chuỗi NDJSON tức thời để phía consumer tiêu thụ.

### Demo 4: ASP.NET Core Endpoint & HttpClient Stream Reader
Khởi tạo endpoint GET `/people-stream` trên Web API Kestrel (`http://localhost:5055`) với header `Content-Type: application/x-ndjson`. Sử dụng `HttpClient` kết hợp `HttpCompletionOption.ResponseHeadersRead` và `StreamReader` để đọc stream.

---

## 4. Hướng dẫn khởi chạy

### Yêu cầu môi trường:
- .NET 9.0 SDK hoặc .NET 10.0 SDK.

### Lệnh thực thi:
```bash
cd e:\Working\Ntechdevelopers\NtechPOC\Ntech.JsonL
dotnet run
```

Hoặc chạy trực tiếp tất cả các bài test qua cờ lệnh:
```bash
dotnet run -- --all
```

---

## 5. Kỹ thuật áp dụng khi dùng NDJSON trong .NET

1. Header HTTP response thiết lập đúng chuẩn `Content-Type: application/x-ndjson`.
2. Thực hiện `await response.Body.FlushAsync()` sau mỗi dòng dữ liệu để đẩy dữ liệu xuống kết nối socket lập tức.
3. Tận dụng `IAsyncEnumerable<T>` để xử lý stream bất đồng bộ, tránh đệm toàn bộ danh sách vào bộ nhớ.
4. Đảm bảo cấu hình `JsonSerializerOptions.WriteIndented = false` vì quy chuẩn NDJSON không cho phép ký tự xuống dòng bên trong một JSON object.
5. Khi dùng `HttpClient` để đọc stream NDJSON, luôn thiết lập cờ `HttpCompletionOption.ResponseHeadersRead`.
