using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitLogParser
{
	class MatrixBuilder
	{
		static IEnumerable<MatrixValue> BuildMatrix(IEnumerable<GitLog.Commit> commits,
		                                            Func<string, bool> IncludeFile, 
		                                            Func<GitLog.Commit, string, Tuple<string, string>> ToKv)
		{
			var flattenedCommits = from c in commits
								   from f in c.Files
								   where IncludeFile(f.FileName)
			                       select ToKv(c, f.FileName);


			var subDirectoryRevs = flattenedCommits.GroupBy(f => f.Item1).
			                                       ToDictionary(fr => fr.Key, fr => fr.Select(v => v.Item2).ToArray());

			var subDirectory = flattenedCommits.Select(f => f.Item1).
										Distinct().
										ToArray();
			
			return from f1 in subDirectory
				   from f2 in subDirectory
				   //where f1 != f2  // Or keep it?
				   select new MatrixValue
				   {
					   Key1 = f1,
					   Key2 = f2,
					   Values = f1 != f2 ? subDirectoryRevs[f1].Intersect(subDirectoryRevs[f2]).ToArray() : new string[] { }
				   }; 

		}

		class MatrixValue
		{
			public string[] Values { get; set; }
			public string Key1 { get; set; }
			public string Key2 { get; set; }
		}

		static void WriteToFile(string fileName, IEnumerable<MatrixValue> m)
		{
			Console.WriteLine("Writing csv...");
			var csv = new StringBuilder();
			var matrix = m.ToArray();
			var dict = matrix.ToDictionary(v => Tuple.Create(v.Key1, v.Key2), v => v.Values.Count());
			var axis = m.Select(k => k.Key1).Distinct().ToArray();

			csv.Append(","); // first cell empty
			foreach(var a in axis)
			{
				csv.Append(a);
				csv.Append(",");
			}
			csv.AppendLine();

			foreach (var y in axis)
			{
				csv.Append(y);
				csv.Append(",");
				foreach (var x in axis)
				{
					csv.Append(dict[Tuple.Create(x, y)]);
					csv.Append(",");
				}
				csv.AppendLine();
			}

			File.WriteAllText(fileName, csv.ToString());
		}


		static void WriteToGv(string fileName, MatrixValue[] matrix)
		{
			Console.WriteLine("Writing csv...");
			var gv = new StringBuilder();

			//var values = matrix.Select(k => k.Key1).Distinct().ToArray();
			var dict = matrix.ToDictionary(v => Tuple.Create(v.Key1, v.Key2), v => v.Values.Count());

			gv.AppendLine("digraph test {");
			var visited = new HashSet<string>();
			/*foreach (var node in values)
			{
				
			}*/

			foreach (var link in dict)
			{
				var visitKey = string.Compare(link.Key.Item1, link.Key.Item2, StringComparison.Ordinal) < 0 ?
								   link.Key.Item1 + link.Key.Item2 : link.Key.Item2 + link.Key.Item1;

				if (link.Value > 0 && !visited.Contains(visitKey))
				{
					gv.AppendFormat("\"{0}\" -- \"{1}\" [weight={2}];", link.Key.Item1, link.Key.Item2, link.Value);
					gv.AppendLine();
					visited.Add(visitKey);
				}
			}
			gv.AppendLine("}");

			File.WriteAllText(fileName, gv.ToString());	

		}


		public static void ReportSubDirectory(IEnumerable<GitLog.Commit> commits, Func<string, bool> IncludeFile)
		{
			Console.WriteLine("ReportSubDirectory...");
			var m = BuildMatrix(commits, IncludeFile, (c, fn) => Tuple.Create(fn.Split('/')[0], c.Revision));
			WriteToFile("subdir.csv", m.ToArray());
		}

		public static void Report(IEnumerable<GitLog.Commit> commits, Func<string, bool> IncludeFile)
		{
			Console.WriteLine("File-file...");
			var m = BuildMatrix(commits, IncludeFile, (c, fn) => Tuple.Create(fn, c.Revision));
			WriteToFile("files.csv", m.ToArray());
		}


		public static void ReportAuthor(IEnumerable<GitLog.Commit> commits, Func<string, bool> IncludeFile)
		{
			Console.WriteLine("ReportAuthor...");
			var m = BuildMatrix(commits, IncludeFile, (c, fn) => Tuple.Create(c.Author, fn)).ToArray();
			WriteToFile("author.csv", m);

			WriteToGv("authors.gv", m);
		}


		public static void ReportAuthorSubDirectory(IEnumerable<GitLog.Commit> commits, Func<string, bool> IncludeFile)
		{
			Console.WriteLine("ReportAuthorSubDirectory...");
			var m = BuildMatrix(commits, IncludeFile, (c, fn) => Tuple.Create(c.Author, fn.Split('/')[0]));
			WriteToFile("authorsubdir.csv", m.ToArray());
		}

	}
	
}
