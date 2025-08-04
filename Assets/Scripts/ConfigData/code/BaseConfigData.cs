using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace YuankunHuang.Unity.GameDataConfig
{
    public abstract class BaseConfigData<T> where T : new()
    {
        protected static Dictionary<int, T> _dataById;
        protected static List<T> _allData;
        protected static bool _isInitialized = false;

        public static void Initialize(string binaryPath)
        {
            if (_isInitialized) return;
            LoadFromBinary(binaryPath);
            _isInitialized = true;
        }

        public static T GetById(int id)
        {
            if (!_isInitialized) throw new Exception("Not initialized");
            if (_dataById.TryGetValue(id, out var value))
                return value;
            return default!;
        }

        public static List<T> GetAll()
        {
            if (!_isInitialized) throw new Exception("Not initialized");
            return _allData ?? new List<T>();
        }

        public static int Count => _allData?.Count ?? 0;

        public static void Reload(string binaryPath)
        {
            _isInitialized = false;
            Initialize(binaryPath);
        }

        private static DateTime ReadValidDateTime(BinaryReader reader)
        {
            long ticks = reader.ReadInt64();
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
            {
                // Return default DateTime for invalid ticks
                return DateTime.MinValue;
            }
            return new DateTime(ticks);
        }

        protected static void LoadFromBinary(string binaryPath)
        {
            _allData = new List<T>();
            _dataById = new Dictionary<int, T>();

            if (!File.Exists(binaryPath))
                throw new FileNotFoundException($"Binary file not found: {binaryPath}");

            using var stream = File.OpenRead(binaryPath);
            using var reader = new BinaryReader(stream);

            try
            {
                // Check if we have enough bytes for header
                if (stream.Length < 8)
                    throw new InvalidDataException($"Binary file too small for header: {stream.Length} bytes");

                int fieldCount = reader.ReadInt32();
                int rowCount = reader.ReadInt32();

                // Validate field count
                if (fieldCount < 0 || fieldCount > 1000)
                    throw new InvalidDataException($"Invalid field count: {fieldCount}");

                // Validate row count
                if (rowCount < 0 || rowCount > 1000000)
                    throw new InvalidDataException($"Invalid row count: {rowCount}");

                var fields = new (string name, int type)[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    if (stream.Position >= stream.Length)
                        throw new EndOfStreamException($"Unexpected end of stream while reading field {i}. Position: {stream.Position}, Length: {stream.Length}");
                    fields[i] = (reader.ReadString(), reader.ReadInt32());
                }

                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (int row = 0; row < rowCount; row++)
                {
                    if (stream.Position >= stream.Length)
                        throw new EndOfStreamException($"Unexpected end of stream while reading row {row}");

                    var item = new T();
                    for (int col = 0; col < fieldCount; col++)
                    {
                        if (stream.Position >= stream.Length)
                            throw new EndOfStreamException($"Unexpected end of stream while reading row {row}, column {col}. Position: {stream.Position}, Length: {stream.Length}");
                        var (fieldName, fieldType) = fields[col];
                        var prop = props.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        if (prop != null)
                        {
                            object value = fieldType switch
                            {
                                0 => (object)reader.ReadString(),
                                1 => (object)reader.ReadInt32(),
                                2 => (object)reader.ReadInt64(),
                                3 => (object)reader.ReadSingle(),
                                4 => (object)reader.ReadBoolean(),
                                5 => (object)ReadValidDateTime(reader),
                                6 => (object)reader.ReadInt32(),
                                _ => (object)reader.ReadString()
                            };
                            // Type-safe assignment for DateTime and other types
                            var propType = prop.PropertyType;
                            if (fieldType == 5 && propType == typeof(int))
                            {
                                value = (int)(((DateTime)value).Ticks / TimeSpan.TicksPerSecond);
                            }
                            else if (fieldType == 5 && propType == typeof(long))
                            {
                                value = ((DateTime)value).Ticks;
                            }
                            else if (fieldType == 5 && propType == typeof(string))
                            {
                                value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else if (fieldType != 5 && propType == typeof(DateTime))
                            {
                                if (value is long longValue)
                                    value = new DateTime(longValue);
                                else if (value is int intValue)
                                    value = new DateTime(intValue * TimeSpan.TicksPerSecond);
                                else if (value is string strValue && DateTime.TryParseExact(strValue, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dt))
                                    value = dt;
                            }
                            // Robust type conversion
                            if (value != null && propType != value.GetType())
                            {
                                if (propType.IsEnum)
                                {
                                    if (value is string str && System.Enum.TryParse(propType, str, out var enumVal))
                                        value = enumVal;
                                    else if (value is int intVal)
                                        value = System.Enum.ToObject(propType, intVal);
                                }
                                else
                                    value = System.Convert.ChangeType(value, propType);
                            }
                            if (value != null)
                                prop.SetValue(item, value);
                        }
                        else
                        {
                            _ = fieldType switch
                            {
                                0 => (object)reader.ReadString(),
                                1 => (object)reader.ReadInt32(),
                                2 => (object)reader.ReadInt64(),
                                3 => (object)reader.ReadSingle(),
                                4 => (object)reader.ReadBoolean(),
                                5 => (object)ReadValidDateTime(reader),
                                6 => (object)reader.ReadInt32(),
                                _ => (object)reader.ReadString()
                            };
                        }
                    }
                    _allData.Add(item);

                    // Index by id property if exists
                    var idProp = props.FirstOrDefault(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
                    if (idProp != null)
                    {
                        int id = (int)(idProp.GetValue(item) ?? 0);
                        if (id > 0)
                            _dataById[id] = item;
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException($"Binary file is corrupted or incomplete: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error reading binary file: {ex.Message}", ex);
            }
        }
    }
}
