﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SigStat.Common
{
    public interface IPipelineItem
    {

    }

    public interface ITransformation : IPipelineItem
    {
        void Transform(Signature signature);
    }

    public interface IClassification : IPipelineItem
    {
        void Train(IEnumerable<Signature> signatures);
        void Test(Signature signature);
    }
}
