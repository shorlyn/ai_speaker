# 架构设计

## 总体架构

[Android设备]
  ├─ 唤醒词（P2）
  ├─ 录音
  ├─ 播放
  └─ API调用
        ↓
[.NET 网关]
  ├─ Device管理
  ├─ Chat编排
  ├─ 会话管理
  ├─ Provider抽象
        ↓
[模型层]
  ├─ ASR
  ├─ LLM（Qwen/Ollama）
  └─ TTS（CosyVoice）

## 设计原则
- App 不直接依赖模型厂商
- 服务端只做“编排”，不做重推理
- Provider 可替换（优先接入自有 Ollama 服务；若接口层采用 OpenAI-compatible 格式，仅作为协议兼容实现方式）

## 数据流
录音 → 上传 → ASR → LLM → TTS → 返回 → 播放
