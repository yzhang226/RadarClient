using log4net;
using System;

namespace RedCell.Diagnostics.Update
{
    /// <summary>
    /// A general purpose logging facility.
    /// </summary>
    public static class Log
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Log));

        #region Initialization
        static Log ()
        {
            Prefix = "[Update] ";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Log"/> logs to the console.
        /// </summary>
        /// <value><c>true</c> if console; otherwise, <c>false</c>.</value>
        public static bool Console { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Log"/> logs using the debug facilty.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        public static bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public static string Prefix { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Writes to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void Write (string format, params object[] args)
        {
            string message = string.Format(format, args);
            logger.InfoFormat(message);
        }
        #endregion
    }
}
