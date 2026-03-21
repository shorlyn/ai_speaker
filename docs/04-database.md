# 数据库设计

## Device
Id
DeviceCode
DeviceName
SecretKey
LastOnlineTime
IsEnabled
CreatedTime

## ConversationSession
Id
DeviceId
SessionId
StartTime
LastMessageTime

## ConversationMessage
Id
SessionId
Role (user/assistant)
Content
AudioUrl
DurationMs
CreatedTime