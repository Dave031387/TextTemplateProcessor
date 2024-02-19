namespace TextTemplateProcessor.Core
{
    using System;
    using static Messages;

    internal static class Utility
    {
        public static void NullDependencyCheck(
            object dependencyObject,
            string className,
            string serviceName,
            string parameterName)
        {
            if (dependencyObject is null)
            {
                string message = string.Format(MsgDependencyIsNull, className, serviceName);
                throw new ArgumentNullException(parameterName, message);
            }
        }
    }
}