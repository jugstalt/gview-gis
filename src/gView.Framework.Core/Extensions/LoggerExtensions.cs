using Microsoft.Extensions.Logging;
using System;

namespace gView.Framework.Core.Extensions;

public static class LoggerExtensions
{
    public static void Log<T>(this ILogger logger, LogLevel logLevel, string messageTemplate, Func<T> action)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                T result = action();
                logger.Log(logLevel, messageTemplate, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging action.");
            }
        }
    }

    public static void Log<T1, T2>(this ILogger logger, LogLevel logLevel, string messageTemplate, Func<T1> action1, Func<T2> action2)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                T1 result1 = action1();
                T2 result2 = action2();
                logger.Log(logLevel, messageTemplate, result1, result2);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging actions.");
            }
        }
    }

    public static void Log<T1, T2, T3>(this ILogger logger, LogLevel logLevel, string messageTemplate, Func<T1> action1, Func<T2> action2, Func<T3> action3)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                T1 result1 = action1();
                T2 result2 = action2();
                T3 result3 = action3();
                logger.Log(logLevel, messageTemplate, result1, result2, result3);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging actions.");
            }
        }
    }

    public static void Log<T1, T2, T3, T4>(this ILogger logger, LogLevel logLevel, string messageTemplate, Func<T1> action1, Func<T2> action2, Func<T3> action3, Func<T4> action4)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                T1 result1 = action1();
                T2 result2 = action2();
                T3 result3 = action3();
                T4 result4 = action4();
                logger.Log(logLevel, messageTemplate, result1, result2, result3, result4);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging actions.");
            }
        }
    }

    public static void Log<T1, T2, T3, T4, T5>(this ILogger logger, LogLevel logLevel, string messageTemplate, Func<T1> action1, Func<T2> action2, Func<T3> action3, Func<T4> action4, Func<T5> action5)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                T1 result1 = action1();
                T2 result2 = action2();
                T3 result3 = action3();
                T4 result4 = action4();
                T5 result5 = action5();
                logger.Log(logLevel, messageTemplate, result1, result2, result3, result4, result5);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging actions.");
            }
        }
    }

    public static void Log(this ILogger logger, LogLevel logLevel, string messageTemplate, Action action)
    {
        if (logger.IsEnabled(logLevel))
        {
            try
            {
                action();
                logger.Log(logLevel, messageTemplate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the logging action.");
            }
        }
    }
}