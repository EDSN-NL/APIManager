using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Framework.Logging
{
    /// <summary>
    /// Check the validity of input parameters and throw Argument Exceptions when not compliant.
    /// This class has been "borrowed" from the LibGit2Sharp library. 
    /// Copyright: LibGit2Sharp at https://github.com/libgit2/libgit2sharp
    /// </summary>
    [DebuggerStepThrough]
    internal static class Ensure
    {
        /// <summary>
        /// Checks an argument to ensure it isn't null.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Checks an array argument to ensure it isn't null or empty.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentNotNullOrEmptyEnumerable<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);

            if (!argumentValue.Any())
            {
                throw new ArgumentException("Enumerable cannot be empty", argumentName);
            }
        }

        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);

            if (String.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException("String cannot be empty", argumentName);
            }
        }

        /// <summary>
        /// Checks a string argument to ensure it doesn't contain a zero byte.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentDoesNotContainZeroByte(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                return;
            }

            int zeroPos = -1;
            for (var i = 0; i < argumentValue.Length; i++)
            {
                if (argumentValue[i] == '\0')
                {
                    zeroPos = i;
                    break;
                }
            }

            if (zeroPos == -1)
            {
                return;
            }

            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture,
                    "Zero bytes ('\\0') are not allowed. A zero byte has been found at position {0}.", zeroPos), argumentName);
        }

        /// <summary>
        /// Checks an argument to ensure it isn't a IntPtr.Zero (aka null).
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentNotZeroIntPtr(IntPtr argumentValue, string argumentName)
        {
            if (argumentValue == IntPtr.Zero)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Checks a pointer argument to ensure it is the expected pointer value.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentIsExpectedIntPtr(IntPtr argumentValue, IntPtr expectedValue, string argumentName)
        {
            if (argumentValue != expectedValue)
            {
                throw new ArgumentException("Unexpected IntPtr value", argumentName);
            }
        }

        /// <summary>
        /// Checks an argument by applying provided checker.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="checker">The predicate which has to be satisfied</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentConformsTo<T>(T argumentValue, Func<T, bool> checker, string argumentName)
        {
            if (checker(argumentValue))
            {
                return;
            }

            throw new ArgumentException(argumentName);
        }

        /// <summary>
        /// Checks an argument is a positive integer.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void ArgumentPositiveInt32(long argumentValue, string argumentName)
        {
            if (argumentValue >= 0 && argumentValue <= uint.MaxValue)
            {
                return;
            }

            throw new ArgumentException(argumentName);
        }
    }
}
