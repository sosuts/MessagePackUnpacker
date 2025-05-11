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
        private string _structureText;
        public string StructureText
        {
            get => _structureText;
            set
            {
                _structureText = value;
                OnPropertyChanged();
            }
        }
        public string InputText
        {
            get => _inputText;
            set
            {
                if (value == null)
                {
                    _inputText = null;
                    OnPropertyChanged();
                    return;
                }

                string processedValue = value;

                if (processedValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    // "0x" を削除
                    processedValue = processedValue.Substring(2);
                }

                _inputText = processedValue;
                Deserialize();
                OnPropertyChanged();
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
                byte[] bytes = Util.HexStringToBytes(InputText);
                // MessagePackでデシリアライズ（dynamic型として）
                var deserializedObject = MessagePackSerializer.Deserialize<dynamic>(bytes);

                // 整形されたJSONを生成（pretty print）
                string prettyJson = JsonConvert.SerializeObject(deserializedObject, Formatting.Indented);
                // JSONをOutputTextBoxに表示
                OutputText = prettyJson;
                StructureText = Util.BuildStructure(bytes);
            }
            catch (Exception ex)
            {
                OutputText = "デシリアライズエラー:\n" + ex.Message;
                StructureText = string.Empty;
            }
        }
    }
}