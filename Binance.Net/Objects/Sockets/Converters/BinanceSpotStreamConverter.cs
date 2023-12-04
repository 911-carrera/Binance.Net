﻿using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;

namespace Binance.Net.Objects.Sockets.Converters
{
    internal class BinanceSpotStreamConverter : SocketConverter
    {
        public override MessageInterpreterPipeline InterpreterPipeline { get; } = new MessageInterpreterPipeline
        {
            GetIdentity = GetIdentity
        };

        private static string? GetIdentity(IMessageAccessor accessor)
        { 
            var id = accessor.GetStringValue("id");
            if (id != null)
                return id;

            var streamValue = accessor.GetStringValue("stream");
            if (streamValue == null)
                return null;

            return streamValue;
        }
    }
}