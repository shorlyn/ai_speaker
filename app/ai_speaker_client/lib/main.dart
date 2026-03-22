import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:audioplayers/audioplayers.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:path_provider/path_provider.dart';
import 'package:record/record.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const AiSpeakerApp());
}

class AiSpeakerApp extends StatefulWidget {
  const AiSpeakerApp({super.key});

  @override
  State<AiSpeakerApp> createState() => _AiSpeakerAppState();
}

class _AiSpeakerAppState extends State<AiSpeakerApp> {
  final TextEditingController _baseUrlController =
      TextEditingController(text: 'http://192.168.10.15:5224');
  final TextEditingController _deviceCodeController =
      TextEditingController(text: 'dev-001');
  final TextEditingController _deviceNameController =
      TextEditingController(text: '客厅音箱');
  final TextEditingController _userTextController = TextEditingController();

  String? _sessionId;
  String? _healthStatus;
  String? _registerStatus;
  String? _textChatStatus;
  String? _audioChatStatus;
  String? _ttsStatus;
  bool _isRegistering = false;
  bool _isHealthChecking = false;
  bool _isSendingText = false;
  bool _isRecording = false;
  bool _isUploadingAudio = false;
  bool _isSynthesizingTts = false;
  String? _lastAssistantText;

  final Record _recorder = Record();
  final AudioPlayer _audioPlayer = AudioPlayer();
  String? _recordFilePath;

  @override
  void initState() {
    super.initState();
    _loadPrefs();
  }

