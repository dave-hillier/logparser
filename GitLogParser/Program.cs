using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitLogParser
{
	
	class MainClass
	{
		//git log --pretty=format:'[%h],%aN,%ad,%s' --date=short --numstat > ../log.txt

		readonly static string[] excludeSubstring = { ".designer." }; 
		readonly static string[] includeExts = { ".js", ".cs" };
		//private static string LogPath = "/Users/daveh/velabs/Ve.Config/log.txt";
		static string LogPath = "/Users/daveh/velabs/ClientSidePanel/log.txt";

		public static void Main(string[] args)
		{
			var commits = GitLog.Read(LogPath);

			FileReport.FileStats(commits, FileFilter);

			//MatrixBuilder.ReportSubDirectory(commits, FileFilter);
			MatrixBuilder.Report(commits, FileFilter);
			MatrixBuilder.ReportAuthor(commits, FileFilter);
			//MatrixBuilder.ReportAuthorSubDirectory(commits, FileFilter);
		}

		public static bool FileFilter(string fileName)
		{
			return includeExts.Any(e => e == Path.GetExtension(fileName)) &&
				              !excludeSubstring.Any(e => fileName.ToLowerInvariant().Contains(e)) &&
				              File.Exists(Path.Combine(Path.GetDirectoryName(LogPath), fileName));
		}
	}
}
