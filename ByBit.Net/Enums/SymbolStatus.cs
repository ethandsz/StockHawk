﻿using CryptoExchange.Net.Attributes;

namespace Bybit.Net.Enums
{
    /// <summary>
    /// Status of a symbol
    /// </summary>
    public enum SymbolStatus
    {
        /// <summary>
        /// Pre launch
        /// </summary>
        [Map("PreLaunch")]
        PreLaunch,
        /// <summary>
        /// Currently trading
        /// </summary>
        [Map("Trading")]
        Trading,
        /// <summary>
        /// Settling
        /// </summary>
        [Map("Settling")]
        Settling,
        /// <summary>
        /// Settling
        /// </summary>
        [Map("Delivering")]
        Delivering,
        /// <summary>
        /// Closed
        /// </summary>
        [Map("Closed")]
        Closed
    }
}
