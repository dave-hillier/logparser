using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitLogParser
{
    class GitLog
    {
        public class FileChange
        {
            public int Added;
            public int Removed;
            public string FileName;
        }

        public class Commit
        {
            public DateTime Time;
            public string Revision;
            public string Author;
            public List<FileChange> Files = new List<FileChange>();
            public string Message;

            public static Commit Parse(string line)
            {
                var parsed = line.Split(',');
                var commit = new Commit
                {
                    Revision = parsed[0].TrimStart('[').TrimEnd(']'),
                    Author = parsed[1],
                    Time = DateTime.Parse(parsed[2]),
                    Message = parsed[3]
                };

                return commit;
            }
        }

        public static IEnumerable<Commit> Read(string logPath)
        {
            var lines = File.ReadLines(logPath).ToArray();

            return lines.Aggregate(new List<Commit>(), (commits, line) =>
            {
                ProcessLine(commits, line);
                return commits;
            });
        }

        static void ProcessLine(List<Commit> acc, string line)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                Commit commit = Commit.Parse(line);
                acc.Add(commit);
            }
            else if (trimmed.Length != 0)
            {
                var parts = line.Split('\t');
                if (parts[0] != "-" && parts[1] != "-")
                {
                    var file = new FileChange { Added = int.Parse(parts[0]), Removed = int.Parse(parts[1]), FileName = parts[2] };
                    acc.Last().Files.Add(file);
                }
            }
        }
    }
}
