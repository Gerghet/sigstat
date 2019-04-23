﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SigStat.Common;
using SigStat.Common.Algorithms;
using SigStat.Common.Loaders;
using System;
using System.Collections.Generic;

namespace SigStat.Common.Test.Loader
{
    [TestClass]
    public class Svc2004LoaderTest
    {
        [TestMethod]
        public void TestLoadSignatureFromFile()
        {
            
            Svc2004Loader loader = new Svc2004Loader(@"Databases\Test\SVC2004\Test.zip", true);
            var signers = new List<Signer>(loader.EnumerateSigners(p => p.ID == "01"));//Load the first signer only
            string id = signers[0].ID;
            string id2 = signers[0].Signatures[0].ID;
            Signer signerFromSignature = signers[0].Signatures[0].Signer;
            Assert.AreEqual(id,"01");
            Assert.AreEqual(id2, "01");
            Assert.AreSame(signerFromSignature, signers[0]); 
            
           //Program.cs->line 340
        }
        [TestMethod]
        public void TestLoadSignatureFromStream()
        {
         //   public static void LoadSignature(Signature signature, Stream stream, bool standardFeatures)
        }

        [TestMethod]
        public void TestParseSignature()
        {
          //  private static void ParseSignature(Signature signature, string[] linesArray, bool standardFeatures)
        }
            
        
    }
}
