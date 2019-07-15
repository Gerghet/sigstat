﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SigStat.Common.Framework.Samplers
{
    /// <summary>
    /// Universal sampler for training on n odd indexed genuine signatures.
    /// </summary>ss
    public class OddNSampler : Sampler
    {
        public int N { get; set; }
        public OddNSampler(int n = 10) : base(null,null,null)
        {
            N = n;
            references = sl => sl.Where(s => s.Origin == Origin.Genuine).Where((s, i) => i % 2 != 0).Take(N).ToList();
            genuineTests = sl => sl.Where(s => s.Origin == Origin.Genuine).Where((s, i) => i % 2 == 0).ToList();
            forgeryTests = sl => sl.Where(s => s.Origin == Origin.Forged).ToList();
        }
    }
}
