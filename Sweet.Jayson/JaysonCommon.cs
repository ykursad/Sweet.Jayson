﻿# region License
//	The MIT License (MIT)
//
//	Copyright (c) 2015, Cagatay Dogan
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//
//		The above copyright notice and this permission notice shall be included in
//		all copies or substantial portions of the Software.
//
//		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//		THE SOFTWARE.
# endregion License

using System;
using System.Collections;
#if !(NET3500 || NET3000 || NET2000)
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace Sweet.Jayson
{
    public static class JaysonCommon
    {
        # region Static Members

        public static readonly IFormatterConverter FormatterConverter = new FormatterConverter();

        private static int s_IsMono = -1;

        private static readonly int LowerCaseDif = (int)'a' - (int)'A';

        private static TimeSpan s_UtcOffsetUpdate;
        private static long s_LastUtcOffsetUpdate = -1;

        private static readonly JaysonSynchronizedDictionary<Type, bool> s_IsGenericCollection = new JaysonSynchronizedDictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, bool> s_IsGenericDictionary = new JaysonSynchronizedDictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, bool> s_IsGenericList = new JaysonSynchronizedDictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
#if !(NET3500 || NET3000 || NET2000)
        private static readonly JaysonSynchronizedDictionary<Type, bool> s_IsProducerConsumerCollection = new JaysonSynchronizedDictionary<Type, bool>(JaysonConstants.CacheInitialCapacity);
#endif
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_ICollectionAdd = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_IDictionaryAdd = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_StackPush = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_QueueEnqueue = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
#if !(NET3500 || NET3000 || NET2000)
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_ConcurrentBagAdd = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, Action<object, object[]>> s_IProducerConsumerCollectionAdd = new JaysonSynchronizedDictionary<Type, Action<object, object[]>>(JaysonConstants.CacheInitialCapacity);
#endif
        private static readonly JaysonSynchronizedDictionary<string, Type> s_TypeCache = new JaysonSynchronizedDictionary<string, Type>(JaysonConstants.CacheInitialCapacity, StringComparer.OrdinalIgnoreCase);
        private static readonly JaysonSynchronizedDictionary<string, Assembly> s_AssemblyCache = new JaysonSynchronizedDictionary<string, Assembly>(JaysonConstants.CacheInitialCapacity, StringComparer.OrdinalIgnoreCase);
        private static readonly JaysonSynchronizedDictionary<Assembly, string> s_AssemblyNameCache = new JaysonSynchronizedDictionary<Assembly, string>(JaysonConstants.CacheInitialCapacity);
        private static readonly JaysonSynchronizedDictionary<Type, Type> s_GenericListArgs = new JaysonSynchronizedDictionary<Type, Type>(JaysonConstants.CacheInitialCapacity);        
        private static readonly JaysonSynchronizedDictionary<Type, Type> s_GenericCollectionArgs = new JaysonSynchronizedDictionary<Type, Type>(JaysonConstants.CacheInitialCapacity);       
        private static readonly JaysonSynchronizedDictionary<Type, Type[]> s_GenericDictionaryArgs = new JaysonSynchronizedDictionary<Type, Type[]>(JaysonConstants.CacheInitialCapacity);
#if !(NET3500 || NET3000 || NET2000)
        private static readonly JaysonSynchronizedDictionary<Type, Type> s_ProducerConsumerCollectionArgs = new JaysonSynchronizedDictionary<Type, Type>(JaysonConstants.CacheInitialCapacity);
#endif

        # endregion Static Members

        # region Static .Ctor

        static JaysonCommon()
        {
            lock (((ICollection)s_TypeCache).SyncRoot)
            {
                s_TypeCache["bool"] = typeof(bool);
                s_TypeCache["byte"] = typeof(byte);
                s_TypeCache["short"] = typeof(short);
                s_TypeCache["int"] = typeof(int);
                s_TypeCache["long"] = typeof(long);
                s_TypeCache["ushort"] = typeof(ushort);
                s_TypeCache["uint"] = typeof(uint);
                s_TypeCache["ulong"] = typeof(ulong);
                s_TypeCache["double"] = typeof(double);
                s_TypeCache["float"] = typeof(float);
                s_TypeCache["decimal"] = typeof(decimal);
                s_TypeCache["string"] = typeof(string);

                s_TypeCache["Boolean"] = typeof(Boolean);
                s_TypeCache["Int16"] = typeof(Int16);
                s_TypeCache["Int32"] = typeof(Int32);
                s_TypeCache["Int64"] = typeof(Int64);
                s_TypeCache["UInt16"] = typeof(UInt16);
                s_TypeCache["UInt32"] = typeof(UInt32);
                s_TypeCache["UInt64"] = typeof(UInt64);
                s_TypeCache["Single"] = typeof(Single);
                s_TypeCache["DateTime"] = typeof(DateTime);
                s_TypeCache["TimeSpan"] = typeof(TimeSpan);

                s_TypeCache["System.Boolean"] = typeof(Boolean);
                s_TypeCache["System.Byte"] = typeof(Byte);
                s_TypeCache["System.Int16"] = typeof(Int16);
                s_TypeCache["System.Int16"] = typeof(Int16);
                s_TypeCache["System.Int32"] = typeof(Int32);
                s_TypeCache["System.Int64"] = typeof(Int64);
                s_TypeCache["System.UInt16"] = typeof(UInt16);
                s_TypeCache["System.UInt32"] = typeof(UInt32);
                s_TypeCache["System.UInt64"] = typeof(UInt64);
                s_TypeCache["System.Double"] = typeof(Double);
                s_TypeCache["System.Single"] = typeof(Single);
                s_TypeCache["System.Decimal"] = typeof(Decimal);
                s_TypeCache["System.String"] = typeof(string);
                s_TypeCache["System.DateTime"] = typeof(DateTime);
                s_TypeCache["System.TimeSpan"] = typeof(TimeSpan);

                s_TypeCache["System.Boolean, mscorlib"] = typeof(Boolean);
                s_TypeCache["System.Byte, mscorlib"] = typeof(Byte);
                s_TypeCache["System.Int16, mscorlib"] = typeof(Int16);
                s_TypeCache["System.Int16, mscorlib"] = typeof(Int16);
                s_TypeCache["System.Int32, mscorlib"] = typeof(Int32);
                s_TypeCache["System.Int64, mscorlib"] = typeof(Int64);
                s_TypeCache["System.UInt16, mscorlib"] = typeof(UInt16);
                s_TypeCache["System.UInt32, mscorlib"] = typeof(UInt32);
                s_TypeCache["System.UInt64, mscorlib"] = typeof(UInt64);
                s_TypeCache["System.Double, mscorlib"] = typeof(Double);
                s_TypeCache["System.Single, mscorlib"] = typeof(Single);
                s_TypeCache["System.Decimal, mscorlib"] = typeof(Decimal);
                s_TypeCache["System.String, mscorlib"] = typeof(string);
                s_TypeCache["System.DateTime, mscorlib"] = typeof(DateTime);
                s_TypeCache["System.TimeSpan, mscorlib"] = typeof(TimeSpan);

                s_TypeCache["bool?"] = typeof(bool?);
                s_TypeCache["byte?"] = typeof(byte?);
                s_TypeCache["short?"] = typeof(short?);
                s_TypeCache["int?"] = typeof(int?);
                s_TypeCache["long?"] = typeof(long?);
                s_TypeCache["ushort?"] = typeof(ushort?);
                s_TypeCache["uint?"] = typeof(uint?);
                s_TypeCache["ulong?"] = typeof(ulong?);
                s_TypeCache["double?"] = typeof(double?);
                s_TypeCache["float?"] = typeof(float?);
                s_TypeCache["decimal?"] = typeof(decimal?);
                s_TypeCache["bool[]"] = typeof(bool[]);
                s_TypeCache["byte[]"] = typeof(byte[]);
                s_TypeCache["short[]"] = typeof(short[]);
                s_TypeCache["int[]"] = typeof(int[]);
                s_TypeCache["long[]"] = typeof(long[]);
                s_TypeCache["ushort[]"] = typeof(ushort[]);
                s_TypeCache["uint[]"] = typeof(uint[]);
                s_TypeCache["ulong[]"] = typeof(ulong[]);
                s_TypeCache["double[]"] = typeof(double[]);
                s_TypeCache["float[]"] = typeof(float[]);
                s_TypeCache["decimal[]"] = typeof(decimal[]);
                s_TypeCache["string[]"] = typeof(string[]);

                s_TypeCache["Boolean[]"] = typeof(bool[]);
                s_TypeCache["DateTime[]"] = typeof(DateTime[]);
                s_TypeCache["TimeSpan[]"] = typeof(TimeSpan[]);

                s_TypeCache["System.Boolean[]"] = typeof(bool[]);
                s_TypeCache["System.Byte[]"] = typeof(Byte[]);
                s_TypeCache["System.Int16[]"] = typeof(Int16[]);
                s_TypeCache["System.Int32[]"] = typeof(Int32[]);
                s_TypeCache["System.Int64[]"] = typeof(Int64[]);
                s_TypeCache["System.UInt16[]"] = typeof(UInt16[]);
                s_TypeCache["System.UInt32[]"] = typeof(UInt32[]);
                s_TypeCache["System.UInt64[]"] = typeof(UInt64[]);
                s_TypeCache["System.Double[]"] = typeof(Double[]);
                s_TypeCache["System.Single[]"] = typeof(Single[]);
                s_TypeCache["System.Decimal[]"] = typeof(Decimal[]);
                s_TypeCache["System.String[]"] = typeof(string[]);
                s_TypeCache["System.DateTime[]"] = typeof(DateTime[]);
                s_TypeCache["System.TimeSpan[]"] = typeof(TimeSpan[]);

                s_TypeCache["System.Boolean[], mscorlib"] = typeof(bool[]);
                s_TypeCache["System.Byte[], mscorlib"] = typeof(Byte[]);
                s_TypeCache["System.Int16[], mscorlib"] = typeof(Int16[]);
                s_TypeCache["System.Int32[], mscorlib"] = typeof(Int32[]);
                s_TypeCache["System.Int64[], mscorlib"] = typeof(Int64[]);
                s_TypeCache["System.UInt16[], mscorlib"] = typeof(UInt16[]);
                s_TypeCache["System.UInt32[], mscorlib"] = typeof(UInt32[]);
                s_TypeCache["System.UInt64[], mscorlib"] = typeof(UInt64[]);
                s_TypeCache["System.Double[], mscorlib"] = typeof(Double[]);
                s_TypeCache["System.Single[], mscorlib"] = typeof(Single[]);
                s_TypeCache["System.Decimal[], mscorlib"] = typeof(Decimal[]);
                s_TypeCache["System.String[], mscorlib"] = typeof(string[]);
                s_TypeCache["System.DateTime[], mscorlib"] = typeof(DateTime[]);
                s_TypeCache["System.TimeSpan[], mscorlib"] = typeof(TimeSpan[]);

                s_TypeCache["System.Nullable`1[System.Boolean]"] = typeof(bool?);
                s_TypeCache["System.Nullable`1[System.Byte]"] = typeof(Byte?);
                s_TypeCache["System.Nullable`1[System.Int16]"] = typeof(Int16?);
                s_TypeCache["System.Nullable`1[System.Int32]"] = typeof(Int32?);
                s_TypeCache["System.Nullable`1[System.Int64]"] = typeof(Int64?);
                s_TypeCache["System.Nullable`1[System.UInt16]"] = typeof(UInt16?);
                s_TypeCache["System.Nullable`1[System.UInt32]"] = typeof(UInt32?);
                s_TypeCache["System.Nullable`1[System.UInt64]"] = typeof(UInt64?);
                s_TypeCache["System.Nullable`1[System.Double]"] = typeof(Double?);
                s_TypeCache["System.Nullable`1[System.Single]"] = typeof(Single?);
                s_TypeCache["System.Nullable`1[System.Decimal]"] = typeof(Decimal?);
                s_TypeCache["System.Nullable`1[System.DateTime]"] = typeof(DateTime?);
                s_TypeCache["System.Nullable`1[System.TimeSpan]"] = typeof(TimeSpan?);

                s_TypeCache["System.Nullable`1[System.Boolean, mscorlib]"] = typeof(bool?);
                s_TypeCache["System.Nullable`1[System.Byte, mscorlib]"] = typeof(Byte?);
                s_TypeCache["System.Nullable`1[System.Int16, mscorlib]"] = typeof(Int16?);
                s_TypeCache["System.Nullable`1[System.Int32, mscorlib]"] = typeof(Int32?);
                s_TypeCache["System.Nullable`1[System.Int64, mscorlib]"] = typeof(Int64?);
                s_TypeCache["System.Nullable`1[System.UInt16, mscorlib]"] = typeof(UInt16?);
                s_TypeCache["System.Nullable`1[System.UInt32, mscorlib]"] = typeof(UInt32?);
                s_TypeCache["System.Nullable`1[System.UInt64, mscorlib]"] = typeof(UInt64?);
                s_TypeCache["System.Nullable`1[System.Double, mscorlib]"] = typeof(Double?);
                s_TypeCache["System.Nullable`1[System.Single, mscorlib]"] = typeof(Single?);
                s_TypeCache["System.Nullable`1[System.Decimal, mscorlib]"] = typeof(Decimal?);
                s_TypeCache["System.Nullable`1[System.DateTime, mscorlib]"] = typeof(DateTime?);
                s_TypeCache["System.Nullable`1[System.TimeSpan, mscorlib]"] = typeof(TimeSpan?);

                s_TypeCache["System.Nullable`1[System.Boolean, mscorlib], mscorlib"] = typeof(bool?);
                s_TypeCache["System.Nullable`1[System.Byte, mscorlib], mscorlib"] = typeof(Byte?);
                s_TypeCache["System.Nullable`1[System.Int16, mscorlib], mscorlib"] = typeof(Int16?);
                s_TypeCache["System.Nullable`1[System.Int32, mscorlib], mscorlib"] = typeof(Int32?);
                s_TypeCache["System.Nullable`1[System.Int64, mscorlib], mscorlib"] = typeof(Int64?);
                s_TypeCache["System.Nullable`1[System.UInt16, mscorlib], mscorlib"] = typeof(UInt16?);
                s_TypeCache["System.Nullable`1[System.UInt32, mscorlib], mscorlib"] = typeof(UInt32?);
                s_TypeCache["System.Nullable`1[System.UInt64, mscorlib], mscorlib"] = typeof(UInt64?);
                s_TypeCache["System.Nullable`1[System.Double, mscorlib], mscorlib"] = typeof(Double?);
                s_TypeCache["System.Nullable`1[System.Single, mscorlib], mscorlib"] = typeof(Single?);
                s_TypeCache["System.Nullable`1[System.Decimal, mscorlib], mscorlib"] = typeof(Decimal?);
                s_TypeCache["System.Nullable`1[System.DateTime, mscorlib], mscorlib"] = typeof(DateTime?);
                s_TypeCache["System.Nullable`1[System.TimeSpan, mscorlib], mscorlib"] = typeof(TimeSpan?);
            }
        }

        # endregion Static .Ctor

        # region Helper Methods

        # region DateTime Methods

        public static TimeSpan GetUtcOffset(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeSpan.Zero;
            }
            if ((dateTime.Ticks / TimeSpan.TicksPerHour) == (DateTime.UtcNow.Ticks / TimeSpan.TicksPerHour))
            {
                return GetLocalUtcOffset();
            }
#if (NET3000 || NET2000)
            return TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
#else
            return TimeZoneInfo.Local.GetUtcOffset(dateTime);
#endif
        }

        public static TimeSpan GetLocalUtcOffset()
        {
            long utcNow = 0;
            if ((s_LastUtcOffsetUpdate < 0) || ((utcNow = DateTime.UtcNow.Ticks) - s_LastUtcOffsetUpdate > 10000000))
            {
                s_LastUtcOffsetUpdate = utcNow > 0 ? utcNow : DateTime.UtcNow.Ticks;
#if (NET3000 || NET2000)
                s_UtcOffsetUpdate = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
#else
                s_UtcOffsetUpdate = TimeZoneInfo.Local.BaseUtcOffset;
#endif
            }
            return s_UtcOffsetUpdate;
        }

        public static DateTime ToLocalTime(DateTime dateTime)
        {
            DateTimeKind kind = dateTime.Kind;
            if (kind == DateTimeKind.Local)
            {
                return dateTime;
            }

            TimeSpan utcOffset = GetUtcOffset(dateTime);

            long utcOffsetTicks = utcOffset.Ticks;
            if (utcOffsetTicks == 0)
            {
                return new DateTime(dateTime.Ticks, DateTimeKind.Local);
            }

            if (utcOffsetTicks > 0)
            {
                if (DateTime.MaxValue - utcOffset < dateTime)
                {
                    return new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Local);
                }
            }
            else if (dateTime.Ticks + utcOffsetTicks < DateTime.MinValue.Ticks)
            {
                return new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Local);
            }

            return new DateTime(dateTime.AddTicks(utcOffsetTicks).Ticks, DateTimeKind.Local);
        }

        public static DateTime ToUniversalTime(DateTime dateTime)
        {
            DateTimeKind kind = dateTime.Kind;
            if (kind == DateTimeKind.Utc)
            {
                return dateTime;
            }
            if (dateTime == DateTime.MinValue)
            {
                return JaysonConstants.DateTimeUtcMinValue;
            }
            if (kind == DateTimeKind.Unspecified)
            {
                return new DateTime(dateTime.Subtract(GetUtcOffset(dateTime)).Ticks, DateTimeKind.Utc);
            }

            long ticks = dateTime.Ticks - GetUtcOffset(dateTime).Ticks;
            if (ticks > 3155378975999999999L)
            {
                return new DateTime(3155378975999999999L, DateTimeKind.Utc);
            }
            if (ticks < 0L)
            {
                return new DateTime(0L, DateTimeKind.Utc);
            }
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static TimeSpan SinceUnixEpochStart(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime.Subtract(JaysonConstants.DateTimeUnixEpochMinValue);
            }
            return ToUniversalTime(dateTime).Subtract(JaysonConstants.DateTimeUnixEpochMinValue);
        }

        public static long ToUnixTimeSec(DateTime dateTime)
        {
            return SinceUnixEpochStart(dateTime).Ticks / TimeSpan.TicksPerSecond;
        }

        public static long ToUnixTimeMsec(DateTime dateTime)
        {
            return (long)(SinceUnixEpochStart(dateTime).TotalMilliseconds);
        }

        public static long ToUnixTimeMsec(long ticks)
        {
            return (ticks - JaysonConstants.UnixEpochMinValue) / TimeSpan.TicksPerMillisecond;
        }

        public static DateTime FromUnixTimeSec(long unixTime)
        {
            return JaysonConstants.DateTimeUnixEpochMinValue + TimeSpan.FromSeconds(unixTime);
        }

        public static DateTime FromUnixTimeMsec(long msecSince1970)
        {
            return JaysonConstants.DateTimeUnixEpochMinValue + TimeSpan.FromMilliseconds(msecSince1970);
        }

        public static DateTime FromUnixTimeMsec(long msecSince1970, TimeSpan offset)
        {
            return new DateTime(JaysonConstants.DateTimeUnixEpochMinValueUnspecified.Ticks +
                TimeSpan.FromMilliseconds(msecSince1970).Ticks + offset.Ticks, DateTimeKind.Local);
        }

        // Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
        // dd-MM-yyyyTHH:mm:ss.fffffff%K 
        public static DateTime ParseIso8601DateTime(string str,
            JaysonDateTimeZoneType timeZoneType = JaysonDateTimeZoneType.KeepAsIs)
        {
            DateTime dateTime;
            TimeSpan timeSpan;
            ParseIso8601DateTimeOffset(str, out dateTime, out timeSpan);

	    if (datetime == DateTime.MinValue || datetime == DateTime.MaxValue )
	    {
            	return dateTime;
	    } 
		
            switch (timeZoneType)
            {
                case JaysonDateTimeZoneType.ConvertToUtc:
                    {
                        if (timeSpan == TimeSpan.Zero)
                        {
                            return new DateTime(dateTime.Ticks, DateTimeKind.Utc);
                        }
                        return new DateTime(dateTime.Subtract(timeSpan).Ticks, DateTimeKind.Utc);
                    }
                case JaysonDateTimeZoneType.ConvertToLocal:
                    {
                        if (timeSpan == TimeSpan.Zero)
                        {
                            return ToLocalTime(dateTime);
                        }
                        return ToLocalTime(dateTime.Subtract(timeSpan));
                    }
                default:
                    {
                        if (timeSpan == TimeSpan.Zero)
                        {
                            return dateTime;
                        }
                        return ToLocalTime(dateTime.Subtract(timeSpan));
                    }
            }
        }

        // Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
        // dd-MM-yyyyTHH:mm:ss.fffffff%K 
        public static DateTimeOffset ParseIso8601DateTimeOffset(string str)
        {
            DateTime dateTime;
            TimeSpan timeSpan;
            ParseIso8601DateTimeOffset(str, out dateTime, out timeSpan);
            return new DateTimeOffset(dateTime, timeSpan);
        }

        // Supports: yyyy-MM-ddTHH:mm:ss.fffffff%K, yyyyMMddTHHmmss.fffffff%K and
        // dd-MM-yyyyTHH:mm:ss.fffffff%K 
        public static void ParseIso8601DateTimeOffset(string str, out DateTime dateTime, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            if (str == null)
            {
                dateTime = default(DateTime);
                return;
            }

            int length = str.Length;
            if (length == 0)
            {
                dateTime = default(DateTime);
                return;
            }

            if (length < 10)
            {
                throw new JaysonException(JaysonError.InvalidISO8601DateFormat);
            }

            DateTimeKind kind = DateTimeKind.Unspecified;
            if (str[length - 1] == 'Z')
            {
                kind = DateTimeKind.Utc;
            }

            try
            {
                int year = 0;
                int month = 1;
                int day = 1;

                char ch;
                int pos = 0;
                bool basic = false;

                if (str[2] == '-')
                {
                    day = 10 * (int)(str[0] - '0') + (int)(str[1] - '0');
                    month = 10 * (int)(str[3] - '0') + (int)(str[4] - '0');

                    year = 1000 * (int)(str[6] - '0') + 100 * (int)(str[7] - '0') +
                        10 * (int)(str[8] - '0') + (int)(str[9] - '0');
                    pos = 10;
                }
                else
                {
                    year = 1000 * (int)(str[0] - '0') + 100 * (int)(str[1] - '0') +
                        10 * (int)(str[2] - '0') + (int)(str[3] - '0');

                    ch = str[4];
                    basic = ch >= '0' && ch <= '9';
                    if (basic)
                    {
                        month = 10 * (int)(str[4] - '0') + (int)(str[5] - '0');
                        day = 10 * (int)(str[6] - '0') + (int)(str[7] - '0');
                        pos = 8;
                    }
                    else
                    {
                        month = 10 * (int)(str[5] - '0') + (int)(str[6] - '0');
                        day = 10 * (int)(str[8] - '0') + (int)(str[9] - '0');
                        pos = 10;
                    }
                }

                if (month > 12 && day < 13)
                {
                    int tmp = month;
                    month = day;
                    day = tmp;
                }

                if (length == pos)
                {
                    dateTime = new DateTime(year, month, day, 0, 0, 0, kind);
                    return;
                }

                ch = str[pos];
                if (!(ch == 'T' || ch == ' '))
                {
                    dateTime = new DateTime(year, month, day, 0, 0, 0, kind);
                    return;
                }

                int minute = 0;
                int second = 0;
                int millisecond = 0;

                int hour = 10 * (int)(str[pos + 1] - '0') + (int)(str[pos + 2] - '0');
                pos += 3;

                if (pos < length)
                {
                    ch = str[pos];
                    if (ch == ':')
                    {
                        minute = 10 * (int)(str[pos + 1] - '0') + (int)(str[pos + 2] - '0');
                        pos += 3;

                        if (pos < length)
                        {
                            ch = str[pos];
                            if (ch == ':')
                            {
                                second = 10 * (int)(str[pos + 1] - '0') + (int)(str[pos + 2] - '0');
                                pos += 3;
                            }
                        }
                    }
                    else if (basic)
                    {
                        minute = 10 * (int)(str[pos] - '0') + (int)(str[pos + 1] - '0');
                        pos += 2;

                        if (pos < length)
                        {
                            ch = str[pos];
                            if (ch >= '0' || ch <= '9')
                            {
                                second = 10 * (int)(str[pos] - '0') + (int)(str[pos + 1] - '0');
                                pos += 2;
                            }
                        }
                    }

                    if (pos < length)
                    {
                        ch = str[pos];
                        if (ch == 'Z')
                        {
                            dateTime = new DateTime(year, month, day, hour, minute, second, kind);
                            return;
                        }

                        if (ch == '.')
                        {
                            int msIndex = 0;
                            while (++pos < length)
                            {
                                ch = str[pos];
                                if (ch < '0' || ch > '9')
                                {
                                    break;
                                }

                                msIndex++;
                                if (msIndex < 4)
                                {
                                    millisecond *= 10;
                                    millisecond += (int)(ch - '0');
                                }
                            }
                        }

                        if (pos < length)
                        {
                            ch = str[pos];
                            if (ch == '+' || ch == '-')
                            {
                                int tzHour = 10 * (int)(str[pos + 1] - '0') + (int)(str[pos + 2] - '0');
                                int tzMinute = 0;
                                pos += 3;

                                if (pos < length)
                                {
                                    ch = str[pos];
                                    if (ch == ':')
                                    {
                                        pos++;
                                    }

                                    tzMinute = 10 * (int)(str[pos] - '0') + (int)(str[pos + 1] - '0');
                                }

                                timeSpan = new TimeSpan(tzHour, tzMinute, 0);
                                dateTime = new DateTime(year, month, day, hour, minute,
                                    second, millisecond, DateTimeKind.Unspecified);
                                return;
                            }
                        }
                    }
                }

                dateTime = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            }
            catch (Exception)
            {
                throw new JaysonException(JaysonError.InvalidISO8601DateFormat);
            }
        }

        public static DateTime ParseUnixEpoch(string str)
        {
            if (str == null)
            {
                return default(DateTime);
            }

            int length = str.Length;
            if (length == 0)
            {
                return default(DateTime);
            }

            char ch;
            long l = 0;
            int timeZonePos = -1;
            int timeZoneSign = 1;

            for (int i = 0; i < length; i++)
            {
                ch = str[i];
                if (ch == '-')
                {
                    timeZonePos = i;
                    timeZoneSign = -1;
                    break;
                }

                if (ch == '+')
                {
                    timeZonePos = i;
                    break;
                }

                if (ch < '0' || ch > '9')
                {
                    if (l == 0 && IsWhiteSpace(ch))
                        continue;
                    throw new JaysonException(JaysonError.InvalidJsonDateFormat);
                }

                l *= 10;
                l += (long)(ch - '0');
            }

            if (timeZonePos == -1)
            {
                DateTime dt1 = FromUnixTimeMsec(l);
                if (dt1 > JaysonConstants.DateTimeUnixEpochMaxValue)
                {
                    throw new JaysonException(JaysonError.InvalidJsonDateFormat);
                }
                return dt1;
            }

            if (timeZonePos > length - 5)
            {
                throw new JaysonException(JaysonError.InvalidJsonDateFormat);
            }

            TimeSpan tz = new TimeSpan(10 * (str[timeZonePos + 1] - '0') + (str[timeZonePos + 2] - '0'),
                10 * (str[timeZonePos + 3] - '0') + (str[timeZonePos + 4] - '0'), 0);

            if (timeZoneSign == -1)
            {
                tz = new TimeSpan(-tz.Ticks);
            }

            DateTime dt2 = FromUnixTimeMsec(l, tz);
            if (dt2 > JaysonConstants.DateTimeUnixEpochMaxValue)
            {
                throw new JaysonException(JaysonError.InvalidJsonDateFormat);
            }
            return dt2;
        }

        private static DateTime DefaultDateTime(JaysonDateTimeZoneType timeZoneType)
        {
            switch (timeZoneType)
            {
                case JaysonDateTimeZoneType.ConvertToUtc:
                    return new DateTime(0, DateTimeKind.Utc);
                case JaysonDateTimeZoneType.ConvertToLocal:
                    return new DateTime(0, DateTimeKind.Local);
                default:
                    return default(DateTime);
            }
        }

        public static DateTime TryConvertDateTime(object value, JaysonDateTimeZoneType timeZoneType)
        {
            if (value == null)
            {
                return DefaultDateTime(timeZoneType);
            }

            DateTime dateTime;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return DefaultDateTime(timeZoneType);
                }

                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    dateTime = ParseUnixEpoch(str);
                }
                else
                {
                    dateTime = ParseIso8601DateTime(str, timeZoneType);
                }
            }
            else
            {
                if (value is DateTime)
                {
                    dateTime = (DateTime)value;
                }
                else if (value is DateTime?)
                {
                    dateTime = ((DateTime?)value).Value;
                }
                else if (value is int)
                {
                    dateTime = FromUnixTimeSec((int)value);
                }
                else if (value is long)
                {
                    dateTime = FromUnixTimeSec((long)value);
                }
                else
                {
                    dateTime = Convert.ToDateTime(value);
                }
            }

            switch (timeZoneType)
            {
                case JaysonDateTimeZoneType.ConvertToUtc:
                    return ToUniversalTime(dateTime);
                case JaysonDateTimeZoneType.ConvertToLocal:
                    return ToLocalTime(dateTime);
                default:
                    return dateTime;
            }
        }

        public static DateTime TryConvertDateTime(object value, string dateFormat,
            JaysonDateTimeZoneType timeZoneType)
        {
            if (value == null)
            {
                return DefaultDateTime(timeZoneType);
            }

            DateTime dateTime;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return DefaultDateTime(timeZoneType);
                }

                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    dateTime = ParseUnixEpoch(str);
                }
                else if (String.IsNullOrEmpty(dateFormat))
                {
                    dateTime = ParseIso8601DateTime(str, timeZoneType);
                }
                else
                {
                    DateTimeStyles dtStyle = DateTimeStyles.None;
                    if (EndsWith(str, 'Z'))
                    {
                        dtStyle = DateTimeStyles.AdjustToUniversal;
                    }
                    else if (timeZoneType == JaysonDateTimeZoneType.ConvertToLocal)
                    {
                        dtStyle = DateTimeStyles.AssumeLocal;
                    }

                    if (!DateTime.TryParseExact(str, dateFormat, JaysonConstants.InvariantCulture,
                        dtStyle, out dateTime))
                    {
                        throw new JaysonException(JaysonError.InvalidDateFormat);
                    }
                }
            }
            else
            {
                if (value is DateTime)
                {
                    dateTime = (DateTime)value;
                }
                else if (value is DateTime?)
                {
                    dateTime = ((DateTime?)value).Value;
                }
                else if (value is int)
                {
                    dateTime = FromUnixTimeSec((int)value);
                }
                else if (value is long)
                {
                    dateTime = FromUnixTimeSec((long)value);
                }
                else
                {
                    dateTime = Convert.ToDateTime(value);
                }
            }

            switch (timeZoneType)
            {
                case JaysonDateTimeZoneType.ConvertToUtc:
                    return ToUniversalTime(dateTime);
                case JaysonDateTimeZoneType.ConvertToLocal:
                    return ToLocalTime(dateTime);
                default:
                    return dateTime;
            }
        }

        public static DateTime TryConvertDateTime(object value, string[] dateFormats,
            JaysonDateTimeZoneType timeZoneType)
        {
            if (value == null)
            {
                return DefaultDateTime(timeZoneType);
            }

            DateTime dateTime;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return DefaultDateTime(timeZoneType);
                }

                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    dateTime = ParseUnixEpoch(str);
                }
                else if (dateFormats == null || dateFormats.Length == 0)
                {
                    dateTime = ParseIso8601DateTime(str, timeZoneType);
                }
                else
                {
                    DateTimeStyles dtStyle = DateTimeStyles.None;
                    if (EndsWith(str, 'Z'))
                    {
                        dtStyle = DateTimeStyles.AdjustToUniversal;
                    }
                    else if (timeZoneType == JaysonDateTimeZoneType.ConvertToLocal)
                    {
                        dtStyle = DateTimeStyles.AssumeLocal;
                    }

                    if (!DateTime.TryParseExact(str, dateFormats, JaysonConstants.InvariantCulture,
                        dtStyle, out dateTime))
                    {
                        throw new JaysonException(JaysonError.InvalidDateFormat);
                    }
                }
            }
            else
            {
                if (value is DateTime)
                {
                    dateTime = (DateTime)value;
                }
                else if (value is DateTime?)
                {
                    dateTime = ((DateTime?)value).Value;
                }
                else if (value is int)
                {
                    dateTime = FromUnixTimeSec((int)value);
                }
                else if (value is long)
                {
                    dateTime = FromUnixTimeSec((long)value);
                }
                else
                {
                    dateTime = Convert.ToDateTime(value);
                }
            }

            switch (timeZoneType)
            {
                case JaysonDateTimeZoneType.ConvertToUtc:
                    return ToUniversalTime(dateTime);
                case JaysonDateTimeZoneType.ConvertToLocal:
                    return ToLocalTime(dateTime);
                default:
                    return dateTime;
            }
        }

        public static DateTimeOffset TryConvertDateTimeOffset(object value, out bool converted)
        {
            converted = true;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return default(DateTime);
                }

                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    return new DateTimeOffset(ParseUnixEpoch(str));
                }

                return ParseIso8601DateTimeOffset(str);
            }

            if (value == null)
            {
                return default(DateTimeOffset);
            }

            if (value is DateTimeOffset)
            {
                return (DateTimeOffset)value;
            }

            if (value is DateTime?)
            {
                return ((DateTimeOffset?)value).Value;
            }

            if (value is DateTime)
            {
                return new DateTimeOffset((DateTime)value);
            }

            if (value is DateTime?)
            {
                return new DateTimeOffset(((DateTime?)value).Value);
            }

            if (value is int)
            {
                return new DateTimeOffset(FromUnixTimeSec((int)value));
            }

            if (value is long)
            {
                return new DateTimeOffset(FromUnixTimeSec((long)value));
            }

            return new DateTimeOffset(Convert.ToDateTime(value));
        }

        public static DateTimeOffset TryConvertDateTimeOffset(object value, string dateFormat, out bool converted)
        {
            converted = true;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return default(DateTime);
                }
                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    return new DateTimeOffset(ParseUnixEpoch(str));
                }

                if (String.IsNullOrEmpty(dateFormat))
                {
                    return ParseIso8601DateTimeOffset(str);
                }

                DateTimeStyles dtStyle = DateTimeStyles.None;
                if (EndsWith(str, 'Z'))
                {
                    dtStyle = DateTimeStyles.AdjustToUniversal;
                }

                DateTimeOffset result;
                converted = DateTimeOffset.TryParseExact(str,
                    !String.IsNullOrEmpty(dateFormat) ? dateFormat : JaysonConstants.DateIso8601Format,
                    JaysonConstants.InvariantCulture, dtStyle, out result);
                return result;
            }

            if (value == null)
            {
                return default(DateTimeOffset);
            }

            if (value is DateTimeOffset)
            {
                return (DateTimeOffset)value;
            }

            if (value is DateTimeOffset?)
            {
                return ((DateTimeOffset?)value).Value;
            }

            if (value is DateTime)
            {
                return new DateTimeOffset((DateTime)value);
            }

            if (value is DateTime?)
            {
                return new DateTimeOffset(((DateTime?)value).Value);
            }

            if (value is int)
            {
                return new DateTimeOffset(FromUnixTimeSec((int)value));
            }

            if (value is long)
            {
                return new DateTimeOffset(FromUnixTimeSec((long)value));
            }

            return new DateTimeOffset(Convert.ToDateTime(value));
        }

        public static DateTimeOffset TryConvertDateTimeOffset(object value, string[] dateFormats, out bool converted)
        {
            converted = true;

            string str = value as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    return default(DateTime);
                }
                if (StartsWith(str, JaysonConstants.MicrosoftDateFormatStart) &&
                    EndsWith(str, JaysonConstants.MicrosoftDateFormatEnd))
                {
                    str = str.Substring(JaysonConstants.MicrosoftDateFormatStartLen,
                        str.Length - JaysonConstants.MicrosoftDateFormatLen);
                    return new DateTimeOffset(ParseUnixEpoch(str));
                }

                if (dateFormats == null || dateFormats.Length == 0)
                {
                    return ParseIso8601DateTimeOffset(str);
                }

                DateTimeStyles dtStyle = DateTimeStyles.None;
                if (EndsWith(str, 'Z'))
                {
                    dtStyle = DateTimeStyles.AdjustToUniversal;
                }

                DateTimeOffset result;
                converted = DateTimeOffset.TryParseExact(str, dateFormats,
                    JaysonConstants.InvariantCulture, dtStyle, out result);
                return result;
            }

            if (value == null)
            {
                return default(DateTimeOffset);
            }

            if (value is DateTimeOffset)
            {
                return (DateTimeOffset)value;
            }

            if (value is DateTimeOffset?)
            {
                return ((DateTimeOffset?)value).Value;
            }

            if (value is DateTime)
            {
                return new DateTimeOffset((DateTime)value);
            }

            if (value is DateTime?)
            {
                return new DateTimeOffset(((DateTime?)value).Value);
            }

            if (value is int)
            {
                return new DateTimeOffset(FromUnixTimeSec((int)value));
            }

            if (value is long)
            {
                return new DateTimeOffset(FromUnixTimeSec((long)value));
            }

            return new DateTimeOffset(Convert.ToDateTime(value));
        }

        # endregion DateTime Methods

        # region String Methods

        public static bool StartsWith(string str1, string str2)
        {
            if (str1 != null && str2 != null)
            {
                int length2 = str2.Length;
                if (str1.Length >= length2)
                {
                    for (int i = 0; i < length2; i++)
                    {
                        if (str1[i] != str2[i])
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool StartsWith(string str, char ch)
        {
            return !String.IsNullOrEmpty(str) && str[0] == ch;
        }

        public static bool EndsWith(string str1, string str2)
        {
            if (str1 != null && str2 != null)
            {
                int length2 = str2.Length;
                int offset = str1.Length - length2;

                if (offset == 0)
                {
                    for (int i = 0; i < length2; i++)
                    {
                        if (str1[i] != str2[i])
                            return false;
                    }
                    return true;
                }
                else if (offset > 0)
                {
                    for (int i = length2 - 1; i > -1; i--)
                    {
                        if (str1[offset + i] != str2[i])
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool EndsWith(string str, char ch)
        {
            if (str != null)
            {
                int length = str.Length;
                if (length > 0)
                {
                    return str[length - 1] == ch;
                }
            }
            return false;
        }

        public static string AsciiToLower(string asciiStr)
        {
            if (asciiStr == null)
            {
                return asciiStr;
            }

            int length = asciiStr.Length;
            if (length == 0)
            {
                return asciiStr;
            }

            char ch;
            int start = 0;
            char[] chArray = null;

            for (int i = 0; i < length; i++)
            {
                ch = asciiStr[i];
                if (ch >= 'A' && ch <= 'Z')
                {
                    if (chArray == null)
                    {
                        chArray = new char[length];
                    }

                    if (i > start)
                    {
                        for (var j = start; j < i; j++)
                        {
                            chArray[j] = asciiStr[j];
                        }
                    }
                    chArray[i] = (char)((int)ch + LowerCaseDif);
                    start = i + 1;
                }
            }

            if (start == 0)
            {
                return asciiStr;
            }

            if (start < length)
            {
                if (chArray == null)
                {
                    chArray = new char[length];
                }

                for (int i = start; i < length; i++)
                {
                    chArray[i] = asciiStr[i];
                }
            }
            return new string(chArray);
        }

        public static string AsciiToUpper(string asciiStr)
        {
            if (asciiStr == null)
            {
                return asciiStr;
            }

            int length = asciiStr.Length;
            if (length == 0)
            {
                return asciiStr;
            }

            char ch;
            int start = 0;
            char[] chArray = null;

            for (int i = 0; i < length; i++)
            {
                ch = asciiStr[i];
                if (ch >= 'a' && ch <= 'z')
                {
                    if (chArray == null)
                    {
                        chArray = new char[length];
                    }

                    if (i > start)
                    {
                        for (var j = start; j < i; j++)
                        {
                            chArray[j] = asciiStr[j];
                        }
                    }
                    chArray[i] = (char)((int)ch - LowerCaseDif);
                    start = i + 1;
                }
            }

            if (start == 0)
            {
                return asciiStr;
            }

            if (start < length)
            {
                if (chArray == null)
                {
                    chArray = new char[length];
                }

                for (int i = start; i < length; i++)
                {
                    chArray[i] = asciiStr[i];
                }
            }
            return new string(chArray);
        }

        public static bool ParseBoolean(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                char ch;
                int pos = 0;
                int length = str.Length;

                while (pos < length)
                {
                    ch = str[pos];

                    if (IsWhiteSpace(ch))
                    {
                        pos++;
                        continue;
                    }

                    if (ch == 't' || ch == 'T')
                    {
                        if (pos < length - 3 &&
                            (str[++pos] == 'r' || str[pos] == 'R') &&
                            (str[++pos] == 'u' || str[pos] == 'U') &&
                            (str[++pos] == 'e' || str[pos] == 'E'))
                        {
                            if (++pos < length)
                            {
                                do
                                {
                                    if (!IsWhiteSpace(str[pos++]))
                                    {
                                        throw new JaysonException(JaysonError.InvalidBooleanString);
                                    }
                                } while (pos < length);
                            }
                            return true;
                        }
                        throw new JaysonException(JaysonError.InvalidBooleanString);
                    }

                    if (ch == 'f' || ch == 'F')
                    {
                        if (pos < length - 4 &&
                            (str[++pos] == 'a' || str[pos] == 'A') &&
                            (str[++pos] == 'l' || str[pos] == 'L') &&
                            (str[++pos] == 's' || str[pos] == 'S') &&
                            (str[++pos] == 'e' || str[pos] == 'E'))
                        {
                            if (++pos < length)
                            {
                                do
                                {
                                    if (!IsWhiteSpace(str[pos++]))
                                    {
                                        throw new JaysonException(JaysonError.InvalidBooleanString);
                                    }
                                } while (pos < length);
                            }
                            return false;
                        }
                        throw new JaysonException(JaysonError.InvalidBooleanString);
                    }

                    throw new JaysonException(JaysonError.InvalidBooleanString);
                }
            }
            throw new JaysonException(JaysonError.InvalidBooleanString);
        }

        # endregion String Methods

        public static bool IsOnMono()
        {
            if (s_IsMono == -1)
            {
                s_IsMono = Type.GetType("Mono.Runtime", false) != null ? 1 : 0;
            }
            return (s_IsMono == 1);
        }

        public static object EnumToObject(Type enumType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var jtc = JaysonTypeInfo.GetJTypeCode(value.GetType());
            switch (jtc)
            {
                case JaysonTypeCode.Long:
                    {
                        return Enum.ToObject(enumType, (long)value);
                    }
                case JaysonTypeCode.Int:
                    {
                        return Enum.ToObject(enumType, (long)((int)value));
                    }
                case JaysonTypeCode.Short:
                    {
                        return Enum.ToObject(enumType, (long)((short)value));
                    }
                case JaysonTypeCode.Byte:
                    {
                        return Enum.ToObject(enumType, (long)((byte)value));
                    }
                case JaysonTypeCode.ULong:
                    {
                        return Enum.ToObject(enumType, (long)((ulong)value));
                    }
                case JaysonTypeCode.UInt:
                    {
                        return Enum.ToObject(enumType, (long)((uint)value));
                    }
                case JaysonTypeCode.UShort:
                    {
                        return Enum.ToObject(enumType, (long)((ushort)value));
                    }
                case JaysonTypeCode.SByte:
                    {
                        return Enum.ToObject(enumType, (long)((sbyte)value));
                    }
                case JaysonTypeCode.Char:
                    {
                        return Enum.ToObject(enumType, (long)((char)value));
                    }
                case JaysonTypeCode.Double:
                    {
                        return Enum.ToObject(enumType, (long)((double)value));
                    }
                case JaysonTypeCode.Float:
                    {
                        return Enum.ToObject(enumType, (long)((float)value));
                    }
                case JaysonTypeCode.LongNullable:
                    {
                        return Enum.ToObject(enumType, ((long?)value).Value);
                    }
                case JaysonTypeCode.IntNullable:
                    {
                        return Enum.ToObject(enumType, (long)((int?)value).Value);
                    }
                case JaysonTypeCode.ShortNullable:
                    {
                        return Enum.ToObject(enumType, (long)((short?)value).Value);
                    }
                case JaysonTypeCode.ByteNullable:
                    {
                        return Enum.ToObject(enumType, (long)((byte?)value).Value);
                    }
                case JaysonTypeCode.ULongNullable:
                    {
                        return Enum.ToObject(enumType, (long)((ulong?)value).Value);
                    }
                case JaysonTypeCode.UIntNullable:
                    {
                        return Enum.ToObject(enumType, (long)((uint?)value).Value);
                    }
                case JaysonTypeCode.UShortNullable:
                    {
                        return Enum.ToObject(enumType, (long)((ushort?)value).Value);
                    }
                case JaysonTypeCode.SByteNullable:
                    {
                        return Enum.ToObject(enumType, (long)((sbyte?)value).Value);
                    }
                case JaysonTypeCode.CharNullable:
                    {
                        return Enum.ToObject(enumType, (long)((char?)value).Value);
                    }
                case JaysonTypeCode.BoolNullable:
                    {
                        return Enum.ToObject(enumType, ((bool?)value).Value ? 1L : 0L);
                    }
                case JaysonTypeCode.DoubleNullable:
                    {
                        return Enum.ToObject(enumType, (long)((double?)value).Value);
                    }
                case JaysonTypeCode.FloatNullable:
                    {
                        return Enum.ToObject(enumType, (long)((float?)value).Value);
                    }
                default:
                    throw new JaysonException(JaysonError.ArgumentMustBeEnum);
            }
        }

        private static MethodInfo GetOverridingMethod(Type overridingType, string overridingMethod)
        {
            if (overridingType != null)
            {
                return overridingType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                        .Where((mi) => mi.ReturnType == typeof(object))
                                        .Where((mi) => mi.Name.Equals(overridingMethod, StringComparison.OrdinalIgnoreCase))
                                        .Where((mi) =>
                                        {
                                            var prms = mi.GetParameters();
                                            return (prms != null) && (prms.Length == 2) &&
                                                (prms[0].ParameterType == typeof(string)) &&
                                                (prms[1].ParameterType == typeof(object));
                                        })
                                        .FirstOrDefault();
            }
            return null;
        }

        public static MethodInfo GetOverridingMethod(MemberInfo memberInf)
        {
            if (memberInf != null)
            {
                var oAttr = memberInf.GetCustomAttributes(typeof(JaysonMemberOverrideAttribute), true)
                    .Cast<JaysonMemberOverrideAttribute>()
                    .Where((attr) => !String.IsNullOrEmpty(attr.OverridingMethod))
                    .FirstOrDefault();

                if (oAttr != null)
                {
                    if (!String.IsNullOrEmpty(oAttr.OverridingType))
                    {
                        return GetOverridingMethod(GetType(oAttr.OverridingType), oAttr.OverridingMethod);
                    }

                    return GetOverridingMethod(memberInf.DeclaringType, oAttr.OverridingMethod);
                }
            }
            return null;
        }

        public static string GetAssemblyName(Assembly asm)
        {
            if (asm != null)
                return s_AssemblyNameCache.GetValueOrUpdate(asm, (a) => a.GetName().Name);
            return null;
        }

        public static Assembly FindAssembly(string assemblyName)
        {
            if (!String.IsNullOrEmpty(assemblyName))
            {
                return s_AssemblyCache.GetValueOrUpdate(assemblyName, (an) =>
                    {
                        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (GetAssemblyName(asm).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                                return asm;
                        }
                        return null;
                    });
            }
            return null;
        }

        public static Type GetType(string typeName, SerializationBinder binder = null)
        {
            Type result = null;
            if (!String.IsNullOrEmpty(typeName))
            {
                if (binder != null)
                {
                    string[] typeParts = typeName.Split(',');
                    if (typeParts.Length > 1)
                    {
                        result = binder.BindToType(typeParts[0], typeParts[1]);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }

                if (!s_TypeCache.TryGetValue(typeName, out result))
                {
                    result = Type.GetType(typeName, false, true);
                    if ((result == null) && !typeName.Contains('['))
                    {
                        var asmNameSepPos = typeName.IndexOf(',');
                        if (asmNameSepPos == -1)
                        {
                            var pos = typeName.IndexOf('.');
                            if (pos > -1)
                            {
                                var assemblyNameStart = typeName.Substring(0, pos + 1);
                                if (!String.IsNullOrEmpty(assemblyNameStart))
                                {
                                    Type typeRef;

                                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                                    {
                                        if (GetAssemblyName(asm).StartsWith(assemblyNameStart, StringComparison.OrdinalIgnoreCase))
                                        {
                                            typeRef = asm.GetType(typeName, false, true);
                                            if (typeRef != null)
                                            {
                                                s_TypeCache[typeName] = typeRef;
                                                return typeRef;
                                            }
                                        }
                                    }
                                }
                            }

                            s_TypeCache[typeName] = null;
                            return (Type)null;
                        }

                        #if (NET3500 || NET3000 || NET2000)
                        var assemblyName = (typeName.Substring(asmNameSepPos + 1, typeName.Length - asmNameSepPos - 1) ?? String.Empty).Trim();
                        if (!String.IsNullOrEmpty(assemblyName))
                        {
                            typeName = (typeName.Substring(0, asmNameSepPos) ?? String.Empty).Trim();
                            if (!String.IsNullOrEmpty(typeName))
                            {
                                var assembly = FindAssembly(assemblyName);

                                if (assembly == null)
                                {
                                    if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                    {
                                        assemblyName += ".dll";
                                    }
                                    assembly = Assembly.Load(assemblyName);
                                }

                                if (assembly != null)
                                {
                                    result = assembly.GetType(typeName, false, true);
                                    s_TypeCache[typeName] = result;
                                }
                            }
                        }
                        #else
                        result = Type.GetType(typeName,
                            (assemblyRef) =>
                                {
                                    if (assemblyRef != null)
                                    {
                                        var assemblyName = assemblyRef.Name;
                                        if (!String.IsNullOrEmpty(assemblyName))
                                        {
                                            try
                                            {
                                                var assembly = FindAssembly(assemblyName);
                                                if (assembly == null)
                                                {
                                                    try
                                                    {
                                                        assembly = Assembly.Load(assemblyRef);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        var assemblyFile = assemblyName;
                                                        if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            assemblyFile += ".dll";
                                                        }

                                                        assembly = Assembly.Load(assemblyFile);
                                                    }

                                                    if (assembly != null)
                                                    {
                                                        s_AssemblyCache[assemblyName] = assembly;
                                                        s_AssemblyNameCache[assembly] = assemblyName;
                                                    }
                                                }
                                                return assembly;
                                            }
                                            catch (Exception)
                                            { }
                                        }
                                    }
                                    return (Assembly)null;
                                },
                            (assembly, name, ignoreCase) =>
                                {
                                    if (assembly != null)
                                    {
                                        return assembly.GetType(name, false, ignoreCase);
                                    }

                                    var pos = name.IndexOf('.');
                                    if (pos > -1)
                                    {
                                        var assemblyNameStart = name.Substring(0, pos + 1);
                                        if (!String.IsNullOrEmpty(assemblyNameStart))
                                        {
                                            Type typeRef;

                                            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                                            {
                                                if (GetAssemblyName(asm).StartsWith(assemblyNameStart, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    typeRef = asm.GetType(name, false, ignoreCase);
                                                    if (typeRef != null)
                                                    {                                                        
                                                        return typeRef;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    return (Type)null;
                                });
#endif
                    }

                    s_TypeCache[typeName] = result;
                }
            }

            return result;
        }

        public static bool IsNaN(object obj)
        {
            return (obj != null) && (double.NaN.Equals(obj) || float.NaN.Equals(obj));
        }

        public static bool IsInfinity(object obj)
        {
            return (obj != null) && JaysonConstants.InfinityValues.Any(d => Object.Equals(d, obj));
        }

        public static object ConvertToPrimitive(object value, Type toPrimitiveType, out bool converted)
        {
            var info = JaysonTypeInfo.GetTypeInfo(toPrimitiveType);

            // Do not change the type check order
            converted = true;
            switch (info.JTypeCode)
            {
                case JaysonTypeCode.Int:
                    {
                        return ToInt(value);
                    }
                case JaysonTypeCode.Bool:
                    {
                        return ToBoolean(value);
                    }
                case JaysonTypeCode.Long:
                    {
                        return ToLong(value);
                    }
                case JaysonTypeCode.Double:
                    {
                        return ToDouble(value);
                    }
                case JaysonTypeCode.DateTime:
                    {
                        return TryConvertDateTime(value, JaysonDateTimeZoneType.KeepAsIs);
                    }
                case JaysonTypeCode.Short:
                    {
                        return ToShort(value);
                    }
                case JaysonTypeCode.IntNullable:
                    {
                        return ToIntNullable(value);
                    }
                case JaysonTypeCode.BoolNullable:
                    {
                        return ToBooleanNullable(value);
                    }
                case JaysonTypeCode.LongNullable:
                    {
                        return ToLongNullable(value);
                    }
                case JaysonTypeCode.DoubleNullable:
                    {
                        return ToDoubleNullable(value);
                    }
                case JaysonTypeCode.DateTimeNullable:
                    {
                        return ToDateTimeNullable(value);
                    }
                case JaysonTypeCode.ShortNullable:
                    {
                        return ToShortNullable(value);
                    }
                case JaysonTypeCode.Float:
                    {
                        return ToFloat(value);
                    }
                case JaysonTypeCode.Decimal:
                    {
                        return ToDecimal(value);
                    }
                case JaysonTypeCode.Byte:
                    {
                        return ToByte(value);
                    }
                case JaysonTypeCode.Guid:
                    {
                        return ToGuid(value);
                    }
                case JaysonTypeCode.Char:
                    {
                        return ToChar(value);
                    }
                case JaysonTypeCode.TimeSpan:
                    {
                        return ToTimeSpan(value);
                    }
                case JaysonTypeCode.FloatNullable:
                    {
                        return ToFloatNullable(value);
                    }
                case JaysonTypeCode.DecimalNullable:
                    {
                        return ToDecimalNullable(value);
                    }
                case JaysonTypeCode.ByteNullable:
                    {
                        return ToByteNullable(value);
                    }
                case JaysonTypeCode.GuidNullable:
                    {
                        return ToGuidNullable(value);
                    }
                case JaysonTypeCode.CharNullable:
                    {
                        return ToCharNullable(value);
                    }
                case JaysonTypeCode.TimeSpanNullable:
                    {
                        return ToTimeSpanNullable(value);
                    }
                case JaysonTypeCode.UInt:
                    {
                        return ToUInt(value);
                    }
                case JaysonTypeCode.ULong:
                    {
                        return ToULong(value);
                    }
                case JaysonTypeCode.UShort:
                    {
                        return ToUShort(value);
                    }
                case JaysonTypeCode.SByte:
                    {
                        return ToSByte(value);
                    }
                case JaysonTypeCode.UIntNullable:
                    {
                        return ToUIntNullable(value);
                    }
                case JaysonTypeCode.ULongNullable:
                    {
                        return ToULongNullable(value);
                    }
                case JaysonTypeCode.UShortNullable:
                    {
                        return ToUShortNullable(value);
                    }
                case JaysonTypeCode.SByteNullable:
                    {
                        return ToSByteNullable(value);
                    }
                case JaysonTypeCode.DateTimeOffset:
                    {
                        return TryConvertDateTimeOffset(value, out converted);
                    }
                case JaysonTypeCode.DateTimeOffsetNullable:
                    {
                        return ToDateTimeOffsetNullable(value, ref converted);
                    }
                default:
                    {
                        converted = false;

                        if (info.Nullable)
                        {
                            var uType = info.UnderlyingType;
                            if (uType != null)
                            {
                                var uInfo = JaysonTypeInfo.GetTypeInfo(uType);
                                if (uInfo.Enum)
                                {
                                    converted = true;
                                    object result = ToEnum(value, uType);
                                    return JaysonNullable.New(uType, result);
                                }
                            }
                        }

                        if (info.Enum)
                        {
                            converted = true;
                            return ToEnum(value, toPrimitiveType);
                        }
                        break;
                    }
            }

            return value;
        }

        private static object ToEnum(object value, Type toPrimitiveType)
        {
            if (value == null)
            {
                return Enum.ToObject(toPrimitiveType, 0L);
            }

            if (value is int)
            {
                return Enum.ToObject(toPrimitiveType, (int)value);
            }
            if (value is long)
            {
                return Enum.ToObject(toPrimitiveType, (long)value);
            }
            if (value is short)
            {
                return Enum.ToObject(toPrimitiveType, (short)value);
            }
            if (value is byte)
            {
                return Enum.ToObject(toPrimitiveType, (byte)value);
            }
            if (value is uint)
            {
                return Enum.ToObject(toPrimitiveType, (uint)value);
            }
            if (value is ulong)
            {
                return Enum.ToObject(toPrimitiveType, (ulong)value);
            }
            if (value is ushort)
            {
                return Enum.ToObject(toPrimitiveType, (ushort)value);
            }
            if (value is sbyte)
            {
                return Enum.ToObject(toPrimitiveType, (sbyte)value);
            }

            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return Enum.ToObject(toPrimitiveType, 0L);
                }
                return JaysonEnumCache.Parse(s, toPrimitiveType);
            }

            return Enum.ToObject(toPrimitiveType, Convert.ToInt64(value));
        }

        private static object ToDateTimeNullable(object value)
        {
            var dt = TryConvertDateTime(value, JaysonDateTimeZoneType.KeepAsIs);
            if (dt == default(DateTime))
            {
                return null;
            }
            return (DateTime?)dt;
        }

        private static object ToByteNullable(object value)
        {
            if (value == null)
            {
                return (byte?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (byte?)null;
                }
                return (byte?)byte.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (byte?)Convert.ToByte(value);
        }

        private static object ToDateTimeOffsetNullable(object value, ref bool converted)
        {
            if (value == null)
            {
                return (DateTimeOffset?)value;
            }
            var dto = TryConvertDateTimeOffset(value, out converted);
            if (dto == default(DateTimeOffset))
            {
                return null;
            }
            return dto;
        }

        private static object ToSByteNullable(object value)
        {
            if (value == null)
            {
                return (sbyte?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (sbyte?)null;
                }
                return (sbyte?)sbyte.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (sbyte?)Convert.ToSByte(value);
        }

        private static object ToUShortNullable(object value)
        {
            if (value == null)
            {
                return (ushort?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (ushort?)null;
                }
                return (ushort?)ushort.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (ushort?)Convert.ToUInt16(value);
        }

        private static object ToULongNullable(object value)
        {
            if (value == null)
            {
                return (ulong?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (ulong?)null;
                }
                return (ulong?)ulong.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (ulong?)Convert.ToUInt64(value);
        }

        private static object ToUIntNullable(object value)
        {
            if (value == null)
            {
                return (uint?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (uint?)null;
                }
                return (uint?)uint.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (uint?)Convert.ToUInt32(value);
        }

        private static object ToSByte(object value)
        {
            if (value == null)
            {
                return (sbyte)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (sbyte)0;
                }
                return sbyte.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToSByte(value);
        }

        private static object ToUShort(object value)
        {
            if (value == null)
            {
                return (ushort)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (ushort)0;
                }
                return ushort.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToUInt16(value);
        }

        private static object ToULong(object value)
        {
            if (value == null)
            {
                return (ulong)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (ulong)0;
                }
                return ulong.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToUInt64(value);
        }

        private static object ToUInt(object value)
        {
            if (value == null)
            {
                return (uint)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (uint)0;
                }
                return uint.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToUInt32(value);
        }

        private static object ToCharNullable(object value)
        {
            if (value == null)
            {
                return (char?)null;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (char?)null;
                }
                return (char?)s[0];
            }
            return Convert.ToChar(value);
        }

        private static object ToTimeSpanNullable(object value)
        {
            if (value is TimeSpan)
            {
                return (TimeSpan?)((TimeSpan)value);
            }
            if (value == null)
            {
                return (TimeSpan?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (TimeSpan?)null;
                }
#if !(NET3500 || NET3000 || NET2000)
                return (TimeSpan?)TimeSpan.Parse(s, JaysonConstants.InvariantCulture);
#else
						return (TimeSpan?)TimeSpan.Parse(s);
#endif
            }
            return (TimeSpan?)(new TimeSpan(Convert.ToInt64(value)));
        }

        private static object ToGuidNullable(object value)
        {
            if (value is Guid)
            {
                return (Guid?)((Guid)value);
            }
            if (value == null)
            {
                return (Guid?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return default(Guid?);
                }
                if (s[0] == '!')
                {
                    return (Guid?)(new Guid(Convert.FromBase64String(s.Substring(1))));
                }
#if (NET3500 || NET3000 || NET2000)
						return (Guid?)(new Guid(s));
#else
                return (Guid?)Guid.Parse(s);
#endif
            }
            if (value is byte[])
            {
                return (Guid?)(new Guid((byte[])value));
            }
#if (NET3500 || NET3000 || NET2000)
					return (Guid?)(new Guid(value.ToString()));
#else
            return (Guid?)Guid.Parse(value.ToString());
#endif
        }

        private static object ToDecimalNullable(object value)
        {
            if (value is decimal)
            {
                return (decimal?)((decimal)value);
            }
            if (value is double)
            {
                return (decimal?)Convert.ToDecimal((double)value);
            }
            if (value == null)
            {
                return (decimal?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (decimal?)null;
                }
                return (decimal?)decimal.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
            }
            return (decimal?)Convert.ToDecimal(value);
        }

        private static object ToFloatNullable(object value)
        {
            if (value == null)
            {
                return (float?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (float?)null;
                }
#if !(NET3500 || NET3000 || NET2000)
                float result;
                if (float.TryParse(s, NumberStyles.Float, JaysonConstants.InvariantCulture, out result))
                {
                    return (float?)result;
                }
                if (s == "∞")
                {
                    return (float?)float.PositiveInfinity;
                }
                if (s == "-∞")
                {
                    return (float?)float.NegativeInfinity;
                }
#else
                return (float?)float.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
#endif
            }
            return (float?)Convert.ToSingle(value);
        }

        private static object ToTimeSpan(object value)
        {
            if (value is TimeSpan)
            {
                return value;
            }
            if (value == null)
            {
                return default(TimeSpan);
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return default(TimeSpan);
                }
#if !(NET3500 || NET3000 || NET2000)
                return TimeSpan.Parse(s, JaysonConstants.InvariantCulture);
#else
						return TimeSpan.Parse(s);
#endif
            }
            return new TimeSpan(Convert.ToInt64(value));
        }

        private static object ToChar(object value)
        {
            if (value == null)
            {
                return (char)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (char)0;
                }
                return s[0];
            }
            return Convert.ToChar(value);
        }

        private static object ToGuid(object value)
        {
            if (value is Guid)
            {
                return value;
            }
            if (value == null)
            {
                return default(Guid);
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return default(Guid);
                }
                if (s[0] == '!')
                {
                    return new Guid(Convert.FromBase64String(s.Substring(1)));
                }
#if (NET3500 || NET3000 || NET2000)
						return new Guid(s);
#else
                return Guid.Parse(s);
#endif
            }
            if (value is byte[])
            {
                return new Guid((byte[])value);
            }
#if (NET3500 || NET3000 || NET2000)
					return new Guid(value.ToString());
#else
            return Guid.Parse(value.ToString());
#endif
        }

        private static object ToByte(object value)
        {
            if (value == null)
            {
                return (byte)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (byte)0;
                }
                return byte.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToByte(value);
        }

        private static object ToDecimal(object value)
        {
            if (value is decimal)
            {
                return value;
            }
            if (value is double)
            {
                return Convert.ToDecimal((double)value);
            }
            if (value == null)
            {
                return 0m;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return 0m;
                }
                return decimal.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
            }
            return Convert.ToDecimal(value);
        }

        private static object ToFloat(object value)
        {
            if (value == null)
            {
                return 0f;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return 0f;
                }
#if !(NET3500 || NET3000 || NET2000)
                float result;
                if (float.TryParse(s, NumberStyles.Float, JaysonConstants.InvariantCulture, out result))
                {
                    return result;
                }
                if (s == "∞")
                {
                    return float.PositiveInfinity;
                }
                if (s == "-∞")
                {
                    return float.NegativeInfinity;
                }
#else
                return float.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
#endif
            }
            return Convert.ToSingle(value);
        }

        private static object ToShortNullable(object value)
        {
            if (value == null)
            {
                return (short?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (short?)null;
                }
                return (short?)short.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (short?)Convert.ToInt16(value);
        }

        private static object ToDoubleNullable(object value)
        {
            if (value == null)
            {
                return (double?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (double?)null;
                }
#if !(NET3500 || NET3000 || NET2000)
                double result;
                if (double.TryParse(s, NumberStyles.Float, JaysonConstants.InvariantCulture, out result))
                {
                    return (double?)result;
                }
                if (s == "∞")
                {
                    return (double?)double.PositiveInfinity;
                }
                if (s == "-∞")
                {
                    return (double?)double.NegativeInfinity;
                }
#else
                return (double?)decimal.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
#endif
            }
            return (double?)Convert.ToDouble(value);
        }

        private static object ToLongNullable(object value)
        {
            if (value == null)
            {
                return (long?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (long?)null;
                }
                return (long?)long.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (long?)Convert.ToInt64(value);
        }

        private static object ToBooleanNullable(object value)
        {
            if (value == null)
            {
                return (bool?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (bool?)null;
                }
                return (bool?)ParseBoolean(s);
            }
            return (bool?)Convert.ToBoolean(value);
        }

        private static object ToIntNullable(object value)
        {
            if (value == null)
            {
                return (int?)value;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (int?)null;
                }
                return (int?)int.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return (int?)Convert.ToInt32(value);
        }

        private static object ToShort(object value)
        {
            if (value == null)
            {
                return (short)0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return (short)0;
                }
                return short.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToInt16(value);
        }

        private static object ToDouble(object value)
        {
            if (value == null)
            {
                return 0d;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return 0d;
                }
#if !(NET3500 || NET3000 || NET2000)
                double result;
                if (double.TryParse(s, NumberStyles.Float, JaysonConstants.InvariantCulture, out result))
                {
                    return result;
                }
                if (s == "∞")
                {
                    return double.PositiveInfinity;
                }
                if (s == "-∞")
                {
                    return double.NegativeInfinity;
                }
#else
                return double.Parse(s, NumberStyles.Float, JaysonConstants.InvariantCulture);
#endif
            }
            return Convert.ToDouble(value);
        }

        private static object ToLong(object value)
        {
            if (value is long)
            {
                return value;
            }
            if (value == null)
            {
                return 0L;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return 0L;
                }
                return long.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToInt64(value);
        }

        private static object ToBoolean(object value)
        {
            if (value is bool)
            {
                return value;
            }
            if (value == null)
            {
                return false;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return false;
                }
                return ParseBoolean(s);
            }
            return Convert.ToBoolean(value);
        }

        private static object ToInt(object value)
        {
            if (value is int)
            {
                return value;
            }
            if (value == null)
            {
                return 0;
            }
            if (value is string)
            {
                var s = (string)value;
                if (s.Length == 0)
                {
                    return 0;
                }
                return int.Parse(s, NumberStyles.Integer, JaysonConstants.InvariantCulture);
            }
            return Convert.ToInt32(value);
        }

        public static bool IsWhiteSpaceChar(char ch)
        {
            return IsWhiteSpace(ch);
        }

        public static bool IsWhiteSpace(int ch)
        {
            if (ch != 32 && (ch < 9 || ch > 13) && ch != 160)
                return ch == 133;
            return true;
        }

        internal static bool FindInterface(Type objType, Type interfaceType, out Type[] arguments)
        {
            arguments = null;
            if (objType.IsGenericType)
            {
                JaysonTypeInfo iInfo;
                var iTypes = JaysonTypeInfo.GetInterfaces(objType);

                for (int i = iTypes.Length - 1; i > -1; i--)
                {
                    iInfo = JaysonTypeInfo.GetTypeInfo(iTypes[i]);
                    if (iInfo.Type == interfaceType ||
                        (iInfo.Generic && iInfo.GenericTypeDefinitionType == interfaceType))
                    {
                        arguments = iInfo.GenericArguments;
                        if (arguments.Length == 0)
                        {
                            arguments = null;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsGenericCollection(Type objType)
        {
            return s_IsGenericCollection.GetValueOrUpdate(objType, (t) => 
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(ICollection<>), out arguments);

                    s_GenericCollectionArgs[t] = found ? arguments[0] : null;

                    return found;
                });
        }

#if !(NET3500 || NET3000 || NET2000)
        internal static bool IsProducerConsumerCollection(Type objType)
        {
            return s_IsProducerConsumerCollection.GetValueOrUpdate(objType, (t) =>
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(IProducerConsumerCollection<>), out arguments);

                    s_ProducerConsumerCollectionArgs[t] = found ? arguments[0] : null;

                    return found;
                });
        }
#endif

        internal static bool IsGenericDictionary(Type objType)
        {
            return s_IsGenericDictionary.GetValueOrUpdate(objType, (t) =>
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(IDictionary<,>), out arguments);

                    s_GenericDictionaryArgs[t] = arguments;

                    return found;
                });
        }

        internal static bool IsGenericList(Type objType)
        {
            return s_IsGenericList.GetValueOrUpdate(objType, (t) =>
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(IList<>), out arguments);

                    s_GenericListArgs[t] = found ? arguments[0] : null;

                    return found;
                });
        }

        internal static Type GetGenericCollectionArgs(Type objType)
        {
            return s_GenericCollectionArgs.GetValueOrUpdate(objType, (t) => 
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(ICollection<>), out arguments);

                    s_IsGenericCollection[t] = found;
                
                    return found ? arguments[0] : null;
                });
        }

#if !(NET3500 || NET3000 || NET2000)
        internal static Type GetProducerConsumerCollectionArgs(Type objType)
        {
            return s_ProducerConsumerCollectionArgs.GetValueOrUpdate(objType, (t) =>
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(IProducerConsumerCollection<>), out arguments);

                    s_IsProducerConsumerCollection[t] = found;

                    return found ? arguments[0] : null;

                });
        }
#endif

        internal static Type[] GetGenericDictionaryArgs(Type objType)
        {
            return s_GenericDictionaryArgs.GetValueOrUpdate(objType, (t) => 
                {
                    Type[] arguments;
                    s_IsGenericDictionary[t] = FindInterface(t, typeof(IDictionary<,>), out arguments);

                    return arguments;
                });
        }

        internal static Type GetGenericListArgs(Type objType)
        {
            return s_GenericListArgs.GetValueOrUpdate(objType, (t) =>
                {
                    Type[] arguments;
                    var found = FindInterface(t, typeof(IList<>), out arguments);

                    s_IsGenericList[t] = found;

                    return found ? arguments[0] : null;
                });
        }

        internal static bool StackContains(ArrayList stack, object obj)
        {
            for (int i = stack.Count - 1; i > -1; i--)
            {
                if (obj == stack[i])
                    return true;
            }
            return false;
        }

        public static Func<object[], object> CreateActivator(ConstructorInfo ctor)
        {
            var declaringT = ctor.DeclaringType;
            var ctorParams = ctor.GetParameters();

            // Create a single param of type object[]
            var paramExp = Expression.Parameter(typeof(object[]), "args");

            int length = ctorParams.Length;
            var argsExp = new Expression[length];

            Type paramType;
            Expression paramAccessorExp;
            UnaryExpression paramCastExp;

            // Pick each arg from the params array and create a typed expression of them
            for (int i = 0; i < length; i++)
            {
                paramType = ctorParams[i].ParameterType;

#if !(NET3500 || NET3000 || NET2000)
                paramAccessorExp = Expression.ArrayAccess(paramExp, Expression.Constant(i));
#else
				paramAccessorExp = Expression.ArrayIndex(paramExp, Expression.Constant (i));
#endif
                paramCastExp = !paramType.IsValueType ?
                    Expression.TypeAs(paramAccessorExp, paramType) :
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            // Make a NewExpression that calls the ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);
            var returnExp = !declaringT.IsValueType ?
                (Expression)newExp :
                Expression.Convert(newExp, typeof(object));

            // Create a lambda with the New Expression as body and our param object[] as arg
            var lambda = Expression.Lambda<Func<object[], object>>(returnExp, paramExp);

            return lambda.Compile();
        }

#if !(NET3500 || NET3000 || NET2000)
        public static Action<object, object[]> PrepareMethodCall(MethodInfo methodInfo)
        {
            var declaringT = methodInfo.DeclaringType;
            var parameters = methodInfo.GetParameters();

            var argsExp = Expression.Parameter(typeof(object[]), "args");
            var inputObjExp = Expression.Parameter(typeof(object), "inputObj");
            var tVariable = Expression.Variable(declaringT);

            var variableList = new List<ParameterExpression> { tVariable };

            var inputCastExp = !declaringT.IsValueType ?
                Expression.TypeAs(inputObjExp, declaringT) :
                Expression.Convert(inputObjExp, declaringT);

            var assignmentExp = Expression.Assign(tVariable, inputCastExp);

            var bodyExps = new List<Expression> { assignmentExp };

            Expression callExp = null;
            if (parameters.Length == 0)
            {
                callExp = Expression.Call(tVariable, methodInfo);
            }
            else
            {
                Type paramType;
                Expression arrayAccessExp;
                Expression arrayValueCastExp;
                Expression variableAssignExp;
                ParameterExpression newVariableExp;

                var callArguments = new List<ParameterExpression>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    paramType = parameters[i].ParameterType;

                    newVariableExp = Expression.Variable(paramType, "param" + i);

                    callArguments.Add(newVariableExp);

                    arrayAccessExp = Expression.ArrayAccess(argsExp, Expression.Constant(i));
                    arrayValueCastExp = !paramType.IsValueType ?
                        Expression.TypeAs(arrayAccessExp, paramType) :
                        Expression.Convert(arrayAccessExp, paramType);

                    variableAssignExp = Expression.Assign(newVariableExp, arrayValueCastExp);
                    bodyExps.Add(variableAssignExp);
                }

                variableList.AddRange(callArguments);
                callExp = Expression.Call(tVariable, methodInfo, callArguments);
            }

            bodyExps.Add(callExp);

            var body = Expression.Block(variableList, bodyExps);
            return Expression.Lambda<Action<object, object[]>>(body, inputObjExp, argsExp).Compile();
        }
#else
		public static Action<object, object[]> PrepareMethodCall(MethodInfo methodInfo)
		{
			var declaringT = methodInfo.DeclaringType;
			var methodParams = methodInfo.GetParameters();

			var argsExp = Expression.Parameter(typeof (object[]), "args");
			var inputObjExp = Expression.Parameter(typeof(object), "inputObj");

			var inputCastExp = !declaringT.IsValueType ?
				Expression.TypeAs (inputObjExp, declaringT) : 
				Expression.Convert(inputObjExp, declaringT);

			Expression callExp;
			if (methodParams.Length == 0) {
				callExp = Expression.Call (inputCastExp, methodInfo);
			} else {
				var callArguments = new Expression[methodParams.Length];

				Type argType;
				Expression arrayAccessExp;
				Expression arrayValueCastExp;

				for (var i = 0; i < methodParams.Length; i++) {
					argType = methodParams [i].ParameterType;

					arrayAccessExp = Expression.ArrayIndex(argsExp, Expression.Constant(i));
					arrayValueCastExp = !argType.IsValueType ?
						Expression.TypeAs (arrayAccessExp, argType) : 
						Expression.Convert (arrayAccessExp, argType);

					callArguments [i] = arrayValueCastExp;
				}

				callExp = Expression.Call (inputCastExp, methodInfo, callArguments);
			}
			return Expression.Lambda<Action<object, object[]>>(callExp, inputObjExp, argsExp).Compile ();
		}
#endif

        internal static Action<object, object[]> GetICollectionAddMethod(Type objType)
        {
            return s_ICollectionAdd.GetValueOrUpdate(objType, (t) =>
                {
                    MethodInfo method;
                    var methods = t.GetMethods();

                    for (int i = methods.Length - 1; i > -1; i--)
                    {
                        method = methods[i];
                        if (method.Name == "Add" && method.GetParameters().Length == 1)
                            return PrepareMethodCall(method);
                    }

                    return null;
                });
        }

        internal static Action<object, object[]> GetStackPushMethod(Type objType)
        {
            return s_StackPush.GetValueOrUpdate(objType, (t) =>
                {
                    MethodInfo method;
                    var methods = t.GetMethods();

                    for (int i = methods.Length - 1; i > -1; i--)
                    {
                        method = methods[i];
                        if (method.Name == "Push" && method.GetParameters().Length == 1)
                            return PrepareMethodCall(method);
                    }
                    return null;
                });            
        }

        internal static Action<object, object[]> GetQueueEnqueueMethod(Type objType)
        {
            return s_QueueEnqueue.GetValueOrUpdate(objType, (t) =>
                {
                    MethodInfo method;
                    var methods = t.GetMethods();

                    for (int i = methods.Length - 1; i > -1; i--)
                    {
                        method = methods[i];
                        if (method.Name == "Enqueue" && method.GetParameters().Length == 1)
                            return PrepareMethodCall(method);
                    }
                    return null;
                });
        }

#if !(NET3500 || NET3000 || NET2000)
        internal static Action<object, object[]> GetConcurrentBagMethod(Type objType)
        {
            return s_ConcurrentBagAdd.GetValueOrUpdate(objType, (t) =>
                {
                    MethodInfo method;
                    var methods = t.GetMethods();

                    for (int i = methods.Length - 1; i > -1; i--)
                    {
                        method = methods[i];
                        if (method.Name == "Add" && method.GetParameters().Length == 1)
                            return PrepareMethodCall(method);
                    }
                    return null;
                });
        }

        internal static Action<object, object[]> GetIProducerConsumerCollectionAddMethod(Type objType)
        {
            return s_IProducerConsumerCollectionAdd.GetValueOrUpdate(objType, (t) =>
                {
                    if (IsProducerConsumerCollection(t))
                    {
                        var argTypes = t.GetGenericArguments();
                        if (argTypes != null && argTypes.Length == 1)
                        {
                            var ipcType = typeof(IProducerConsumerCollection<>).MakeGenericType(new Type[] { argTypes[0] });

                            MethodInfo method;
                            var methods = ipcType.GetMethods();

                            for (int i = methods.Length - 1; i > -1; i--)
                            {
                                method = methods[i];
                                if (method.Name == "TryAdd" && method.GetParameters().Length == 1)
                                    return PrepareMethodCall(method);
                            }
                        }
                    }
                    return null;
                });            
        }
#endif

        internal static Action<object, object[]> GetIDictionaryAddMethod(Type objType)
        {
            return s_IDictionaryAdd.GetValueOrUpdate(objType, (t) =>
                {
                    MethodInfo method;
                    var methods = objType.GetMethods();

                    for (int i = methods.Length - 1; i > -1; i--)
                    {
                        method = methods[i];
                        if (method.Name == "Add" && method.GetParameters().Length == 2)
                            return PrepareMethodCall(method);
                    }
                    return null;
                });
        }

        internal static JaysonDictionaryType GetDictionaryType(IEnumerable obj, out Type entryType)
        {
            entryType = null;
            var enumerator = obj.GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current is DictionaryEntry)
                    {
                        return JaysonDictionaryType.IDictionary;
                    }

                    entryType = enumerator.Current.GetType();

                    var cache = JaysonFastMemberCache.GetCache(entryType);
                    if (cache.GetAnyMember("Key") != null && cache.GetAnyMember("Value") != null)
                    {
                        return JaysonDictionaryType.IGenericDictionary;
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    ((IDisposable)enumerator).Dispose();
                }
            }
            return JaysonDictionaryType.Undefined;
        }

        public static bool IsNull (object value, JaysonFloatSerStrategy floatNanStrategy,
            JaysonFloatSerStrategy floatInfinityStrategy)
        {
            if ((value == null) || (value == DBNull.Value))
                return true;

            if (value is double) {
                var d = (double)value;
                return (double.IsNaN (d) && floatNanStrategy == JaysonFloatSerStrategy.ToNull) ||
                    (double.IsInfinity (d) && floatInfinityStrategy == JaysonFloatSerStrategy.ToNull);
            }

            if (value is float) {
                var f = (float)value;
                return (float.IsNaN (f) && floatNanStrategy == JaysonFloatSerStrategy.ToNull) ||
                    (float.IsInfinity (f) && floatInfinityStrategy == JaysonFloatSerStrategy.ToNull);
            }
            return false;
        }

        # endregion Helper Methods
    }
}