  Future<void> _loadPrefs() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      _baseUrlController.text =
          prefs.getString('baseUrl') ?? _baseUrlController.text;
      _deviceCodeController.text =
          prefs.getString('deviceCode') ?? _deviceCodeController.text;
      _deviceNameController.text =
          prefs.getString('deviceName') ?? _deviceNameController.text;
      _sessionId = prefs.getString('sessionId');
    });
  }

  Future<void> _savePrefs() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('baseUrl', _baseUrlController.text.trim());
    await prefs.setString('deviceCode', _deviceCodeController.text.trim());
    await prefs.setString('deviceName', _deviceNameController.text.trim());
    if (_sessionId != null) {
      await prefs.setString('sessionId', _sessionId!);
    }
  }

  String? _normalizeBaseUrl() {
    final raw = _baseUrlController.text.trim();
    if (raw.isEmpty) return null;
    return raw.endsWith('/') ? raw.substring(0, raw.length - 1) : raw;
  }

  Uri? _buildUri(String path) {
    final base = _normalizeBaseUrl();
    if (base == null) return null;
    final normalizedPath = path.startsWith('/') ? path : '/$path';
    return Uri.parse('$base$normalizedPath');
  }

  Future<void> _testHealth() async {
    final uri = _buildUri('/health');
    if (uri == null) {
      setState(() => _healthStatus = 'BaseUrl 不能为空');
      return;
    }
    setState(() {
      _isHealthChecking = true;
      _healthStatus = null;
    });
    try {
      final response = await http.get(uri).timeout(const Duration(seconds: 5));
      if (response.statusCode == 200) {
        setState(() => _healthStatus = '✅ 可连接');
      } else {
        setState(() => _healthStatus = '❌ HTTP ${response.statusCode}');
      }
    } catch (e) {
      setState(() => _healthStatus = '❌ $e');
    } finally {
      setState(() => _isHealthChecking = false);
    }
  }

  Future<void> _registerDevice() async {
    final uri = _buildUri('/api/device/register');
    if (uri == null) {
      setState(() => _registerStatus = 'BaseUrl 不能为空');
      return;
    }
    final deviceCode = _deviceCodeController.text.trim();
    final deviceName = _deviceNameController.text.trim();
    if (deviceCode.isEmpty || deviceName.isEmpty) {
      setState(() => _registerStatus = 'DeviceCode/DeviceName 不能为空');
      return;
    }
    await _savePrefs();
    setState(() {
      _isRegistering = true;
      _registerStatus = null;
    });
    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'deviceCode': deviceCode,
              'deviceName': deviceName,
            }),
          )
          .timeout(const Duration(seconds: 10));
      if (response.statusCode == 200) {
        final body = jsonDecode(response.body) as Map<String, dynamic>;
        setState(() => _registerStatus =
            '注册成功，secretKey=${body['secretKey'] ?? '未知'}');
      } else {
        setState(() => _registerStatus = '注册失败：HTTP ${response.statusCode}');
      }
    } catch (e) {
      setState(() => _registerStatus = '注册失败：$e');
    } finally {
      setState(() => _isRegistering = false);
    }
  }

  Future<void> _sendTextChat() async {
    final uri = _buildUri('/api/chat/text');
    if (uri == null) {
      setState(() => _textChatStatus = 'BaseUrl 不能为空');
      return;
    }
    final deviceCode = _deviceCodeController.text.trim();
    final text = _userTextController.text.trim();
    if (deviceCode.isEmpty || text.isEmpty) {
      setState(() => _textChatStatus = 'DeviceCode/UserText 不能为空');
      return;
    }
    await _savePrefs();
    setState(() {
      _isSendingText = true;
      _textChatStatus = null;
    });
    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'deviceCode': deviceCode,
              'sessionId': _sessionId,
              'userText': text,
            }),
          )
          .timeout(const Duration(seconds: 10));
      if (response.statusCode == 200) {
        final body = jsonDecode(response.body) as Map<String, dynamic>;
        _sessionId = body['sessionId'] as String?;
        _lastAssistantText = body['assistantText'] as String?;
        await _savePrefs();
        var prompt = 'sessionId=$_sessionId';
        if (_lastAssistantText == '[ollama-error]') {
          prompt += '，服务暂不可用';
        }
        setState(() => _textChatStatus = prompt);
        unawaited(_playAssistantTts(_lastAssistantText));
      } else {
        setState(() =>
            _textChatStatus = '对话失败：HTTP ${response.statusCode}');
      }
    } catch (e) {
      setState(() => _textChatStatus = '对话失败：$e');
    } finally {
      setState(() => _isSendingText = false);
    }
  }

  Future<void> _startRecording() async {
    final deviceCode = _deviceCodeController.text.trim();
    if (deviceCode.isEmpty) {
      setState(() => _audioChatStatus = 'DeviceCode 不能为空');
      return;
    }
    if (!await _recorder.hasPermission()) {
      setState(() => _audioChatStatus = '未授予录音权限');
      return;
    }
    final dir = await getTemporaryDirectory();
    final path =
        '${dir.path}/audio-${DateTime.now().millisecondsSinceEpoch}.wav';
    await _recorder.start(
      path: path,
      encoder: AudioEncoder.wav,
      bitRate: 128000,
      samplingRate: 16000,
      numChannels: 1,
    );
    setState(() {
      _isRecording = true;
      _audioChatStatus = '录音中...';
      _recordFilePath = path;
    });
  }

  Future<void> _stopRecordingAndSend() async {
    if (!_isRecording) return;
    final path = await _recorder.stop();
    setState(() {
      _isRecording = false;
      _audioChatStatus = '录音结束，准备上传...';
      _recordFilePath = path ?? _recordFilePath;
    });
    if (path == null) {
      setState(() => _audioChatStatus = '未获得录音文件');
      return;
    }
    await _uploadAudio(path);
  }

  Future<void> _uploadAudio(String filePath) async {
    final uri = _buildUri('/api/chat/audio');
    if (uri == null) {
      setState(() => _audioChatStatus = 'BaseUrl 不能为空');
      return;
    }
    final deviceCode = _deviceCodeController.text.trim();
    if (deviceCode.isEmpty) {
      setState(() => _audioChatStatus = 'DeviceCode 不能为空');
      return;
    }
    await _savePrefs();
    setState(() {
      _isUploadingAudio = true;
      _audioChatStatus = '上传中...';
    });
    try {
      final request = http.MultipartRequest('POST', uri)
        ..fields['deviceCode'] = deviceCode
        ..fields['sessionId'] = _sessionId ?? ''
        ..files.add(await http.MultipartFile.fromPath(
          'file',
          filePath,
          filename: 'audio.wav',
          contentType: MediaType('audio', 'wav'),
        ));
      final streamed = await request.send().timeout(const Duration(seconds: 15));
      final response = await http.Response.fromStream(streamed);
      if (response.statusCode == 200) {
        final body = jsonDecode(response.body) as Map<String, dynamic>;
        _sessionId = body['sessionId'] as String?;
        _lastAssistantText = body['assistantText'] as String?;
        await _savePrefs();
        var prompt = '语音成功，sessionId=$_sessionId';
        if (_lastAssistantText == '[ollama-error]') {
          prompt += '，服务暂不可用';
        }
        setState(() => _audioChatStatus = prompt);
        unawaited(_playAssistantTts(_lastAssistantText));
      } else {
        setState(() =>
            _audioChatStatus = '上传失败：HTTP ${response.statusCode}');
      }
    } catch (e) {
      setState(() => _audioChatStatus = '上传失败：$e');
    } finally {
      setState(() => _isUploadingAudio = false);
    }
  }

  Future<void> _playAssistantTts(String? assistantText) async {
    if (_isSynthesizingTts) {
      return;
    }
    final text = assistantText?.trim();
    if (text == null || text.isEmpty || text == '[ollama-error]') {
      setState(() {
        if (assistantText == '[ollama-error]') {
          _ttsStatus = '语音合成失败，文本已显示';
        }
      });
      return;
    }

    final uri = _buildUri('/api/tts/speak');
    if (uri == null) {
      setState(() => _ttsStatus = '语音合成失败，文本已显示');
      return;
    }

    setState(() {
      _isSynthesizingTts = true;
      _ttsStatus = '语音合成中...';
    });

    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'text': text}),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final contentType = response.headers['content-type'] ?? 'audio/wav';
        final ext = contentType.contains('mpeg') ? 'mp3' : 'wav';
        final dir = await getTemporaryDirectory();
        final filePath =
            '${dir.path}/tts-${DateTime.now().millisecondsSinceEpoch}.$ext';
        final file = File(filePath);
        await file.writeAsBytes(response.bodyBytes);
        await _audioPlayer.stop();
        await _audioPlayer.play(DeviceFileSource(file.path));
        setState(() => _ttsStatus = '语音播放完成');
      } else {
        setState(() => _ttsStatus = '语音合成失败，文本已显示');
      }
    } catch (e) {
      setState(() => _ttsStatus = '语音合成失败，文本已显示');
    } finally {
      setState(() => _isSynthesizingTts = false);
    }
  }

  Future<void> _resetSession() async {
    setState(() => _sessionId = null);
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('sessionId');
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AI Speaker Client',
      theme: ThemeData(colorSchemeSeed: Colors.teal, useMaterial3: true),
      home: DefaultTabController(
        length: 3,
        child: Scaffold(
          appBar: AppBar(
            title: const Text('AI Speaker Minimal Client'),
            bottom: const TabBar(
              tabs: [
                Tab(text: '设置'),
                Tab(text: '文本'),
                Tab(text: '音频'),
              ],
            ),
          ),
          body: TabBarView(
            children: [
              _buildSettingsTab(),
              _buildTextTab(),
              _buildAudioTab(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSettingsTab() {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: ListView(
        children: [
          TextField(
            controller: _baseUrlController,
            decoration: const InputDecoration(
              labelText: 'BaseUrl (e.g. http://192.168.0.10:5224)',
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _deviceCodeController,
            decoration: const InputDecoration(labelText: 'DeviceCode'),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _deviceNameController,
            decoration: const InputDecoration(labelText: 'DeviceName'),
          ),
          const SizedBox(height: 16),
          Wrap(
            spacing: 12,
            children: [
              ElevatedButton.icon(
                onPressed: _isHealthChecking ? null : _testHealth,
                icon: const Icon(Icons.wifi_tethering),
                label: Text(_isHealthChecking ? '检测中...' : '测试健康'),
              ),
              ElevatedButton.icon(
                onPressed: _isRegistering ? null : _registerDevice,
                icon: const Icon(Icons.app_registration),
                label: Text(_isRegistering ? '注册中...' : '注册设备'),
              ),
              OutlinedButton(
                onPressed: _resetSession,
                child: const Text('清空 sessionId'),
              ),
            ],
          ),
          if (_healthStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('健康状态：$_healthStatus'),
            ),
          if (_registerStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('注册状态：$_registerStatus'),
            ),
          if (_sessionId != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('当前 sessionId：$_sessionId'),
            ),
        ],
      ),
    );
  }

  Widget _buildTextTab() {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          TextField(
            controller: _userTextController,
            decoration: const InputDecoration(
              labelText: 'UserText',
              border: OutlineInputBorder(),
            ),
            minLines: 3,
            maxLines: 5,
          ),
          const SizedBox(height: 12),
          ElevatedButton.icon(
            onPressed: _isSendingText ? null : _sendTextChat,
            icon: const Icon(Icons.send),
            label: Text(_isSendingText ? '发送中...' : '发送文本'),
          ),
          if (_textChatStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('状态：$_textChatStatus'),
            ),
          if (_lastAssistantText != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('assistantText：$_lastAssistantText'),
            ),
          if (_ttsStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('TTS：$_ttsStatus'),
            ),
        ],
      ),
    );
  }

  Widget _buildAudioTab() {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          ElevatedButton.icon(
            onPressed: (_isRecording || _isUploadingAudio) ? null : _startRecording,
            icon: const Icon(Icons.mic),
            label: Text(_isRecording ? '录音中...' : '开始录音'),
          ),
          const SizedBox(height: 8),
          ElevatedButton.icon(
            onPressed: _isRecording ? _stopRecordingAndSend : null,
            icon: const Icon(Icons.stop_circle_outlined),
            label: const Text('停止并上传'),
          ),
          const SizedBox(height: 8),
          if (!_isRecording && _recordFilePath != null)
            Text('最近文件：$_recordFilePath'),
          if (_audioChatStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('状态：$_audioChatStatus'),
            ),
          if (_isUploadingAudio)
            const Padding(
              padding: EdgeInsets.only(top: 8),
              child: LinearProgressIndicator(),
            ),
          if (_lastAssistantText != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('assistantText：$_lastAssistantText'),
            ),
          if (_ttsStatus != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text('TTS：$_ttsStatus'),
            ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _baseUrlController.dispose();
    _deviceCodeController.dispose();
    _deviceNameController.dispose();
    _userTextController.dispose();
    _recorder.dispose();
    _audioPlayer.dispose();
    super.dispose();
  }
}
