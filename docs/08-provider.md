# Provider 设计

## 目标
统一对接不同模型服务

## 接口

IAsrProvider
- Task<string> TranscribeAsync(stream)

ILlmProvider
- Task<string> ChatAsync(messages)

ITtsProvider
- Task<string> GenerateAsync(text)

## 实现建议
- 优先接入自有 Ollama 服务；若接口层采用 OpenAI-compatible 格式，仅作为协议兼容实现方式
- 支持切换模型

## 配置示例
{
  "llm": {
    "provider": "Ollama",
    "baseUrl": "https://ollama.example.com",
    "model": "qwen-plus"
  },
  "tts": "cosyvoice",
  "asr": "aliyun"
}
