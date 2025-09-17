// <copyright file="RCloneUtils.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace KapitelShelf.Api.Utils
{
    /// <summary>
    /// RClone utilities.
    /// </summary>
    public static class RCloneUtils
    {
        /// <summary>
        /// Parsed stats from a single rclone JSON log line.
        /// </summary>
        public sealed record RCloneStats(
            double? Percent, // as percentage
            double? Speed, // in bytes/second
            int? Eta, // in seconds
            double? ElapsedTime // in seconds
        );

        /// <summary>
        /// Tries to parse a single JSON line from rclone and extract <see cref="RCloneStats"/>.
        /// </summary>
        /// <param name="json">the json line from rclone.</param>
        /// <param name="stats">the parsed stats if successful; otherwise default.</param>
        /// <returns>true if parsing succeeded, otherwise false.</returns>
        public static bool TryParseStats(string json, out RCloneStats stats)
        {
            stats = default!;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("stats", out var statsRes) || statsRes.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                // compute percentage from bytes/totalBytes
                double? percent = null;
                if (TryGetLong(statsRes, "bytes", out var bytes) && TryGetLong(statsRes, "totalBytes", out var totalBytes) && totalBytes > 0)
                {
                    percent = (double)bytes / totalBytes * 100.0;
                }

                var speed = TryGetDouble(statsRes, "speed");
                var eta = TryGetInt(statsRes, "eta");
                var elapsed = TryGetDouble(statsRes, "elapsedTime");

                stats = new RCloneStats(Percent: percent, Speed: speed, Eta: eta, ElapsedTime: elapsed);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats a bytes-per-second value to human readable binary units (KiB/MiB/GiB) with "/s" suffix.
        /// </summary>
        /// <param name="bytesPerSecond">bytes per second; null yields null.</param>
        /// <returns>formatted string like "2.29 MiB/s", or null if input is null.</returns>
        public static string? FormatSpeed(double? bytesPerSecond)
        {
            if (!bytesPerSecond.HasValue)
            {
                return null;
            }

            const double k = 1024.0;
            string[] units = ["B", "KiB", "MiB", "GiB", "TiB"];

            double value = bytesPerSecond.Value;
            int idx = 0;

            while (Math.Abs(value) >= k && idx < units.Length - 1)
            {
                value /= k;
                idx++;
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture) + " " + units[idx] + "/s";
        }

        /// <summary>
        /// Formats an ETA in seconds to a human-readable string.
        /// </summary>
        /// <param name="etaSeconds">eta in seconds; null yields null.</param>
        /// <returns>formatted ETA like "2m41s" or "00:02:41", or null.</returns>
        public static string? FormatEta(int? etaSeconds)
        {
            if (!etaSeconds.HasValue || etaSeconds.Value < 0)
            {
                return null;
            }

            var ts = TimeSpan.FromSeconds(etaSeconds.Value);

            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes}m{ts.Seconds:D2}s";
        }

        /// <summary>
        /// Tries to read a <see cref="double"/> from a json property.
        /// </summary>
        /// <param name="obj">json object.</param>
        /// <param name="name">property name.</param>
        /// <returns>nullable double.</returns>
        public static double? TryGetDouble(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var element))
            {
                return null;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var val))
            {
                return val;
            }

            return null;
        }

        /// <summary>
        /// Tries to read a <see cref="long"/> from a json property.
        /// </summary>
        /// <param name="obj">json object.</param>
        /// <param name="name">property name.</param>
        /// <param name="value">parsed value.</param>
        /// <returns>true if parsed, otherwise false.</returns>
        public static bool TryGetLong(JsonElement obj, string name, out long value)
        {
            value = 0;

            if (!obj.TryGetProperty(name, out var element))
            {
                return false;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var val))
            {
                value = val;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to read a nullable <see cref="int"/> from a json property.
        /// </summary>
        /// <param name="obj">json object.</param>
        /// <param name="name">property name.</param>
        /// <returns>nullable int.</returns>
        public static int? TryGetInt(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var element))
            {
                return null;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var num))
            {
                return num;
            }

            return null;
        }
    }
}
