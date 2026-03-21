# API 设计

## 设备

### 注册
POST /api/device/register
{
  "deviceCode": "xxx",
  "deviceName": "客厅音箱"
}

### 心跳
POST /api/device/heartbeat

---

## 对话

### 音频对话
POST /api/chat/audio

Request:
- deviceCode
- sessionId
- file (wav)

Response:
{
  "sessionId": "xxx",
  "userText": "今天天气",
  "assistantText": "今天晴",
  "audioUrl": "https://..."
}

---

### 文本对话（调试）
POST /api/chat/text

---

## 配置
GET /api/device/config