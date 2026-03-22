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
  "assistantText": "今天晴"
}

---

### 文本对话（调试）
POST /api/chat/text

---

## 语音合成

### 获取播报音频
POST /api/tts/speak

Request Body:
{
  "text": "需要播报的中文"
}

Response:
- Status 200: 直接返回 `audio/wav` 或 `audio/mpeg` 二进制流
- Status 400: `{ "error": "text is required" }`
- Status 503: `{ "error": "[tts-error]" }`

---

## 配置
GET /api/device/config
