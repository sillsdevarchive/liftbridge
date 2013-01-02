using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIL.LiftBridge.Services
{
	/// <summary>
	/// Handle some special file/directory behavior for Lift Bridge.
	/// </summary>
	internal static class FileAndDirectoryServices
	{
		/// <summary>
		/// Collect up all folders and files.
		/// </summary>
		internal static HashSet<string> EnumerateExtantFiles(string baseDirPath)
		{
			var results = new HashSet<string>(Directory.GetFiles(baseDirPath)) {baseDirPath}; // Top level folder's files.

			foreach (var dirName in Directory.GetDirectories(baseDirPath))
			{
				if (dirName.EndsWith(".hg") || dirName.EndsWith(".git"))
					continue; // Skip the repo file (for Hg or Git, if it becomes an option).

				results.UnionWith(EnumerateExtantFiles(dirName));
			}

			return results;
		}

		/// <summary>
		/// Delete all new files and/or folders.
		/// </summary>
		internal static void WipeOutNewStuff(ICollection<string> extantFileBeforeExport, IEnumerable<string> extantFileAfterExport)
		{
			foreach (var pathname in extantFileAfterExport.Where(pathname => !extantFileBeforeExport.Contains(pathname)))
			{
				// Wipe it out, if it hasn't be zapped already.
				if (File.Exists(pathname))
					File.Delete(pathname);
				else if (Directory.Exists(pathname))
					Directory.Delete(pathname, true); // A new folder has to have new files, so just wipe them all out.
			}
		}
	}
}
