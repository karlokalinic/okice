using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Scans serialized project text for legacy narrative identifiers that must not
    /// survive the GRADOMRAZ authorship remake. This is intentionally an audit tool:
    /// it never rewrites project files automatically.
    /// </summary>
    public static class IdentityAudit
    {
        private static readonly string[] LegacyTerms =
        {
            "Maddie",
            "MirrorMan",
            "Mirror Man",
            "Block 28",
            "Block 29",
            "AFTERLIVES",
            "Cold City",
            "HladanGrad",
            "HaveEyes",
            "HaveEyeKey",
            "HaveSaw",
            "BridgeEvents",
            "jet crash",
            "MaddieRoom",
            "MaddieHallway",
            "mirror stage",
            "bunker phone"
        };

        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".unity", ".prefab", ".asset", ".controller", ".anim",
            ".shader", ".compute", ".json", ".txt", ".md", ".asmdef",
            ".uss", ".uxml", ".xml", ".yaml", ".yml"
        };

        private static readonly string[] ProductionIgnoredPathFragments =
        {
            "/_TranslationBackup/",
            "/_QualityUpgradeBackup/",
            "/_QualitySnapshots/",
            "/Assets/Editor/KARLOLEGEND_GRADOMRAZ/IdentityAudit.cs"
        };

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Identity Audit/Scan Production Assets")]
        public static void ScanProductionAssets()
        {
            RunAudit(includeHistoricalBackups: false);
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Identity Audit/Scan Including Historical Backups")]
        public static void ScanEverything()
        {
            RunAudit(includeHistoricalBackups: true);
        }

        private static void RunAudit(bool includeHistoricalBackups)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                Debug.LogError("GRADOMRAZ Identity Audit: could not resolve project root.");
                return;
            }

            var roots = new[]
            {
                Path.Combine(projectRoot, "Assets"),
                Path.Combine(projectRoot, "ProjectSettings")
            };

            var hits = new List<AuditHit>();
            var scannedFiles = 0;

            try
            {
                foreach (var root in roots.Where(Directory.Exists))
                {
                    foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        if (!TextExtensions.Contains(Path.GetExtension(path)))
                        {
                            continue;
                        }

                        var normalizedPath = Normalize(path);
                        if (!includeHistoricalBackups && ShouldIgnoreInProductionAudit(normalizedPath))
                        {
                            continue;
                        }

                        scannedFiles++;
                        ScanFile(projectRoot, path, hits);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "GRADOMRAZ Identity Audit",
                    "Audit aborted because a project file could not be scanned. Check the Console.",
                    "OK");
                return;
            }

            var reportPath = Path.Combine(projectRoot, "Temp", "GRADOMRAZ_IdentityAudit.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);
            File.WriteAllText(reportPath, BuildReport(scannedFiles, hits, includeHistoricalBackups), Encoding.UTF8);

            foreach (var hit in hits.Take(250))
            {
                Debug.LogWarning($"GRADOMRAZ LEGACY IDENTITY: {hit.Term} -> {hit.RelativePath}:{hit.LineNumber}\n{hit.Line.Trim()}");
            }

            if (hits.Count > 250)
            {
                Debug.LogWarning($"GRADOMRAZ Identity Audit: {hits.Count - 250} additional hits omitted from Console. Full report: {reportPath}");
            }

            var scopeName = includeHistoricalBackups ? "full project history" : "production Assets + ProjectSettings";
            var message = hits.Count == 0
                ? $"PASS\n\nScanned {scannedFiles} text files in {scopeName}.\nNo prohibited legacy identifiers were found."
                : $"FOUND {hits.Count} LEGACY REFERENCES\n\nScanned {scannedFiles} text files in {scopeName}.\n\nThis branch is NOT provenance-clean yet.\nFull report:\n{reportPath}";

            Debug.Log($"GRADOMRAZ Identity Audit complete. files={scannedFiles}, hits={hits.Count}, report={reportPath}");
            EditorUtility.DisplayDialog("GRADOMRAZ Identity Audit", message, "OK");
        }

        private static void ScanFile(string projectRoot, string path, ICollection<AuditHit> hits)
        {
            var lineNumber = 0;
            foreach (var line in File.ReadLines(path))
            {
                lineNumber++;
                foreach (var term in LegacyTerms)
                {
                    if (line.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    hits.Add(new AuditHit(
                        MakeRelative(projectRoot, path),
                        lineNumber,
                        term,
                        line));
                }
            }
        }

        private static bool ShouldIgnoreInProductionAudit(string normalizedPath)
        {
            return ProductionIgnoredPathFragments.Any(fragment =>
                normalizedPath.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string BuildReport(int scannedFiles, IReadOnlyCollection<AuditHit> hits, bool includeHistoricalBackups)
        {
            var builder = new StringBuilder();
            builder.AppendLine("GRADOMRAZ — LEGACY IDENTITY AUDIT");
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Scope: {(includeHistoricalBackups ? "Assets + ProjectSettings including backups" : "production Assets + ProjectSettings")}");
            builder.AppendLine($"Files scanned: {scannedFiles}");
            builder.AppendLine($"Legacy references: {hits.Count}");
            builder.AppendLine();

            foreach (var group in hits.GroupBy(hit => hit.Term, StringComparer.OrdinalIgnoreCase).OrderByDescending(group => group.Count()))
            {
                builder.AppendLine($"[{group.Key}] {group.Count()} hit(s)");
                foreach (var hit in group.OrderBy(hit => hit.RelativePath).ThenBy(hit => hit.LineNumber))
                {
                    builder.AppendLine($"  {hit.RelativePath}:{hit.LineNumber}");
                    builder.AppendLine($"    {hit.Line.Trim()}");
                }

                builder.AppendLine();
            }

            if (hits.Count == 0)
            {
                builder.AppendLine("PASS: no prohibited legacy identifiers found in the selected scope.");
            }
            else
            {
                builder.AppendLine("FAIL: creative-provenance cleanup is incomplete.");
                builder.AppendLine("Do not silence findings by renaming only. Remove or redesign the underlying inherited mechanic/narrative dependency.");
            }

            return builder.ToString();
        }

        private static string MakeRelative(string root, string path)
        {
            var rootUri = new Uri(AppendDirectorySeparator(root));
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        private readonly struct AuditHit
        {
            public AuditHit(string relativePath, int lineNumber, string term, string line)
            {
                RelativePath = relativePath;
                LineNumber = lineNumber;
                Term = term;
                Line = line;
            }

            public string RelativePath { get; }
            public int LineNumber { get; }
            public string Term { get; }
            public string Line { get; }
        }
    }
}
