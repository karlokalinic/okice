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
    ///
    /// SIMPLE MODEL:
    ///
    ///      list of forbidden/legacy words
    ///                 ↓
    ///      walk through Assets + ProjectSettings
    ///                 ↓
    ///      ignore binary/irrelevant file extensions
    ///                 ↓
    ///      optionally ignore historical backup folders
    ///                 ↓
    ///      read every remaining file line by line
    ///                 ↓
    ///      case-insensitive search for every LegacyTerm
    ///                 ↓
    ///      collect AuditHit(path, line, term, text)
    ///                 ↓
    ///      write full Temp/GRADOMRAZ_IdentityAudit.txt report
    ///                 ↓
    ///      print first 250 hits to Unity Console
    ///                 ↓
    ///      display PASS / FOUND dialog
    ///
    /// The important design choice is that this code is a DETECTOR, not an automatic fixer.
    /// A finding is evidence that somebody must inspect the creative/mechanical dependency; blindly replacing
    /// a name could hide inherited design while leaving the underlying structure unchanged.
    /// </summary>
    public static class IdentityAudit
    {
        /// <summary>
        /// Case-insensitive search vocabulary for identities/concepts considered legacy provenance.
        ///
        /// Each entry is searched literally as a substring of each source line. This is intentionally simple:
        /// "Mirror Man" can match prose, YAML, C# strings, scene serialization, etc. It is NOT a parser and does
        /// not distinguish comments from executable code or dialogue from metadata.
        ///
        /// Consequence: adding a term here broadens the audit globally across every scanned text format.
        /// </summary>
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

        /// <summary>
        /// File types considered safe/useful to treat as text and scan line-by-line.
        ///
        /// Why filter extensions? A Unity project contains textures, audio, models and other binary files.
        /// Feeding those to File.ReadLines would be meaningless, slower, and can fail on arbitrary binary bytes.
        /// StringComparer.OrdinalIgnoreCase means `.CS`, `.cs`, etc. are treated as the same extension.
        /// </summary>
        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".unity", ".prefab", ".asset", ".controller", ".anim",
            ".shader", ".compute", ".json", ".txt", ".md", ".asmdef",
            ".uss", ".uxml", ".xml", ".yaml", ".yml"
        };

        /// <summary>
        /// Paths intentionally excluded from the normal PRODUCTION audit.
        ///
        /// These locations can legitimately preserve historical material. The audit also excludes its own source
        /// file because this very LegacyTerms array contains every forbidden word by definition; scanning itself
        /// would otherwise guarantee false positives every run.
        ///
        /// ScanEverything() bypasses this production-ignore list so historical backups can still be inspected.
        /// </summary>
        private static readonly string[] ProductionIgnoredPathFragments =
        {
            "/_TranslationBackup/",
            "/_QualityUpgradeBackup/",
            "/_QualitySnapshots/",
            "/Assets/Editor/KARLOLEGEND_GRADOMRAZ/IdentityAudit.cs"
        };

        /// <summary>
        /// Normal day-to-day audit command exposed in Unity's KARLOLEGEND menu.
        /// Historical backup/snapshot locations are ignored so old archival evidence does not fail production.
        /// </summary>
        [MenuItem("KARLOLEGEND/GRADOMRAZ/Identity Audit/Scan Production Assets")]
        public static void ScanProductionAssets()
        {
            RunAudit(includeHistoricalBackups: false);
        }

        /// <summary>
        /// Stronger forensic mode: scans the same roots but does NOT apply the production backup ignore list.
        /// Use this when the question is "does this identifier exist anywhere in project history?" rather than
        /// "can this legacy identity leak into the current production project?".
        /// </summary>
        [MenuItem("KARLOLEGEND/GRADOMRAZ/Identity Audit/Scan Including Historical Backups")]
        public static void ScanEverything()
        {
            RunAudit(includeHistoricalBackups: true);
        }

        /// <summary>
        /// Orchestrates one complete audit from locating the project root to showing the final result dialog.
        ///
        /// `includeHistoricalBackups` changes FILTERING only; both modes scan Assets and ProjectSettings.
        ///
        /// PERFORMANCE MODEL:
        /// For every accepted text file, ScanFile reads every line and compares it against every LegacyTerm.
        /// Roughly speaking the work grows with:
        ///
        ///     number of files × lines per file × number of legacy terms
        ///
        /// This is fine for an editor audit, but it is deliberately not an every-frame runtime system.
        /// </summary>
        private static void RunAudit(bool includeHistoricalBackups)
        {
            // Application.dataPath points at <project>/Assets. Its parent is therefore the Unity project root.
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                Debug.LogError("GRADOMRAZ Identity Audit: could not resolve project root.");
                return;
            }

            // Only project content and project configuration are searched. Library/Temp/Packages are intentionally
            // outside this root list, preventing generated caches and installed package source from polluting results.
            var roots = new[]
            {
                Path.Combine(projectRoot, "Assets"),
                Path.Combine(projectRoot, "ProjectSettings")
            };

            // All findings from all files accumulate here before report generation.
            var hits = new List<AuditHit>();

            // Counts only files that survive extension + optional production path filtering and are actually scanned.
            var scannedFiles = 0;

            try
            {
                // Ignore a configured root if it does not exist instead of throwing before enumeration begins.
                foreach (var root in roots.Where(Directory.Exists))
                {
                    // SearchOption.AllDirectories recursively walks the entire hierarchy below each root.
                    foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        // Skip textures/models/audio/etc.; only extensions declared above are treated as searchable text.
                        if (!TextExtensions.Contains(Path.GetExtension(path)))
                        {
                            continue;
                        }

                        // Convert Windows backslashes to forward slashes before fragment matching so ignore rules are
                        // stable and readable regardless of the native separator used in the raw path.
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
                // A permissions/encoding/filesystem error aborts the WHOLE audit rather than returning a misleading
                // "PASS" produced from an incomplete subset of files.
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "GRADOMRAZ Identity Audit",
                    "Audit aborted because a project file could not be scanned. Check the Console.",
                    "OK");
                return;
            }

            // Temp is appropriate for a generated diagnostic report: useful locally, not canonical authored content.
            var reportPath = Path.Combine(projectRoot, "Temp", "GRADOMRAZ_IdentityAudit.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);
            File.WriteAllText(reportPath, BuildReport(scannedFiles, hits, includeHistoricalBackups), Encoding.UTF8);

            // Console output is intentionally capped. A pathological migration can produce thousands of hits and
            // flooding the Unity Console would make the useful first findings harder to inspect.
            foreach (var hit in hits.Take(250))
            {
                Debug.LogWarning($"GRADOMRAZ LEGACY IDENTITY: {hit.Term} -> {hit.RelativePath}:{hit.LineNumber}\n{hit.Line.Trim()}");
            }

            if (hits.Count > 250)
            {
                Debug.LogWarning($"GRADOMRAZ Identity Audit: {hits.Count - 250} additional hits omitted from Console. Full report: {reportPath}");
            }

            // The same hit collection drives both a human-readable PASS/FAIL-style summary and the detailed report.
            var scopeName = includeHistoricalBackups ? "full project history" : "production Assets + ProjectSettings";
            var message = hits.Count == 0
                ? $"PASS\n\nScanned {scannedFiles} text files in {scopeName}.\nNo prohibited legacy identifiers were found."
                : $"FOUND {hits.Count} LEGACY REFERENCES\n\nScanned {scannedFiles} text files in {scopeName}.\n\nThis branch is NOT provenance-clean yet.\nFull report:\n{reportPath}";

            Debug.Log($"GRADOMRAZ Identity Audit complete. files={scannedFiles}, hits={hits.Count}, report={reportPath}");
            EditorUtility.DisplayDialog("GRADOMRAZ Identity Audit", message, "OK");
        }

        /// <summary>
        /// Scans ONE text file and appends findings into the collection owned by RunAudit().
        ///
        /// File.ReadLines is lazy: it reads the file progressively rather than first loading the entire file into
        /// one giant string. For large serialized Unity scenes this is more memory-friendly.
        ///
        /// A single line may generate multiple AuditHit records if it contains multiple legacy terms.
        /// </summary>
        private static void ScanFile(string projectRoot, string path, ICollection<AuditHit> hits)
        {
            var lineNumber = 0;
            foreach (var line in File.ReadLines(path))
            {
                lineNumber++;

                foreach (var term in LegacyTerms)
                {
                    // OrdinalIgnoreCase is culture-independent literal text comparison:
                    // "maddie", "MADDIE" and "Maddie" all count as the same legacy term.
                    if (line.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    // Store enough context to make the finding actionable without reopening/scanning the file again.
                    hits.Add(new AuditHit(
                        MakeRelative(projectRoot, path),
                        lineNumber,
                        term,
                        line));
                }
            }
        }

        /// <summary>
        /// Returns true when a normalized path contains ANY production-ignore fragment.
        /// `Any(...)` stops as soon as the first matching fragment is found.
        /// </summary>
        private static bool ShouldIgnoreInProductionAudit(string normalizedPath)
        {
            return ProductionIgnoredPathFragments.Any(fragment =>
                normalizedPath.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Converts raw AuditHit objects into the full text report written under Temp/.
        /// Findings are grouped by legacy term, largest groups first, then each group's occurrences are ordered
        /// by file path and line number. This answers both "what contaminates the project most?" and
        /// "exactly where are all occurrences?".
        /// </summary>
        private static string BuildReport(int scannedFiles, IReadOnlyCollection<AuditHit> hits, bool includeHistoricalBackups)
        {
            // StringBuilder avoids repeatedly allocating ever-larger immutable strings while constructing the report.
            var builder = new StringBuilder();
            builder.AppendLine("GRADOMRAZ — LEGACY IDENTITY AUDIT");
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Scope: {(includeHistoricalBackups ? "Assets + ProjectSettings including backups" : "production Assets + ProjectSettings")}");
            builder.AppendLine($"Files scanned: {scannedFiles}");
            builder.AppendLine($"Legacy references: {hits.Count}");
            builder.AppendLine();

            // Group terms case-insensitively, show most frequent legacy identities first.
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

        /// <summary>
        /// Converts an absolute filesystem path into a project-relative path for readable reports.
        /// Uri is used here because it handles path-segment relative conversion reliably; the final Replace converts
        /// URI forward slashes back to the current operating system's directory separator.
        /// </summary>
        private static string MakeRelative(string root, string path)
        {
            // A directory URI must end with a slash/separator or URI relative-resolution can treat its last segment as a file.
            var rootUri = new Uri(AppendDirectorySeparator(root));
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Guarantees that a directory path ends in the platform's directory separator.
        /// Does not duplicate it if the path already ends correctly.
        /// </summary>
        private static string AppendDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Canonicalizes separators ONLY for the audit's string-based ignore matching.
        /// It does not touch the real file on disk and is not intended as a general filesystem normalization routine.
        /// </summary>
        private static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Immutable data record for ONE occurrence of ONE legacy term on ONE source line.
        /// Being a readonly struct keeps the finding simple: once created, its path/line/term/text cannot be mutated.
        /// </summary>
        private readonly struct AuditHit
        {
            /// <summary>Create a fully self-contained finding used by Console and report generation.</summary>
            public AuditHit(string relativePath, int lineNumber, string term, string line)
            {
                RelativePath = relativePath;
                LineNumber = lineNumber;
                Term = term;
                Line = line;
            }

            /// <summary>Project-relative file location, e.g. Assets/Scenes/Foo.unity.</summary>
            public string RelativePath { get; }

            /// <summary>1-based line number within the scanned text file.</summary>
            public int LineNumber { get; }

            /// <summary>The exact LegacyTerms entry that matched this line.</summary>
            public string Term { get; }

            /// <summary>Original full source line, preserved so the report shows textual context.</summary>
            public string Line { get; }
        }
    }
}
