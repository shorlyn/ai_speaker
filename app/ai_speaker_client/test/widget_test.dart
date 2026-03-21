// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:ai_speaker_client/main.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('renders tabs', (tester) async {
    await tester.pumpWidget(const AiSpeakerApp());
    expect(find.text('设置'), findsOneWidget);
    expect(find.text('文本'), findsOneWidget);
    expect(find.text('音频'), findsOneWidget);
  });
}
