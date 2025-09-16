using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Threading.Tasks;
using UnityEngine.Networking;
#endif

namespace YuankunHuang.Unity.GameDataConfig
{
    public abstract class BaseConfigData<T> where T : new()
    {
        protected static Dictionary<int, T> _dataById;
        protected static List<T> _allData;
        protected static bool _isInitialized = false;

        public static void Initialize(string binaryPath)
        {
            Debug.Log($"[BaseConfigData] Initialize ENTER for {typeof(T).Name}, path: {binaryPath}");

            if (_isInitialized)
            {
                Debug.Log($"[BaseConfigData] {typeof(T).Name} already initialized, skipping");
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log($"[BaseConfigData] WebGL mode - calling InitializeAsync for {typeof(T).Name}");
            // WebGL requires async initialization
            _ = InitializeAsync(binaryPath);
#else
            Debug.Log($"[BaseConfigData] Non-WebGL mode - calling LoadFromBinary for {typeof(T).Name}");
            LoadFromBinary(binaryPath);
            _isInitialized = true;
            Debug.Log($"[BaseConfigData] LoadFromBinary completed for {typeof(T).Name}, calling CallPostInitialize");
            CallPostInitialize();
#endif
            Debug.Log($"[BaseConfigData] Initialize EXIT for {typeof(T).Name}");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public static async Task InitializeAsync(string binaryPath)
        {
            Debug.Log($"[BaseConfigData] InitializeAsync ENTER for {typeof(T).Name}, path: {binaryPath}");

            if (_isInitialized)
            {
                Debug.Log($"[BaseConfigData] {typeof(T).Name} already initialized, skipping async");
                return;
            }

            try
            {
                Debug.Log($"[BaseConfigData] Calling LoadFromBinaryAsync for {typeof(T).Name}");
                await LoadFromBinaryAsync(binaryPath);
                _isInitialized = true;
                Debug.Log($"[BaseConfigData] LoadFromBinaryAsync completed for {typeof(T).Name}, calling CallPostInitialize");
                CallPostInitialize();
                Debug.Log($"[BaseConfigData] {typeof(T).Name} loaded successfully from {binaryPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BaseConfigData] Failed to load {typeof(T).Name} from {binaryPath}: {e.Message}");
                Debug.LogException(e);
                // Initialize empty collections to prevent null reference
                _allData = new List<T>();
                _dataById = new Dictionary<int, T>();
                _isInitialized = true;
                Debug.Log($"[BaseConfigData] Error recovery - calling CallPostInitialize for {typeof(T).Name}");
                CallPostInitialize();
            }

            Debug.Log($"[BaseConfigData] InitializeAsync EXIT for {typeof(T).Name}");
        }
#endif

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

        private static void CallPostInitialize()
        {
            Debug.Log($"[BaseConfigData] CallPostInitialize ENTER for {typeof(T).Name}");

            try
            {
                // Call PostInitialize method if it exists on the concrete Config class
                // We need to find the actual Config class that inherits from BaseConfigData<T>
                Debug.Log($"[BaseConfigData] Getting assemblies for {typeof(T).Name}");

                var executingAssembly = Assembly.GetExecutingAssembly();
                var typeAssembly = typeof(T).Assembly;

                Debug.Log($"[BaseConfigData] ExecutingAssembly: {executingAssembly?.FullName ?? "null"}");
                Debug.Log($"[BaseConfigData] TypeAssembly: {typeAssembly?.FullName ?? "null"}");

                var assemblies = new[] { executingAssembly, typeAssembly };

                Debug.Log($"[BaseConfigData] Processing {assemblies.Length} assemblies");

                foreach (var assembly in assemblies.Distinct())
                {
                    if (assembly != null)
                    {
                        Debug.Log($"[BaseConfigData] Processing assembly: {assembly.FullName}");

                        try
                        {
                            Debug.Log($"[BaseConfigData] Getting types from assembly: {assembly.FullName}");
                            var allTypes = assembly.GetTypes();
                            Debug.Log($"[BaseConfigData] Found {allTypes.Length} types in assembly");

                            var configTypes = allTypes
                                .Where(type => type.IsClass && !type.IsAbstract)
                                .Where(type => type.BaseType != null && type.BaseType.IsGenericType)
                                .Where(type => type.BaseType.GetGenericTypeDefinition() == typeof(BaseConfigData<>))
                                .Where(type => type.BaseType.GetGenericArguments()[0] == typeof(T));

                            var configTypesList = configTypes.ToList();
                            Debug.Log($"[BaseConfigData] Found {configTypesList.Count} matching config types");

                            foreach (var configType in configTypesList)
                            {
                                Debug.Log($"[BaseConfigData] Checking config type: {configType.Name}");

                                var postInitMethod = configType.GetMethod("PostInitialize", BindingFlags.NonPublic | BindingFlags.Static);
                                if (postInitMethod != null)
                                {
                                    Debug.Log($"[BaseConfigData] Found PostInitialize method in {configType.Name}, invoking...");

                                    try
                                    {
                                        postInitMethod.Invoke(null, null);
                                        Debug.Log($"[BaseConfigData] PostInitialize completed successfully for {configType.Name}");
                                        return; // Found and called, exit
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError($"[BaseConfigData] PostInitialize failed for {configType.Name}: {e.Message}");
                                        Debug.LogException(e);
                                    }
                                }
                                else
                                {
                                    Debug.Log($"[BaseConfigData] No PostInitialize method found in {configType.Name}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[BaseConfigData] Failed to search for PostInitialize in assembly {assembly.FullName}: {e.Message}");
                            Debug.LogException(e);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[BaseConfigData] Assembly is null");
                    }
                }

                Debug.Log($"[BaseConfigData] CallPostInitialize COMPLETE for {typeof(T).Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BaseConfigData] CallPostInitialize FAILED for {typeof(T).Name}: {e.Message}");
                Debug.LogException(e);
            }
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

#if UNITY_WEBGL && !UNITY_EDITOR
        protected static async Task LoadFromBinaryAsync(string binaryPath)
        {
            _allData = new List<T>();
            _dataById = new Dictionary<int, T>();

            // Convert Unity StreamingAssets path to WebGL URL
            string url = binaryPath;
            if (!url.StartsWith("http"))
            {
                // Convert local path to StreamingAssets URL for WebGL
                var fileName = Path.GetFileName(binaryPath);
                url = Path.Combine(Application.streamingAssetsPath, "ConfigData", fileName);
            }

            using UnityWebRequest www = UnityWebRequest.Get(url);
            await SendWebRequest(www);

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new FileNotFoundException($"Failed to load binary file from {url}: {www.error}");
            }

            byte[] data = www.downloadHandler.data;
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            // Same binary reading logic as LoadFromBinary
            // ... (copy the entire LoadFromBinary logic here)
            try
            {
                if (stream.Length < 8)
                    throw new InvalidDataException($"Binary file too small for header: {stream.Length} bytes");

                int fieldCount = reader.ReadInt32();
                int rowCount = reader.ReadInt32();

                if (fieldCount < 0 || fieldCount > 1000)
                    throw new InvalidDataException($"Invalid field count: {fieldCount}");

                if (rowCount < 0 || rowCount > 1000000)
                    throw new InvalidDataException($"Invalid row count: {rowCount}");

                var fields = new (string name, int type)[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    if (stream.Position >= stream.Length)
                        throw new EndOfStreamException($"Unexpected end of stream while reading field {i}");
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
                        var (fieldName, fieldType) = fields[col];
                        var prop = props.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                        if (prop != null)
                        {
                            object value = fieldType switch
                            {
                                0 => reader.ReadString(),
                                1 => reader.ReadInt32(),
                                2 => reader.ReadInt64(),
                                3 => reader.ReadSingle(),
                                4 => reader.ReadBoolean(),
                                5 => ReadValidDateTime(reader),
                                6 => reader.ReadInt32(),
                                _ => reader.ReadString()
                            };

                            // Type conversion logic (same as LoadFromBinary)
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
                            // Skip unknown field
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

        private static Task SendWebRequest(UnityWebRequest www)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = www.SendWebRequest();

            operation.completed += _ =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                    tcs.SetResult(true);
                else
                    tcs.SetException(new Exception($"WebRequest failed: {www.error}"));
            };

            return tcs.Task;
        }
#endif
    }
}
