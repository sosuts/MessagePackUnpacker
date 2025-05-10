using MessagePack;
using MessagePack.Resolvers;
using System;

namespace MessagePackUnpacker.Model
{
    internal class IO
    {
        private string _input;
        private string _output;
        protected byte[] Input { get; set; }
        private string Output
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
            }
        }
        public IO(string input)
        {
            // Convert the input string to a byte array
            Input = Convert.FromBase64String(input);
        }
        internal string DecodeWithMessagePack(string input)
        {
            var dynamicModel = MessagePackSerializer.Deserialize<dynamic>(Input, ContractlessStandardResolver.Options);
            return MessagePackSerializer.ConvertToJson(dynamicModel);
        }
    }
}
