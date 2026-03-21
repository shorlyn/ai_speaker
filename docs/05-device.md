# 设备端设计（Flutter + Android）

## 模块划分

### Flutter
- 设置页
- 状态页
- 日志页

### Android 原生
- ForegroundService
- Recorder
- Player
- WakeWord（P2）

## 工作流
按钮 → 录音 → 上传 → 播放

## 关键点
- 前台服务常驻
- 忽略电池优化
- 固定设备使用（插电）

## 不做
- 后台无限保活 hack
- 本地模型