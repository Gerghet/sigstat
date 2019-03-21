﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SigStat.Common.Loaders
{
    public class SigComp11DutchLoader : DataSetLoader
    {
        /// <summary>
        /// Set of features containing raw data loaded from MCYT-format database.
        /// </summary>
        public static class SigComp11
        {
            /// <summary>
            /// X cooridnates from the online signature imported from the MCYT database
            /// </summary>
            public static readonly FeatureDescriptor<List<int>> X = FeatureDescriptor.Get<List<int>>("SigComp11.X");
            /// <summary>
            /// Y cooridnates from the online signature imported from the MCYT database
            /// </summary>
            public static readonly FeatureDescriptor<List<int>> Y = FeatureDescriptor.Get<List<int>>("SigComp11.Y");
            /// <summary>
            /// Z cooridnates from the online signature imported from the MCYT database
            /// </summary>
            public static readonly FeatureDescriptor<List<int>> Z = FeatureDescriptor.Get<List<int>>("SigComp11.Z");
            /// <summary>
            /// T values from the online signature imported from the SVC2004 database
            /// </summary>
            public static readonly FeatureDescriptor<List<int>> T = FeatureDescriptor.Get<List<int>>("SigComp11.T");
        }

        private struct SigComp11DutchSignatureFile
        {
            public string FilePath { get; set; }
            public string SignerID { get; set; }
            public string SignatureIndex { get; set; }
            public string ForgerID { get; set; }
            public string SignatureID { get; set; }


            public SigComp11DutchSignatureFile(string filepath)
            {
                // TODO: Support original filename format
                FilePath = filepath;
                SignatureID = Path.GetFileNameWithoutExtension(filepath.Split('/').Last());//handle if file is in zip directory

                var parts = SignatureID.Split('_');
                if (parts.Length == 2)
                {
                    SignerID = parts[0];
                    SignatureIndex = parts[1];
                    ForgerID = null;
                }
                else if (parts.Length == 3)
                {
                    SignerID = parts[0];
                    SignatureIndex = parts[2];
                    ForgerID = parts[1];
                }
                else
                    throw new NotSupportedException($"Unsupported filename format '{SignatureID}'");
            }
        }

        public SigComp11DutchLoader(string databasePath, bool standardFeatures)
        {
            DatabasePath = databasePath;
            StandardFeatures = standardFeatures;
        }

        private string DatabasePath { get; }
        private bool StandardFeatures { get; }

        public override IEnumerable<Signer> EnumerateSigners(Predicate<Signer> signerFilter)
        {
            //TODO: EnumerateSigners should ba able to operate with a directory path, not just a zip file
            //signerFilter = signerFilter ?? SignerFilter;

            this.LogInformation("Enumerating signers started.");
            using (ZipArchive zip = ZipFile.OpenRead(DatabasePath))
            {
                //cut names if the files are in directories
                var signatureGroups = zip.Entries.Where(f => f.Name.EndsWith(".hwr")).Select(f => new SigComp11DutchSignatureFile(f.FullName)).GroupBy(sf => sf.SignerID);
                this.LogTrace(signatureGroups.Count().ToString() + " signers found in database");
                foreach (var group in signatureGroups)
                {
                    Signer signer = new Signer { ID = group.Key };

                    if (signerFilter != null && !signerFilter(signer))
                    {
                        continue;
                    }
                    foreach (var signatureFile in group)
                    {
                        Signature signature = new Signature
                        {
                            Signer = signer,
                            ID = signatureFile.SignatureID,
                            Origin = signatureFile.ForgerID == null ? Origin.Genuine : Origin.Forged
                        };
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (Stream s = zip.GetEntry(signatureFile.FilePath).Open())
                            {
                                s.CopyTo(ms);//must use memory stream to use Seek()
                            }
                            ms.Position = 0;//needed after CopyTo
                            LoadSignature(signature, ms, StandardFeatures);
                        }
                        signer.Signatures.Add(signature);

                    }
                    signer.Signatures = signer.Signatures.OrderBy(s => s.ID).ToList();
                    yield return signer;
                }
            }
            this.LogInformation("Enumerating signers finished.");
        }

        /// <summary>
        /// Loads one signature from specified stream.
        /// </summary>
        /// <remarks>
        /// Based on Mohammad's MCYT reader.
        /// </remarks>
        /// <param name="signature">Signature to write features to.</param>
        /// <param name="stream">Stream to read MCYT data from.</param>
        /// <param name="standardFeatures">Convert loaded data to standard <see cref="Features"/>.</param>
        public static void LoadSignature(Signature signature, MemoryStream stream, bool standardFeatures)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                ParseSignature(signature, sr.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None), standardFeatures);
            }

        }
        private static void ParseSignature(Signature signature, string[] linesArray, bool standardFeatures)
        {
            var lines = linesArray
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(' ').Select(s => int.Parse(s)).ToArray())
                .ToList();

            signature.SetFeature(SigComp11.X, lines.Select(l => l[0]).ToList());
            signature.SetFeature(SigComp11.Y, lines.Select(l => l[1]).ToList());
            signature.SetFeature(SigComp11.Z, lines.Select(l => l[2]).ToList());
            // Sampling frequency is 200Hz ==> time should be increased by 5 msec for each slot
            signature.SetFeature(SigComp11.T, Enumerable.Range(0, lines.Count).Select(i=>i*5).ToList());

            if (standardFeatures)
            {
                signature.SetFeature(Features.X, lines.Select(l => (double)l[0]).ToList());
                signature.SetFeature(Features.Y, lines.Select(l => (double)l[1]).ToList());
                signature.SetFeature(Features.Pressure, lines.Select(l => (double)l[2]).ToList());
                signature.SetFeature(Features.T, Enumerable.Range(0, lines.Count).Select(i => i * 5d).ToList());
            }
        }

    }
}
