using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.Core.Debug
{
    public static class TaskExtensions
    {
        public static void SafeFireAndForget(this Task task, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var operationName = $"{System.IO.Path.GetFileName(filePath)}:{callerName}:{lineNumber}";
            
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    LogHelper.LogError($"[TaskExtensions] Fire and Forget task failed in {operationName}:");
                    if (t.Exception != null)
                    {
                        foreach (var ex in t.Exception.InnerExceptions)
                        {
                            LogHelper.LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
                            LogHelper.LogError($"Stack Trace:\n{ex.StackTrace}");
                        }
                    }
                }
                else if (t.IsCanceled)
                {
                    LogHelper.LogWarning($"[TaskExtensions] Fire and Forget task canceled in {operationName}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.OnlyOnCanceled);
        }

        public static async Task<T> SafeWithTimeout<T>(this Task<T> task, int timeoutMs, T defaultValue = default(T), [CallerMemberName] string callerName = "")
        {
            try
            {
                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(task, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    LogHelper.LogWarning($"[TaskExtensions] Task timeout after {timeoutMs}ms in {callerName}");
                    return defaultValue;
                }
                
                return await task;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"[TaskExtensions] Exception in {callerName}: {ex.Message}");
                LogHelper.LogError($"Stack Trace:\n{ex.StackTrace}");
                return defaultValue;
            }
        }

        public static async Task SafeWithTimeout(this Task task, int timeoutMs, [CallerMemberName] string callerName = "")
        {
            try
            {
                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(task, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    LogHelper.LogWarning($"[TaskExtensions] Task timeout after {timeoutMs}ms in {callerName}");
                    return;
                }
                
                await task;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"[TaskExtensions] Exception in {callerName}: {ex.Message}");
                LogHelper.LogError($"Stack Trace:\n{ex.StackTrace}");
            }
        }

        public static async Task<T> WithLogging<T>(this Task<T> task, string operationName = "", [CallerMemberName] string callerName = "")
        {
            var startTime = DateTime.Now;
            var name = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            
            LogHelper.Log($"[TaskExtensions] Starting task: {name}");
            
            try
            {
                var result = await task;
                var duration = DateTime.Now - startTime;
                //LogHelper.Log($"[TaskExtensions] Task completed: {name} (took {duration.TotalMilliseconds:F2}ms)");
                return result;
            }
            catch (Exception ex)
            {
                var duration = DateTime.Now - startTime;
                //LogHelper.LogError($"[TaskExtensions] Task failed: {name} (took {duration.TotalMilliseconds:F2}ms)");
                LogHelper.LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
                LogHelper.LogError($"Stack Trace:\n{ex.StackTrace}");
                throw;
            }
        }

        public static async Task WithLogging(this Task task, string operationName = "", [CallerMemberName] string callerName = "")
        {
            var startTime = DateTime.Now;
            var name = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            
            LogHelper.Log($"[TaskExtensions] Starting task: {name}");
            
            try
            {
                await task;
                var duration = DateTime.Now - startTime;
                //LogHelper.Log($"[TaskExtensions] Task completed: {name} (took {duration.TotalMilliseconds:F2}ms)");
            }
            catch (Exception ex)
            {
                var duration = DateTime.Now - startTime;
                //LogHelper.LogError($"[TaskExtensions] Task failed: {name} (took {duration.TotalMilliseconds:F2}ms)");
                LogHelper.LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
                LogHelper.LogError($"Stack Trace:\n{ex.StackTrace}");
                throw;
            }
        }

        public static async Task<T> WithRetry<T>(this Func<Task<T>> taskFactory, int maxRetries = 3, int delayMs = 1000, [CallerMemberName] string callerName = "")
        {
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    LogHelper.Log($"[TaskExtensions] Attempt {attempt}/{maxRetries} for {callerName}");
                    return await taskFactory();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogHelper.LogWarning($"[TaskExtensions] Attempt {attempt}/{maxRetries} failed for {callerName}: {ex.Message}");
                    
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }
            
            LogHelper.LogError($"[TaskExtensions] All {maxRetries} attempts failed for {callerName}");
            throw lastException;
        }

        public static async Task WithRetry(this Func<Task> taskFactory, int maxRetries = 3, int delayMs = 1000, [CallerMemberName] string callerName = "")
        {
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    LogHelper.Log($"[TaskExtensions] Attempt {attempt}/{maxRetries} for {callerName}");
                    await taskFactory();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogHelper.LogWarning($"[TaskExtensions] Attempt {attempt}/{maxRetries} failed for {callerName}: {ex.Message}");
                    
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }
            
            LogHelper.LogError($"[TaskExtensions] All {maxRetries} attempts failed for {callerName}");
            throw lastException;
        }
    }
}