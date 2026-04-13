using ricaun.NamedPipeWrapper.Json;

namespace ricaun.RevitTest.Application.Revit
{
    /// <summary>
    /// FileVersionInfoUtils
    /// </summary>
    public static class FileVersionInfoUtils
    {
        /// <summary>
        /// GetComments
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks><see cref="System.Reflection.AssemblyDescriptionAttribute"/> is used to get the comments.</remarks>
        public static string GetComments(string fileName)
        {
            try
            {
                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName);
                return fileVersionInfo.Comments;
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// GetComments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks><see cref="System.Reflection.AssemblyDescriptionAttribute"/> is used to get the comments.</remarks>
        public static T GetComments<T>(string fileName) where T : class
        {
            try
            {
                var comments = GetComments(fileName);
                return JsonExtension.JsonDeserialize<T>(comments);
            }
            catch { }
            return null;
        }

        /// <summary>
        /// TryGetComments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        /// <remarks><see cref="System.Reflection.AssemblyDescriptionAttribute"/> is used to get the comments.</remarks>
        public static bool TryGetComments<T>(string fileName, out T comments) where T : class
        {
            comments = GetComments<T>(fileName);
            return comments != null;
        }
    }
}