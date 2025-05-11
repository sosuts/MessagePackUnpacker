using MessagePack;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Globalization;
using System.Linq;


namespace MessagePackUnpacker
{
    public static class Util
    {
        internal static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new FormatException("Hex文字列の長さが不正です（偶数桁である必要があります）");

            int len = hex.Length / 2;
            byte[] bytes = new byte[len];

            for (int i = 0; i < len; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
        internal static string BuildStructure(byte[] bytes)
        {
            var sb = new System.Text.StringBuilder();
            var reader = new MessagePackReader(bytes);

            void Dump(ref MessagePackReader r, int indent)
            {
                string Indent(int i) => new string(' ', i * 2);

                try
                {
                    var type = r.NextMessagePackType;
                    var start = r.Position;

                    switch (type)
                    {
                        case MessagePackType.Array:
                            int arrayLength = r.ReadArrayHeader();
                            var arraySlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}[ // array ({arrayLength} items) {ToHex(arraySlice.ToArray())}");
                            for (int i = 0; i < arrayLength; i++)
                            {
                                Dump(ref r, indent + 1);
                            }
                            sb.AppendLine(Indent(indent) + "]");
                            break;

                        case MessagePackType.Map:
                            int mapLength = r.ReadMapHeader();
                            var mapSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}{{ // map ({mapLength} pairs) {ToHex(mapSlice.ToArray())}");
                            for (int i = 0; i < mapLength; i++)
                            {
                                sb.Append(Indent(indent + 1) + "key: ");
                                Dump(ref r, 0);
                                sb.Append(Indent(indent + 1) + "val: ");
                                Dump(ref r, indent + 1);
                            }
                            sb.AppendLine(Indent(indent) + "}");
                            break;

                        case MessagePackType.String:
                            string str = r.ReadString();
                            var strSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}\"{str}\" (string) // {ToHex(strSlice.ToArray())}");
                            break;

                        case MessagePackType.Integer:
                            long intVal = r.ReadInt64();
                            var intSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}{intVal} (int) // {ToHex(intSlice.ToArray())}");
                            break;

                        case MessagePackType.Boolean:
                            bool boolVal = r.ReadBoolean();
                            var boolSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}{boolVal.ToString().ToLower()} (bool) // {ToHex(boolSlice.ToArray())}");
                            break;

                        case MessagePackType.Nil:
                            r.ReadNil();
                            var nilSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}null (nil) // {ToHex(nilSlice.ToArray())}");
                            break;

                        default:
                            var raw = r.ReadRaw();
                            var rawSlice = r.Sequence.Slice(start, r.Position);
                            sb.AppendLine($"{Indent(indent)}<raw {raw.Length} bytes> // {ToHex(rawSlice.ToArray())}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"{Indent(indent)}<error: {ex.Message}>");
                }
            }
            Dump(ref reader, 0);
            return sb.ToString();
        }
        internal static string ToHex(byte[] bytes)
        {
            return "[" + string.Join(" ", bytes.Select(b => b.ToString("X2"))) + "]";
        }
        internal static void DeserializeInput(string input)
        {
            try
            {
                byte[] bytes = HexStringToBytes(input);
                // deserializedObjectは辞書っぽいオブジェクト
                var deserializedObject = MessagePackSerializer.Deserialize<dynamic>(bytes);

                // 整形されたJSONを生成（pretty print）
                string prettyJson = JsonConvert.SerializeObject(deserializedObject, Formatting.Indented);
                // JSONをOutputTextBoxに表示
                int totalWidth = GetVisualWidth(prettyJson);
                Console.WriteLine();
                WriteCenteredLine("入力", 30, '=');
                Console.WriteLine(input);
                Console.WriteLine();
                Console.WriteLine();
                WriteCenteredLine("デシリアライズ結果", 30, '=');
                Console.WriteLine(prettyJson);
                Console.WriteLine();
                Console.WriteLine();
                WriteCenteredLine("バイナリの構造", 30, '=');
                string StructureText = BuildStructure(bytes);
                Console.WriteLine(StructureText);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"デシリアライズエラー: {ex.Message}");
            }
        }
        internal static void WriteCenteredLine(string text, int totalWidth = 50, char fillChar = '=')
        {
            if (text.Length >= totalWidth)
            {
                Console.WriteLine(text); // 長すぎる場合はそのまま出力
                return;
            }

            int padding = totalWidth - text.Length;
            int left = padding / 2;
            int right = padding - left;

            string result = new string(fillChar, left) + text + new string(fillChar, right);
            Console.WriteLine(result);
        }
        internal static int GetVisualWidth(string s)
        {
            // 各文字の視覚的な幅（全角は2, 半角は1）
            return s.Sum(c => EastAsianWidth.GetWidth(c));
        }

        internal static class EastAsianWidth
        {
            public static int GetWidth(char c)
            {
                // 文字の幅を判定（東アジア文字は2、それ以外は1）
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == UnicodeCategory.OtherLetter || cat == UnicodeCategory.OtherSymbol || cat == UnicodeCategory.OtherPunctuation)
                {
                    return 2; // 全角文字
                }
                return 1; // 半角文字
            }
        }
        internal static void ShowHelp()
        {
            Console.WriteLine("GUIの起動方法: オプションをつけずにexeを実行");
            Console.WriteLine();
            Console.WriteLine("  MessagePackUnpacker.exe");
            Console.WriteLine("CLIの使用法: MessagePackUnpacker.exe [オプション] <16進数の文字列>");
            Console.WriteLine();
            Console.WriteLine("オプション:");
            Console.WriteLine("  -i, --input <hex>    入力文字列を指定（16進文字）");
            Console.WriteLine("  -h, --help           このヘルプを表示");
            Console.WriteLine();
            Console.WriteLine("例:");
            Console.WriteLine("  MessagePackUnpacker.exe -i {MessagePackの16進数文字列}");
            Console.WriteLine("  MessagePackUnpacker.exe --input {MessagePackの16進数文字列}");
            Console.WriteLine("  MessagePackUnpacker.exe {MessagePackの16進数文字列}");
        }
    }
}
