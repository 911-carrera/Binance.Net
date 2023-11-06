﻿using Binance.Net.Objects.Internal;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Blvt;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binance.Net.Objects.Sockets.Converters
{
    internal class BinanceSpotStreamConverter : SocketConverter
    {

        private static Dictionary<string, Type> _streamIdMapping = new Dictionary<string, Type>
        {
            { "!miniTicker@arr", typeof(BinanceCombinedStream<IEnumerable<BinanceStreamMiniTick>>) },
            { "!ticker@arr", typeof(BinanceCombinedStream<IEnumerable<BinanceStreamTick>>) },
            { "!ticker_1h@arr", typeof(BinanceCombinedStream<IEnumerable<BinanceStreamRollingWindowTick>>) },
            { "!ticker_4h@arr", typeof(BinanceCombinedStream<IEnumerable<BinanceStreamRollingWindowTick>>) },
            { "!ticker_1d@arr", typeof(BinanceCombinedStream<IEnumerable<BinanceStreamRollingWindowTick>>) },
        };

        private static Dictionary<string, Type> _eventTypeMapping = new Dictionary<string, Type>
        {
            { "trade", typeof(BinanceCombinedStream<BinanceStreamTrade>) },
            { "kline", typeof(BinanceCombinedStream<BinanceStreamKlineData>) },
            { "aggTrade", typeof(BinanceCombinedStream<BinanceStreamAggregatedTrade>) },
            { "24hrMiniTicker", typeof(BinanceCombinedStream<BinanceStreamMiniTick>) },
            { "24hrTicker", typeof(BinanceCombinedStream<BinanceStreamTick>) },
            { "1hTicker", typeof(BinanceCombinedStream<BinanceStreamRollingWindowTick>) },
            { "4hTicker", typeof(BinanceCombinedStream<BinanceStreamRollingWindowTick>) },
            { "1dTicker", typeof(BinanceCombinedStream<BinanceStreamRollingWindowTick>) },
            { "depthUpdate", typeof(BinanceCombinedStream<BinanceEventOrderBook>) },
            { "nav", typeof(BinanceCombinedStream<BinanceBlvtInfoUpdate>) },
            { "outboundAccountPosition", typeof(BinanceCombinedStream<BinanceStreamPositionsUpdate>) },
            { "balanceUpdate", typeof(BinanceCombinedStream<BinanceStreamBalanceUpdate>) },
            { "executionReport", typeof(BinanceCombinedStream<BinanceStreamOrderUpdate>) },
            { "listStatus", typeof(BinanceCombinedStream<BinanceStreamOrderList>) },
        };

        public override List<StreamMessageParseCallback> InterpreterPipeline { get; } = new List<StreamMessageParseCallback>
        {
            new StreamMessageParseCallback
            {
                TypeFields = new List<string> { "id" },
                IdFields = new List<string> { "id" },
                Callback = GetDeserializationTypeQueryResponse
            },
            new StreamMessageParseCallback
            {
                IdFields = new List<string> { "stream" },
                TypeFields = new List<string> { "stream", "data:e" },
                Callback = GetDeserializationTypeStreamEvent
            }
        };

        private static Type? GetDeserializationTypeQueryResponse(Dictionary<string, string> idValues, IEnumerable<BasePendingRequest> pendingRequests, IEnumerable<Subscription> listeners)
        {
            // different fields present mean different deserializations, if there is an id field then its a query, otherways look at stream field
            // Can we provide some logic to configure this?

            var updateId = int.Parse(idValues["id"]);
            var request = pendingRequests.SingleOrDefault(r => ((BinanceSocketMessage)r.Request).Id == updateId);
            var responseType = request.ResponseType;
            if (responseType == null)
            {
                // Probably shouldn't be exception
                throw new Exception("Unknown update type");
            }

            return responseType;
        }

        private static Type? GetDeserializationTypeStreamEvent(Dictionary<string, string> idValues, IEnumerable<BasePendingRequest> pendingRequests, IEnumerable<Subscription> listeners)
        {
            var streamId = idValues["stream"]!;
            if (_streamIdMapping.TryGetValue(streamId, out var streamIdMapping))
                return streamIdMapping;

            var eventType = idValues["data:e"]!;
            if (_eventTypeMapping.TryGetValue(eventType, out var eventTypeMapping))
                return eventTypeMapping;

            // These are single events but don't have an 'e' event identifier
            if (streamId.EndsWith("@bookTicker")) return typeof(BinanceCombinedStream<BinanceStreamBookPrice>);
            if (streamId.Contains("@depth")) return typeof(BinanceCombinedStream<BinanceOrderBook>);

            return null;
        }
    }
}