using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitLogParser
{

	
	class FileReport
	{
		public static void FileStats(IEnumerable<GitLog.Commit> commits, Func<string, bool> IncludeFile)
		{
			Console.WriteLine("Generating stats per file...");
			var flattenedCommits = from c in commits
								   from f in c.Files
				                   where IncludeFile(f.FileName) 
								   select new
								   {
									   c.Author,
									   c.Revision,
									   c.Message,
									   c.Time,
									   f.Added,
									   f.Removed,
									   f.FileName
								   };

			var commitsByFile = flattenedCommits.GroupBy(f => f.FileName);

			var fs = from fileCommit in commitsByFile
					 select fileCommit.Aggregate(new
					 {
						 FileName = fileCommit.Key,
						 RevisionCount = fileCommit.Count(),
						 Added = 0,
						 Removed = 0,
						 Authors = new HashSet<string>(), // TODO: added-removed per author? Most frequent author?
						 Time = new DateTime(2000, 1, 1)
					 }, (acc, item) =>
					 {
						 acc.Authors.Add(item.Author);
						 var mostRecentTimestamp = acc.Time > item.Time ? acc.Time : item.Time;
						 return new
						 {
							 acc.FileName,
							 RevisionCount = acc.RevisionCount + 1,
							 Added = acc.Added + item.Added,
							 Removed = acc.Removed + item.Removed,
							 acc.Authors,
							 Time = mostRecentTimestamp
						 };
						// TOOD: Put churn back in?
					 });

			var fileStats = fs.ToArray();

			Console.WriteLine("Writing to csv...");
			var csv = new StringBuilder();
			csv.Append("FileName");
			csv.Append(",");
			csv.Append("Revisions");
			csv.Append(",");
			csv.Append("Authors");
			csv.Append(",");
			csv.Append("TotalAdded");
			csv.Append(",");
			csv.Append("TotalRemoved");
			csv.AppendLine();

			foreach (var file in fileStats)
			{
				csv.Append(file.FileName);
				csv.Append(",");
				csv.Append(file.RevisionCount);
				csv.Append(",");
				csv.Append(file.Authors.Count());
				csv.Append(",");
				csv.Append(file.Added);
				csv.Append(",");
				csv.Append(file.Removed);
				csv.AppendLine();
			}

			File.WriteAllText("filestats.csv", csv.ToString());
		}
	}
	
}
