using MessagePack;
using MessagePackUnpacker.Base;
using Newtonsoft.Json;
using System;


namespace MessagePackUnpacker
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private string _inputText;
        private string _outputText;

        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText.StartsWith("0x"))
                {
                    // 0xプレフィックスを削除
                    _inputText = _inputText.Substring(2);
                }
                _inputText = value;
                OnPropertyChanged();
                Deserialize();
            }
        }
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }
        private void Deserialize()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InputText))
                {
                    OutputText = string.Empty;
                    return;
                }

                byte[] bytes = HexStringToBytes(InputText);

                // MessagePackでデシリアライズ（dynamic型として）
                var deserializedObject = MessagePackSerializer.Deserialize<dynamic>(bytes);

                // 整形されたJSONを生成（pretty print）
                string prettyJson = JsonConvert.SerializeObject(deserializedObject, Formatting.Indented);

                // JSONをOutputTextBoxに表示
                OutputText = prettyJson;
            }
            catch (Exception ex)
            {
                OutputText = "デシリアライズエラー:\n" + ex.Message;
            }
        }
        private byte[] HexStringToBytes(string hex)
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
    }
}
