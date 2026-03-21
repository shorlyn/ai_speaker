# 服务端设计（.NET）

## 分层

### Controller
ChatController
DeviceController

### Service
ChatService
DeviceService

### Provider
IAsrProvider
ILlmProvider
ITtsProvider

### Infrastructure
HttpClient封装
日志
配置

---

## Chat流程
1. 接收音频
2. 调ASR
3. 调LLM
4. 调TTS
5. 返回结果

---

## 技术选型
- .NET 9 Minimal API / WebAPI
- SQLite / MySQL
- Serilog