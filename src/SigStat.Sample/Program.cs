﻿
using OfficeOpenXml;
using SigStat.Common;
using SigStat.Common.Framework.Samplers;
using SigStat.Common.Helpers;
using SigStat.Common.Loaders;
using SigStat.Common.Model;
using SigStat.Common.Pipeline;
using SigStat.Common.PipelineItems.Classifiers;
using SigStat.Common.PipelineItems.Transforms.Preprocessing;
using SigStat.Common.Transforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static SigStat.Common.Loaders.MCYTLoader;
using static SigStat.Common.Loaders.SigComp11ChineseLoader;
using static SigStat.Common.Loaders.SigComp13JapaneseLoader;
using SixLabors.Primitives;
using System.Drawing;
using SigStat.Common.Logging;
using SigStat.Benchmark;

namespace SigStat.Sample
{
    class Program
    {

        public static class MyFeatures
        {
            public static readonly FeatureDescriptor<List<Loop>> Loop = FeatureDescriptor.Get<List<Loop>>("Loop");
            public static FeatureDescriptor<bool[,]> Binarized = FeatureDescriptor.Get<bool[,]>("Binarized");
            public static FeatureDescriptor<bool[,]> Skeleton = FeatureDescriptor.Get<bool[,]>("Skeleton");
            public static FeatureDescriptor<List<double>> Tangent = FeatureDescriptor.Get<List<double>>("Tangent");
        }

        class MySignature : Signature
        {
            public List<Loop> Loops { get { return GetFeature(MyFeatures.Loop); } set { SetFeature(MyFeatures.Loop, value); } }
            public SixLabors.Primitives.SizeF Size { get { return GetFeature(Features.Size); } set { SetFeature(Features.Size, value); } }

            public bool[,] Binarized { get { return GetFeature(MyFeatures.Binarized); } set { SetFeature(MyFeatures.Binarized, value); } }
            public List<double> Tangent { get { return GetFeature(MyFeatures.Tangent); } set { SetFeature(MyFeatures.Tangent, value); } }

        }

        public static void Main(string[] args)
        {
            Console.WriteLine("SigStat library sample");



            //SignatureDemo();
            //TransformationPipeline();
            //Classifier();
            //OnlineToImage();
            //DatabaseLoaderDemo();
            //GenerateOfflineDatabase();
            //OfflineVerifierDemo();
            //OnlineVerifierDemo();
            // SignatureToImageTesting();
            //OnlineRotationBenchmarkDemo();
            //SampleRateTestingDemo();
            // SampleRateTestingDemoForSigners();
            //OnlineVerifierBenchmarkDemo();
            //PreprocessingBenchmarkDemo();
            //TestPreprocessingTransformations();
            //JsonSerializeSignature();
            //JsonSerializeOnlineVerifier();
            //JsonSerializeOnlineVerifierBenchmark();
            //ClassificationBenchmark();
            //ExcelReportGeneratorTest();
            PreprocessingBenchmarkDemo();
            Console.WriteLine("Press <<Enter>> to exit.");
            Console.ReadLine();

        }

        private static void ExcelReportGeneratorTest()
        {
            var logger = new ReportInformationLogger();
            var benchmark = new VerifierBenchmark()
            {
                Parameters = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Translation", "none"),
                    new KeyValuePair<string, string>("Classifier", "DtwClassifier"),
                },
                Loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true),
                Verifier = new Verifier()
                {
                    Pipeline = new SequentialTransformPipeline {
                        new Scale() {InputFeature = Features.X, OutputFeature= Features.X },
                        new Scale() {InputFeature = Features.Y, OutputFeature= Features.Y },

                        new FilterPoints() { KeyFeatureInput = Features.Pressure, KeyFeatureOutput = Features.Pressure,
                        InputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y },
                        OutputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y }},
                    },


                    Classifier = new DtwClassifier()
                    {
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure },
                        MultiplicationFactor = 1.7
                    }
                },
                Sampler = new UniversalSampler(3, 10),
                Logger = logger,
            };

            benchmark.ProgressChanged += ProgressPrimary;
            //benchmark.Verifier.ProgressChanged += ProgressSecondary;

            var result = benchmark.Execute(true);

            var model = LogAnalyzer.GetBenchmarkLogModel(logger.GetReportLogs());

            ExcelReportGenerator.GenerateReport(model);


            Console.WriteLine($"AER: {result.FinalResult.Aer}");
        }

        
        private static void SignatureToImageTesting()
        {
            var databaseDir = Environment.GetEnvironmentVariable("SigStatDB");
            Svc2004Loader Loader = new Svc2004Loader(Path.Combine(databaseDir, "SVC2004.zip"), true);
            Signature s1 = Loader.EnumerateSigners(p => (p.ID == "05")).ToList()[0].Signatures[10];//signer 10, signature 10
            Signature s2 = s1;

            var tfs = new SequentialTransformPipeline
            {
                new ParallelTransformPipeline
                {

                   new Normalize() { Input = Features.X, Output = Features.X },
                   new Normalize() { Input = Features.Y, Output = Features.Y },
                },
                new RealisticImageGenerator(1280, 720)
            };
            tfs.Logger = new SimpleConsoleLogger();
            tfs.Transform(s1);

           ImageSaver.Save(s1, $"{s1.Signer.ID }_{s1.ID }_0BeforeTest.png");

            var tfs2 = new SequentialTransformPipeline
            {
                new ParallelTransformPipeline
                {

                   new Normalize() { Input = Features.X, Output = Features.X },
                   new Normalize() { Input = Features.Y, Output = Features.Y },
                   new OrthognalRotation() {InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y}
                },
                new RealisticImageGenerator(1280, 720)
            };
            tfs2.Logger = new SimpleConsoleLogger();
            tfs2.Transform(s2);
            ImageSaver.Save(s2, $"{s2.Signer.ID }_{s2.ID }_AfterNormalizationTest.png");
            int i = 0;
            
            foreach(double y  in s1.GetFeature(Features.Y)) {
                Console.WriteLine("Y1=  " + y + "  Y2= " + s2.GetFeature(Features.Y)[i]+ " dif=  "+(s1.GetFeature(Features.Y)[i] - s2.GetFeature(Features.Y)[i]));
                i++;
            }
           

            }

        private static void SampleRateTestingDemo()
        {
            var p = new ExcelPackage();

            using (p)
            {
                var databaseDir2 = Environment.GetEnvironmentVariable("SigStatDB");
               var Loader = new Svc2004Loader(Path.Combine(databaseDir2, "svc2004.zip"), true);
               List<Signer> signers2 = Loader.EnumerateSigners(null).ToList();
            foreach (Signer s2 in signers2)
            {
                var resultsSheet3 = p.Workbook.Worksheets.Add($"Signer({s2.ID})");
                resultsSheet3.Cells[1, 4].Value = "ID";
                resultsSheet3.Cells[1, 5].Value = "Points";
                resultsSheet3.Cells[1, 6].Value = "S";
                resultsSheet3.Cells[1, 7].Value = "AER";

             
            }

            var samplerate = new List<SampleRateResults>();
            int s = 1;
                for (s = 1; s <= 100; s=s+5)
                {
                    var databaseDir = Environment.GetEnvironmentVariable("SigStatDB");
                    var benchmark = new VerifierBenchmark()
                    {

                        Loader = new Svc2004Loader(Path.Combine(databaseDir, "svc2004.zip"), true),

                        Verifier = new Verifier()
                        {
                            Pipeline = new SequentialTransformPipeline
                    {
            new SampleRate(){samplerate=s,InputX=Features.X, InputY=Features.Y, InputP = Features.Pressure, OutputX = Features.X, OutputY=Features.Y,OutputP=Features.Pressure},
       //                   new SampleRate(){samplerate=s,InputX=Features.X, InputY=Features.Y, InputP = Features.Pressure, OutputX = Features.X, OutputY=Features.Y},

       //      new OrthognalRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},
     //   new Scale() {InputFeature = Features.X, OutputFeature = Features.X},
    //   new Scale() {InputFeature = Features.Y, OutputFeature = Features.Y},
     //    new TranslatePreproc(OriginType.CenterOfGravity){InputFeature = Features.X, OutputFeature = Features.X},
      //      new TranslatePreproc(OriginType.CenterOfGravity){InputFeature = Features.Y, OutputFeature = Features.Y},
       //      new NormalizeRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},
            new ZNormalization(){InputFeature = Features.X, OutputFeature = Features.X},
             new ZNormalization(){InputFeature = Features.Y, OutputFeature = Features.Y},
       //      new ZNormalization(){InputFeature = Features.Pressure, OutputFeature = Features.Pressure},
       //      new ZNormalization(){InputFeature = Features.Pressure, OutputFeature = Features.Pressure}
                        }
                        ,
                            Classifier = new OptimalDtwClassifier()
                            {
                                DistanceFunction = Accord.Math.Distance.Euclidean,
                                Sampler = new EvenNSampler(10),
                               // Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure }
                                Features = new List<FeatureDescriptor>() { Features.X, Features.Y}
                            }
                        },
                        Sampler = new EvenNSampler(10),
                        // OddNSampler 
                        Logger = new SimpleConsoleLogger(),

                    };
                    
                    benchmark.ProgressChanged += ProgressPrimary;

                    var result = benchmark.Execute(true);

                    List<Signer> signers = benchmark.Loader.EnumerateSigners(null).ToList();
                    double avg = 0;
                    int ii = 0;
                    foreach (Signer sig in signers)
                    {
                        avg = avg+ SignerStatisticsHelper.GetPointsAvg(sig);
                       
                         var Sheet = p.Workbook.Worksheets.Single(se => se.Name == $"Signer({sig.ID})");
                        Sheet.Cells[s + 1, 4].Value = sig.ID;
                       Sheet.Cells[s + 1, 5].Value = (avg/s);
                        Sheet.Cells[s + 1, 6].Value = Loader.SamplingFrequency / s;
                        Sheet.Cells[s + 1, 7].Value = result.SignerResults[ii].Aer;

                        ii++;
                    }
                    avg = avg / signers.Count();
                    var samplingrate = Loader.SamplingFrequency;

                    var obj = new SampleRateResults();
                    obj.step = s; obj.AER = result.FinalResult.Aer; obj.samplerate = samplingrate / s;
                    obj.pointsAvg = avg / obj.step;
                    samplerate.Add(obj);


                }
               
                    var summarySheet = p.Workbook.Worksheets.Add("Summary");
                    summarySheet.Cells[2, 2].Value = "SampleRate testing on Chinese_ translation and scaling (X,y) applied_ xy features, Even sampler";
                    summarySheet.Cells[3, 2].Value = DateTime.Now.ToString();

                    var resultsSheet = p.Workbook.Worksheets.Add("Results");
                    resultsSheet.InsertTable(1, 1, samplerate);

                    var resultsSheet2 = p.Workbook.Worksheets.Add("Signers_Statistics");
                    resultsSheet2.Cells[1, 4].Value = "ID";
                    resultsSheet2.Cells[1, 5].Value = "signerPointsAvg";
                    resultsSheet2.Cells[1, 6].Value = "LegthAvg";
                    resultsSheet2.Cells[1, 7].Value = "pointsNumMin";
                    resultsSheet2.Cells[1, 8].Value = "pointsNumMax";
                    resultsSheet2.Cells[1, 9].Value = "widthAvg";
                    resultsSheet2.Cells[1, 10].Value = "heightAvg";

                int i = 2;
                foreach (Signer s2 in signers2)
                {
                    resultsSheet2.Cells[i, 4].Value = s2.ID;
                    resultsSheet2.Cells[i, 5].Value = SignerStatisticsHelper.GetPointsAvg(s2);
                    resultsSheet2.Cells[i, 6].Value = SignerStatisticsHelper.GetLengthAverage(s2);
                    resultsSheet2.Cells[i, 7].Value = SignerStatisticsHelper.GetMinSignaturePoints(s2);
                    resultsSheet2.Cells[i, 8].Value = SignerStatisticsHelper.GetMaxSignaturePoints(s2);
                    resultsSheet2.Cells[i, 9].Value = s2.GetWidthAvg();
                    resultsSheet2.Cells[i, 10].Value = s2.GetHeightAvg();

                    i++;
                }


                p.SaveAs(new FileInfo("Report.xlsx"));
                }
            }


        



        private static void RenderDatabase()
        {
            SigComp11DutchLoader loader = new SigComp11DutchLoader("Dutch_renamed.zip", true);
            var signatures = loader.EnumerateSigners().Take(10);
            List<Signer> signers = loader.EnumerateSigners(null).ToList();

            var pipeline = new SequentialTransformPipeline
            {
                new UniformScale() { BaseDimension=Features.X, ProportionalDimension = Features.Y, BaseDimensionOutput=Features.X, ProportionalDimensionOutput=Features.Y },
                new Normalize() { Input=Features.Pressure, Output=Features.Pressure },
                new RealisticImageGenerator(1280, 720),
            };
            pipeline.Logger = new SimpleConsoleLogger();

            foreach (var signer in signers)
            {
                foreach (var signature in signer.Signatures)
                {
                    pipeline.Transform(signature);
                    ImageSaver.Save(signature, signature.ID + ".png");
                }
            }
            return;
        }
        static string rulesString = @" [Benchmark] -> [Database]_[Split]_[Feature]_[Verifier]
                [Database] -> svc2004 | mcyt100 | dutch | chinese | japanese
                [Feature] -> X | Y | P | XY | XYP | XP | YP
                [Split] -> s1 | s2 | s3 | s4
                [Verifier] -> [Pipeline]_[Classifier]
                [Classifier] -> Dtw_[Distance] | OptimalDtw_[Distance]
                [Distance] -> Manhattan | Euclidean
                [Pipeline] -> [Rotation]_[Gap]_[Resampling]_[Scaling]_[Translation]
                [Rotation] -> none | rotation
                [Gap] -> [FilterGap]_[FillGap]
                [FilterGap] -> none | filter 
                [FillGap] -> none | fill_[FillInterpolation]
                [FillInterpolation] -> linear | cubic
                [Resampling] -> none | [SampleCount]samples_[ResamplingInterpolation]
                [SampleCount] ->  50 | 100 | 500 | 1000
                [ResamplingInterpolation] -> linear | cubic
                [Scaling] -> none | scale1| scaleS
                [Translation] -> none | to0 | toCog";

        private static void PreprocessingBenchmarkDemo()
        {
            string configString = "svc2004_s3_Y_none_filter_fill_linear_1000samples_cubic_scale1_to0_OptimalDtw_Manhattan";
            var rules = GrammarEngine.ParseRules(rulesString);
            var configDict = GrammarEngine.ParseSentence(configString, rules);
            var builder = new Benchmark.BenchmarkBuilder(Environment.GetEnvironmentVariable("SigStatDB")); // feltételezzük ,hog
            var benchmark = builder.Build(configDict);

            benchmark.Logger = new SimpleConsoleLogger(Microsoft.Extensions.Logging.LogLevel.Trace);
            benchmark.Execute(false);
        }

        public static void SignatureDemo()
        {
            // Create a signature instance and initialize main properties
            Signature sig = new Signature();
            sig.ID = "Demo";
            sig.Origin = Origin.Genuine;

            // Set/Get feature value, using a string key
            sig["Height"] = 5;
            var height = (int)sig["Height"];

            // Set/Get feature value, using a generic method and a sting key
            sig.SetFeature<int>("Height", 5);
            height = sig.GetFeature<int>("Height");

            // Register a feature descriptor
            FeatureDescriptor<int> heightDescriptor = FeatureDescriptor.Get<int>("Height");

            // Set/Get feature value, using a generic feature descriptor
            // Note that no casting is required!
            sig.SetFeature(heightDescriptor, 5);
            height = sig.GetFeature(heightDescriptor);

            // Define complex feature values
            var loops = new List<Loop>() { new Loop(1, 1), new Loop(3, 3) };

            // Reusing feature descriptors (see MyFeatures class for details)
            sig.SetFeature(MyFeatures.Loop, loops);
            loops = sig.GetFeature(MyFeatures.Loop);

            // Wrap features into properties, see (MySignature class for details)
            MySignature mySig = new MySignature();
            mySig.Loops = loops;
            loops = mySig.Loops;

            // Enumerate all the features in a signature
            foreach (var descriptor in sig.GetFeatureDescriptors())
            {
                if (!descriptor.IsCollection)
                {
                    Console.WriteLine($"{descriptor.Name}: {sig[descriptor]}");
                }
                else
                {
                    Console.WriteLine($"{descriptor}:");
                    var items = (IList)sig[descriptor];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Console.WriteLine($" {i}.) {items[i]}");
                    }
                }
            }

        }

        static void DatabaseLoaderDemo()
        {
            var databaseDir = Environment.GetEnvironmentVariable("SigStatDB");
            //Load signatures from local database
            //SigComp15GermanLoader loader = new SigComp15GermanLoader(Path.Combine(databaseDir, "SigWiComp2015_German.zip").GetPath(), true);
            //SigComp15GermanLoader loader = new SigComp15GermanLoader(@"Databases\SigWiComp2015_German.zip".GetPath(), true);
            //SigComp11ChineseLoader loader = new SigComp11ChineseLoader(Path.Combine(databaseDir, "SigComp11Chinese.zip").GetPath(), true);
            SigComp13JapaneseLoader loader = new SigComp13JapaneseLoader(Path.Combine(databaseDir, "SigWiComp2013_Japanese.zip").GetPath(), true);
            var signers = loader.EnumerateSigners().ToList();
            Console.WriteLine($"{signers.Count} signers loaded with {signers.SelectMany(s => s.Signatures).Count()} signatures");
        }

        static void LoadSignaturesFromDatabase(out List<Signature> genuines, out Signature challenge)
        {
            //Load signatures from local database
            Svc2004Loader loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true);
            var signer = new List<Signer>(loader.EnumerateSigners(p => p.ID == "01"))[0];//Load the first signer only
            genuines = signer.Signatures.Where(s => s.Origin == Origin.Genuine).ToList();
            challenge = genuines[0];
        }

        static void TransformationPipeline()
        {
            Signature offlineSignature = ImageLoader.LoadSignature(@"Databases\Offline\Images\004_e_001.png".GetPath());

            // Initialize a transformation using object initializer
            var resize = new Resize()
            {
                Width = 60,
                InputImage = Features.Image,
                OutputImage = Features.Image
            };
            // Perform transformation
            resize.Transform(offlineSignature);

            // Initialize a transformation using fluent syntax
            var binarization = new Binarization()
            {
                InputImage = Features.Image,
                OutputMask = MyFeatures.Binarized
            };

            // Perform transformation
            binarization.Transform(offlineSignature);

            // Consume results
            var binaryImage = offlineSignature.GetFeature(MyFeatures.Binarized);
            WriteToConsole(binaryImage);

            // Create signature with online features
            Signature sig = new Signature("Demo");
            sig.SetFeature(Features.X, new List<double> { 1, 1, 2, 2, 2, 2, 1, 1 });
            var x = sig.GetFeature(Features.X);
            Console.WriteLine(string.Join(", ", x));

            // Connect a series of transformations into a pipeline
            SequentialTransformPipeline pipeline = new SequentialTransformPipeline()
            {
                new Multiply(2) { InputList = Features.X },
                new AddConst(3), // no need to specify input, it will work with the output of the previous transformation
            };

            pipeline.Transform(sig);
            Console.WriteLine(string.Join(", ", x));
        }

        static void Classifier()
        {
            LoadSignaturesFromDatabase(out var genuines, out var challenge);

            IClassifier classifier = new DtwClassifier()
            {
                Features = { Features.X, Features.Y, Features.T }
            };
            ISignerModel model = classifier.Train(genuines);//Train on genuine signatures
            var result = classifier.Test(model, challenge);

            Console.WriteLine("Classification result: " + (result > 0.5 ? "Genuine" : "Forged"));
        }


        /// <summary>
        /// Read signature from image, extract features, generate new image
        /// </summary>
        static void OfflineVerifierDemo()
        {
            var verifier = new Verifier(new SimpleConsoleLogger())
            {
                Pipeline = new SequentialTransformPipeline
                {
                    new Binarization(){
                        InputImage = Features.Image
                    },
                    new Trim(5),
                    new ImageGenerator(true),
                    new HSCPThinning(),
                    new ImageGenerator(true),
                    new OnePixelThinning() { Output = MyFeatures.Skeleton },
                    new ImageGenerator(true),
                    //new BaselineExtraction(),
                    //new LoopExtraction(),
                    new EndpointExtraction(),
                    new ComponentExtraction(5) { Skeleton = MyFeatures.Skeleton },
                    new ComponentSorter(),
                    new ComponentsToFeatures(),
                    new ParallelTransformPipeline
                    {
                        new Normalize() { Input = Features.X },
                        new Normalize() { Input = Features.Y }
                    },
                    new ApproximateOnlineFeatures(),
                    new RealisticImageGenerator(1280, 720),
                },
                Classifier = new WeightedClassifier()
            };

            Signature s1 = ImageLoader.LoadSignature(@"Databases\Offline\Images\U1S1.png".GetPath());
            s1.Origin = Origin.Genuine;
            Signer s = new Signer();
            s.Signatures.Add(s1);

            verifier.Train(new List<Signature> { s1 });

            //TODO: ha mar Verifier demo, akkor Test()-et is hasznaljuk..
            ImageSaver.Save(s1, @"GeneratedOfflineImage.png");
        }

        static void OnlineVerifierDemo()
        {
            var timer1 = FeatureDescriptor.Register("Timer1", typeof(DateTime));//TODO: <DateTime> template-tel mukodjon

            var verifier = new Verifier(new SimpleConsoleLogger())
            {
                Pipeline = new SequentialTransformPipeline
                {
                    new ParallelTransformPipeline
                    {
                        new Normalize() { Input = Features.Pressure },
                        new Map(0, 1) { Input = Features.X },
                        new Map(0, 1) { Input = Features.Y },
                        //new TimeReset(),
                    },
                    //new CentroidTranslate(),//is a sequential pipeline of other building blocks
                    //new TangentExtraction(),
                    /*new AlignmentNormalization(Alignment.Origin),
                    new Paper13FeatureExtractor(),*/

                },
                Classifier = new DtwClassifier()
                {
                    Features = { Features.Pressure }
                }
                //Classifier = new WeightedClassifier
                //{
                //    {
                //        (new DtwClassifier(Accord.Math.Distance.Manhattan)
                //        {
                //            Features = { Features.X, Features.Y }
                //        },
                //           0.15)
                //    },
                //    {
                //        (new DtwClassifier(){
                //            Features = { Features.Pressure }
                //        }, 0.3)
                //    },
                //    {
                //        (new DtwClassifier(){
                //            Features = { MyFeatures.Tangent }
                //        }, 0.55)
                //    },
                //    //{
                //    //    (new MultiDimensionKolmogorovSmirnovClassifier
                //    //    {
                //    //        Features = {"X", "Y" },
                //    //        ThresholdStrategy = ThresholdStrategies.AveragePlusDeviance
                //    //    },
                //    //    0.8)
                //    //}
                //}
            };

            Svc2004Loader loader = new Svc2004Loader(@"Databases\ger.rar".GetPath(), true);
            var signers = new List<Signer>(loader.EnumerateSigners(p => p.ID == "01"));//Load the first signer only

            List<Signature> references = signers[0].Signatures.GetRange(0, 10);
            verifier.Train(references);

            Signature questioned1 = signers[0].Signatures[0];
            Signature questioned2 = signers[0].Signatures[25];
            bool isGenuine1 = verifier.Test(questioned1) > 0.5;//true
            bool isGenuine2 = verifier.Test(questioned2) > 0.5;//false
        }

        static void OnlineRotationBenchmarkDemo()
        {

            var databaseDir = Environment.GetEnvironmentVariable("SigStatDB");
            var benchmark = new VerifierBenchmark()
            {

                //Loader = new SigComp11ChineseLoader(Path.Combine(databaseDir, "SigComp11Chinese.zip"), true),
                Loader = new Svc2004Loader(Path.Combine(databaseDir, "SVC2004.zip"), true),

                Verifier = new Verifier()
                {
                    Pipeline = new SequentialTransformPipeline
                    {
                                             //  new OrthognalRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},


                             new Scale() {InputFeature = Features.X, OutputFeature = Features.X},
                               new Scale() {InputFeature = Features.Y, OutputFeature = Features.Y},
                            new TranslatePreproc(OriginType.CenterOfGravity){InputFeature = Features.X, OutputFeature = Features.X},
                            new TranslatePreproc(OriginType.CenterOfGravity){InputFeature = Features.Y, OutputFeature = Features.Y},
                      //new NormalizeRotationForX(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},

                    new NormalizeRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},


                        }
                ,
                    Classifier = new OptimalDtwClassifier()
                    {
                        DistanceFunction = Accord.Math.Distance.Euclidean,
                        Sampler = new FirstNSampler(10),
                        Features = new List<FeatureDescriptor>() { Features.X }
                    }
                },
                Sampler = new FirstNSampler(10),
                Logger = new SimpleConsoleLogger(),
            };


            benchmark.ProgressChanged += ProgressPrimary;
            //benchmark.Verifier.ProgressChanged += ProgressSecondary;

            //var result = benchmark.Execute(true);

            //benchmark.Dump("results.xlsx", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("chinese11", "scale, COG") });
            //using (var p = new ExcelPackage())
            //{
            //    var summarySheet = p.Workbook.Worksheets.Add("Summary");
            //    summarySheet.Cells[2, 2].Value = "Preprocessing benchmark";
            //    summarySheet.Cells[3, 2].Value = DateTime.Now.ToString();

            //    var resultsSheet = p.Workbook.Worksheets.Add("Results");
            //   resultsSheet.InsertTable(1, 1, result.SignerResults);

            //    resultsSheet.InsertTable(1, 1, new object[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });

            //    p.SaveAs(new FileInfo("Report.xlsx"));
            //}

            var signers = new Svc2004Loader(Path.Combine(databaseDir, "SVC2004.zip"), true).EnumerateSigners().ToList();
            //signers[2].Signatures;
            SignatureHelper.SaveImage(signers[1].Signatures[2], "signature1-2.png");


            //foreach (var signerResult in result.SignerResults)
            //{
            //    Console.WriteLine($"{signerResult.Signer} {signerResult.Aer}");
            //}
            //Console.WriteLine($"AER: {result.FinalResult.Aer}");

        }

        static void OnlineVerifierBenchmarkDemo()
        {
            var databaseDir = Environment.GetEnvironmentVariable("SigStatDB");
            var benchmark = new VerifierBenchmark()
            {
                Loader = new SigComp13JapaneseLoader(Path.Combine(databaseDir, "SigWiComp2013_Japanese.zip").GetPath(), true),
                Verifier = new Verifier()
                {
                    Pipeline = new SequentialTransformPipeline
                    {
                        //new TranslatePreproc(OriginType.CenterOfGravity) {InputFeature = Features.X, OutputFeature=Features.X },
                        //new TranslatePreproc(OriginType.CenterOfGravity) {InputFeature = Features.Y, OutputFeature=Features.Y },
                        //new UniformScale() {BaseDimension = Features.X, ProportionalDimension = Features.Y, BaseDimensionOutput = Features.X, ProportionalDimensionOutput = Features.Y},
                        //new NormalizeRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},

                        // new Scale() {InputFeature = Features.X, OutputFeature = Features.X},
                        //   new Scale() {InputFeature = Features.Y, OutputFeature = Features.Y},
                        //new ResampleSamplesCountBased() {
                        //    InputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y, Features.Pressure },
                        //    OutputFeatures = new List<FeatureDescriptor<List<double>>>() {Features.X, Features.Y, Features.Pressure},
                        //    InterpolationType = typeof(CubicInterpolation),
                        //    NumOfSamples = 500,
                        //    OriginalTFeature = Features.T,
                        //    ResampledTFeature = Features.T,
                        //},
                        //new FilterPoints() { KeyFeatureInput = Features.Pressure, KeyFeatureOutput = Features.Pressure,
                        //InputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y },
                        //OutputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y }},
                        //new FillPenUpDurations()
                        //{
                        //    InputFeatures = new List<FeatureDescriptor<List<double>>>(){ Features.X, Features.Y, Features.Pressure },
                        //    OutputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y, Features.Pressure },
                        //    InterpolationType = typeof(CubicInterpolation),
                        //    TimeInputFeature =Features.T,
                        //    TimeOutputFeature = Features.T
                        //}
                    }
                ,
                    Classifier = new OptimalDtwClassifier()
                    {
                        Sampler = new FirstNSampler(10),
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure }
                    }
                },
                Sampler = new FirstNSampler(10),
                Logger = new SimpleConsoleLogger(),
            };

            benchmark.ProgressChanged += ProgressPrimary;
            //benchmark.Verifier.ProgressChanged += ProgressSecondary;

            var result = benchmark.Execute(true);

            Console.WriteLine($"AER: {result.FinalResult.Aer}");
        }

        static void OnlineToImage()
        {
            Svc2004Loader loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true);
            Signature s1 = loader.EnumerateSigners(p => (p.ID == "10")).ToList()[0].Signatures[10];//signer 10, signature 10

            var tfs = new SequentialTransformPipeline
            {
                new ParallelTransformPipeline
                {
                    new Normalize() { Input = Features.X, Output = Features.X },
                    new Normalize() { Input = Features.Y, Output = Features.Y }
                },
                new RealisticImageGenerator(1280, 720)
            };
            tfs.Logger = new SimpleConsoleLogger();
            tfs.Transform(s1);

            ImageSaver.Save(s1, @"GeneratedOnlineImage.png");
        }

        static void GenerateOfflineDatabase()
        {
            string path = @"Databases\Offline\Generated\".GetPath();
            Directory.CreateDirectory(path);

            //Svc2004Loader loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip", true);
            MCYTLoader loader = new MCYTLoader(@"Databases\Online\MCYT100\MCYT_Signature_100.zip".GetPath(), true);

            List<Signer> signers = loader.EnumerateSigners(null).ToList();

            var pipeline = new SequentialTransformPipeline
            {
                new UniformScale() { BaseDimension=Features.X, ProportionalDimension = Features.Y, BaseDimensionOutput=Features.X, ProportionalDimensionOutput=Features.Y },
                new Normalize() { Input=Features.Pressure, Output=Features.Pressure },
                new Normalize() { Input=Features.Altitude, Output=Features.Altitude },
                new Map(0, Math.PI/2) { Input=Features.Altitude, Output=Features.Altitude },
                new Map(0, 2*Math.PI) { Input=Features.Azimuth, Output=Features.Azimuth },
                new RealisticImageGenerator(1280, 720),
            };
            pipeline.Logger = new SimpleConsoleLogger();

            for (int i = 0; i < signers.Count; i++)
            {
                for (int j = 0; j < signers[i].Signatures.Count; j++)
                {
                    pipeline.Transform(signers[i].Signatures[j]);
                    ImageSaver.Save(signers[i].Signatures[j], path + $"U{i + 1}S{j + 1}.png");
                    ProgressSecondary(null, (int)(j / (double)signers[i].Signatures.Count * 100.0));
                }
                Console.WriteLine($"Signer{signers[i].ID} ({i + 1}/{signers.Count}) signature images generated.");
                ProgressPrimary(null, (int)(i / (double)signers.Count * 100.0));
            }


        }

        static void ClassificationBenchmark()
        {
            var benchmark = new VerifierBenchmark()
            {
                Loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true),
                Verifier = new Verifier()
                {
                    Pipeline = new SequentialTransformPipeline {
                        new Scale() {InputFeature = Features.X, OutputFeature= Features.X },
                        new Scale() {InputFeature = Features.Y, OutputFeature= Features.Y },

                        new FilterPoints() { KeyFeatureInput = Features.Pressure, KeyFeatureOutput = Features.Pressure,
                        InputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y },
                        OutputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y }},
                    },

                    Classifier = new DtwClassifier()
                    {
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure },
                        MultiplicationFactor = 1.7
                    }
                },
                Sampler = new UniversalSampler(3, 10),
                Logger = new SimpleConsoleLogger(),
            };

            benchmark.ProgressChanged += ProgressPrimary;
            //benchmark.Verifier.ProgressChanged += ProgressSecondary;

            var result = benchmark.Execute(true);

            Console.WriteLine($"AER: {result.FinalResult.Aer}");

        }

        //static void TestPreprocessingTransformations()
        //{
        //    Svc2004Loader loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true);
        //    var signer = new List<Signer>(loader.EnumerateSigners(p => p.ID == "32"))[0];//Load the first signer only

        //    Signature signature = signer.Signatures[13];

        //    string selectedTransformation = "Translate";
        //    //string selectedTransformation = "UniformScale";
        //    //string selectedTransformation = "Scale";
        //    //string selectedTransformation = "NormalizeRotation";
        //    //string selectedTransformation = "ResampleTimeBased";
        //    //string selectedTransformation = "ResampleSamplesCountBased";
        //    //string selectedTransformation = "FillPenUpDurations";
        //    //string selectedTransformation = "FilterPoints";

        //    if (selectedTransformation == "Translate")
        //    {
        //        var originalValues = signature.GetFeature(Features.X);

        //        new TranslatePreproc(OriginType.CenterOfGravity)
        //        {
        //            InputFeature = Features.X,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>(OriginType.CenterOfGravity + "TranslationResult"),
        //        }.Transform(signature);
        //        var cogValues = signature.GetFeature<List<double>>(OriginType.CenterOfGravity + "TranslationResult");

        //        new TranslatePreproc(OriginType.Minimum)
        //        {
        //            InputFeature = Features.X,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>(OriginType.Minimum + "TranslationResult")
        //        }.Transform(signature);
        //        var minValues = signature.GetFeature<List<double>>(OriginType.Minimum + "TranslationResult");

        //        new TranslatePreproc(OriginType.Maximum)
        //        {
        //            InputFeature = Features.X,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>(OriginType.Maximum + "TranslationResult")
        //        }.Transform(signature);
        //        var maxValues = signature.GetFeature<List<double>>(OriginType.Maximum + "TranslationResult");

        //        new TranslatePreproc(100.0)
        //        {
        //            InputFeature = Features.X,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>(OriginType.Predefined + "TranslationResult")
        //        }.Transform(signature);
        //        var predefValues = signature.GetFeature<List<double>>(OriginType.Predefined + "TranslationResult");

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("Original ; COG translated ; Min translated ; Max translated ; 100.0 translated");
        //        for (int i = 0; i < signature.GetFeature(Features.X).Count; i++)
        //        {
        //            sw.WriteLine($"{originalValues[i]};{cogValues[i]};{minValues[i]};{maxValues[i]};{predefValues[i]}");
        //        }
        //        sw.Close();

        //        //TODO: .net core-ban ez így nem működik
        //        //Process.Start(outputFileName);
        //    }
        //    else if (selectedTransformation == "UniformScale")
        //    {
        //        var originalXValues = signature.GetFeature(Features.X);
        //        var originalYValues = signature.GetFeature(Features.Y);


        //        new UniformScale()
        //        {
        //            BaseDimension = Features.X,
        //            ProportionalDimension = Features.Y,
        //            NewMinBaseValue = 100,
        //            NewMaxBaseValue = 200,
        //            NewMinProportionalValue = 150,
        //            BaseDimensionOutput = FeatureDescriptor.Get<List<double>>("UniformScalingResultBaseDim"),
        //            ProportionalDimensionOutput = FeatureDescriptor.Get<List<double>>("UniformScalingResultProportionalDim")
        //        }.Transform(signature);
        //        var defScaledXValues = new List<double>(signature.GetFeature<List<double>>("UniformScalingResultBaseDim"));
        //        var defScaledYValues = new List<double>(signature.GetFeature<List<double>>("UniformScalingResultProportionalDim"));


        //        new UniformScale
        //        {
        //            BaseDimension = Features.X,
        //            ProportionalDimension = Features.Y,
        //            BaseDimensionOutput = FeatureDescriptor.Get<List<double>>("UniformScalingResultBaseDim"),
        //            ProportionalDimensionOutput = FeatureDescriptor.Get<List<double>>("UniformScalingResultProportionalDim")
        //        }.Transform(signature);
        //        var autoScaledXValues = new List<double>(signature.GetFeature<List<double>>("UniformScalingResultBaseDim"));
        //        var autoScaledYValues = new List<double>(signature.GetFeature<List<double>>("UniformScalingResultProportionalDim"));

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalX; OriginalY ; DefScaledX ; DefScaledY ; AutoScaledX; AutoScaledY");
        //        for (int i = 0; i < signature.GetFeature(Features.X).Count; i++)
        //        {
        //            sw.WriteLine($"{originalXValues[i]};{originalYValues[i]};{defScaledXValues[i]};{defScaledYValues[i]};{autoScaledXValues[i]}; {autoScaledYValues[i]}");
        //        }
        //        sw.Close();

        //        //Process.Start(outputFileName);
        //    }
        //    else if (selectedTransformation == "Scale")
        //    {
        //        var originalXValues = signature.GetFeature(Features.X);
        //        new Scale()
        //        {
        //            InputFeature = Features.X,
        //            NewMinValue = 100,
        //            NewMaxValue = 500,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>("ScalingResult")
        //        }.Transform(signature);
        //        var defScaledXValues = new List<double>(signature.GetFeature<List<double>>("ScalingResult"));

        //        new Scale
        //        {
        //            InputFeature = Features.X,
        //            OutputFeature = FeatureDescriptor.Get<List<double>>("ScalingResult")
        //        }.Transform(signature);
        //        var autoScaledXValues = new List<double>(signature.GetFeature<List<double>>("ScalingResult"));

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalX;  DefScaledX ;  AutoScaledX");
        //        for (int i = 0; i < signature.GetFeature(Features.X).Count; i++)
        //        {
        //            sw.WriteLine($"{originalXValues[i]};{defScaledXValues[i]};{autoScaledXValues[i]}");
        //        }
        //        sw.Close();

        //        //Process.Start(outputFileName);
        //    }
        //    else if (selectedTransformation == "NormalizeRotation")
        //    {
        //        ////Deform
        //        //var xValues = signature.GetFeature(Features.X);
        //        //var yValues = signature.GetFeature(Features.Y);

        //        //double cosa = 1 / Math.Sqrt(2);
        //        //double sina = 1 / Math.Sqrt(2);

        //        //for (int i = 0; i < xValues.Count; i++)
        //        //{
        //        //    double x = xValues[i];
        //        //    double y = yValues[i];
        //        //    xValues[i] = x * cosa - y * sina;
        //        //    yValues[i] = x * sina + y * cosa;
        //        //}


        //        //var originalTValues = new List<double>(signature.GetFeature(Features.T));
        //        var originalXValues = new List<double>(signature.GetFeature(Features.X));
        //        var originalYValues = new List<double>(signature.GetFeature(Features.Y));


        //        var tfsOriginal = new SequentialTransformPipeline
        //        {

        //            new UniformScale() {
        //                 BaseDimension = Features.X,
        //                 ProportionalDimension = Features.Y,
        //                 BaseDimensionOutput = Features.X,
        //                 ProportionalDimensionOutput =Features.Y
        //             },
        //            new RealisticImageGenerator(1280, 720)
        //        };
        //        tfsOriginal.Logger = new SimpleConsoleLogger();
        //        tfsOriginal.Transform(signature);
        //        //var imggen = new RealisticImageGenerator(1280, 720)
        //        //{

        //        //    Logger = new SimpleConsoleLogger()
        //        //};
        //        //imggen.Transform(signature);
        //        ImageSaver.Save(signature, @"GeneratedOnlineImageBase.png");


        //        signature.SetFeature(Features.X, new List<double>(originalXValues));
        //        signature.SetFeature(Features.Y, new List<double>(originalYValues));

        //        new NormalizeRotation()
        //        {
        //        }.Transform(signature);
        //        var rotatedXValues = new List<double>(signature.GetFeature(Features.X));
        //        var rotatedYValues = new List<double>(signature.GetFeature(Features.Y));

        //        var tfsRotated = new SequentialTransformPipeline
        //        {

        //            new UniformScale() {
        //                 BaseDimension = Features.X,
        //                 ProportionalDimension = Features.Y,
        //                 BaseDimensionOutput = Features.X,
        //                 ProportionalDimensionOutput =Features.Y
        //             },
        //            new RealisticImageGenerator(1280, 720)
        //        };
        //        tfsRotated.Logger = new SimpleConsoleLogger();
        //        tfsRotated.Transform(signature);

        //        //imggen.Transform(signature);
        //        ImageSaver.Save(signature, @"GeneratedOnlineImageRotated.png");

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalX; OriginalY; RotatedX ; RotatedY");
        //        for (int i = 0; i < signature.GetFeature(Features.X).Count; i++)
        //        {
        //            sw.WriteLine($"{originalXValues[i]};{originalYValues[i]};{rotatedXValues[i]};{rotatedYValues[i]}");
        //        }
        //        sw.Close();

        //        //Process.Start(outputFileName);
        //    }
        //    else if (selectedTransformation == "ResampleTimeBased")
        //    {

        //        var imggen = new RealisticImageGenerator(1280, 720)
        //        {

        //            Logger = new SimpleConsoleLogger()
        //        };
        //        imggen.Transform(signature);
        //        ImageSaver.Save(signature, @"GeneratedOnlineImageBaseSampled.png");

        //        List<FeatureDescriptor<List<double>>> features = new List<FeatureDescriptor<List<double>>>(
        //            new FeatureDescriptor<List<double>>[]
        //            {
        //                Features.X, Features.Y, Features.Pressure, Features.Azimuth, Features.Altitude
        //            });

        //        var originalTimestamps = new List<double>(signature.GetFeature(Features.T));
        //        var originalValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            originalValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        var resampler = new ResampleTimeBased()
        //        {
        //            InputFeatures = features,
        //            OutputFeatures = features,
        //            TimeSlot = 20,
        //            InterpolationType = typeof(CubicInterpolation)
        //            //Interpolation = new LinearInterpolation()
        //        };
        //        resampler.Transform(signature);

        //        //kisebb timeslotra mint az eredeti nem meg mert a penupot is nézi a kirajzoló
        //        imggen.Transform(signature);
        //        ImageSaver.Save(signature, @"GeneratedOnlineImageResampled.png");

        //        var resampledValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            resampledValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalT; OriginalX; OriginalY ; OriginalP ; OriginalAz; OriginalAl ;" +
        //            "ResampledT; ResampledX; ResampledY ; ResampledP ; ResampledAz; ResampledAl ");
        //        var min = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? originalTimestamps.Count : resampler.ResampledTimestamps.Count;
        //        var max = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? resampler.ResampledTimestamps.Count : originalTimestamps.Count;
        //        var isOriginalMin = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? true : false;
        //        for (int i = 0; i < max; i++)
        //        {
        //            if (i < min)
        //            {
        //                sw.WriteLine(
        //                    $"{originalTimestamps[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};{originalValues[4][i]};" +
        //                    $"{resampler.ResampledTimestamps[i]}; {resampledValues[0][i]};{resampledValues[1][i]};{resampledValues[2][i]};{resampledValues[3][i]};{resampledValues[4][i]}");
        //            }
        //            else if (isOriginalMin)
        //            {
        //                sw.WriteLine(
        //                    $" \"\";\"\";\"\";\"\";\"\";\"\"; " +
        //                    $"{resampler.ResampledTimestamps[i]}; {resampledValues[0][i]};{resampledValues[1][i]};{resampledValues[2][i]};{resampledValues[3][i]};{resampledValues[4][i]}");
        //            }
        //            else
        //            {
        //                sw.WriteLine(
        //                   $"{originalTimestamps[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};{originalValues[4][i]};" +
        //                    $"\"\";\"\";\"\";\"\";\"\";\"\" ");

        //            }
        //        }
        //        sw.Close();

        //    }
        //    else if (selectedTransformation == "ResampleSamplesCountBased")
        //    {
        //        List<FeatureDescriptor<List<double>>> features = new List<FeatureDescriptor<List<double>>>(
        //            new FeatureDescriptor<List<double>>[]
        //            {
        //                Features.X, Features.Y, Features.Pressure, Features.Azimuth, Features.Altitude
        //            });

        //        var originalTimestamps = new List<double>(signature.GetFeature(Features.T));
        //        var originalValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            originalValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        var resampler = new ResampleSamplesCountBased()
        //        {
        //            InputFeatures = features,
        //            OutputFeatures = features,
        //            NumOfSamples = 500,
        //            //Interpolation = new LinearInterpolation()
        //            InterpolationType = typeof(CubicInterpolation)
        //        };
        //        resampler.Transform(signature);

        //        var resampledValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            resampledValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalT; OriginalX; OriginalY ; OriginalP ; OriginalAz; OriginalAl ;" +
        //            "ResampledT; ResampledX; ResampledY ; ResampledP ; ResampledAz; ResampledAl ");
        //        var min = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? originalTimestamps.Count : resampler.ResampledTimestamps.Count;
        //        var max = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? resampler.ResampledTimestamps.Count : originalTimestamps.Count;
        //        var isOriginalMin = originalTimestamps.Count <= resampler.ResampledTimestamps.Count ? true : false;
        //        for (int i = 0; i < max; i++)
        //        {
        //            if (i < min)
        //            {
        //                sw.WriteLine(
        //                    $"{originalTimestamps[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};{originalValues[4][i]};" +
        //                    $"{resampler.ResampledTimestamps[i]}; {resampledValues[0][i]};{resampledValues[1][i]};{resampledValues[2][i]};{resampledValues[3][i]};{resampledValues[4][i]}");
        //            }
        //            else if (isOriginalMin)
        //            {
        //                sw.WriteLine(
        //                    $" \"\";\"\";\"\";\"\";\"\";\"\"; " +
        //                    $"{resampler.ResampledTimestamps[i]}; {resampledValues[0][i]};{resampledValues[1][i]};{resampledValues[2][i]};{resampledValues[3][i]};{resampledValues[4][i]}");
        //            }
        //            else
        //            {
        //                sw.WriteLine(
        //                   $"{originalTimestamps[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};{originalValues[4][i]};" +
        //                    $"\"\";\"\";\"\";\"\";\"\";\"\" ");

        //            }
        //        }
        //        sw.Close();

        //    }
        //    else if (selectedTransformation == "FillPenUpDurations")
        //    {
        //        List<FeatureDescriptor<List<double>>> features = new List<FeatureDescriptor<List<double>>>(
        //            new FeatureDescriptor<List<double>>[]
        //            {
        //                Features.X, Features.Y, Features.Pressure, Features.Azimuth, Features.Altitude
        //            });

        //        var originalTimestamps = new List<double>(signature.GetFeature(Features.T));
        //        var originalValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            originalValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        var filler = new FillPenUpDurations()
        //        {
        //            InputFeatures = features,
        //            OutputFeatures = features,
        //            //Interpolation = new LinearInterpolation(),
        //            InterpolationType = typeof(CubicInterpolation)
        //        };
        //        filler.Transform(signature);

        //        var filledTimestamps = new List<double>(signature.GetFeature(filler.TimeOutputFeature));
        //        var filledValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            filledValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalT; OriginalX; OriginalY ; OriginalP ; OriginalAz; OriginalAl ;" +
        //            "FilledT; FilledX; FilledY ; FilledP ; FilledAz; FilledAl ");
        //        var originalCount = signature.GetFeature(filler.TimeInputFeature).Count;
        //        for (int i = 0; i < filledTimestamps.Count; i++)
        //        {
        //            if (i < originalCount)
        //            {
        //                sw.WriteLine(
        //                    $"{originalTimestamps[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};{originalValues[4][i]};" +
        //                    $"{filledTimestamps[i]}; {filledValues[0][i]};{filledValues[1][i]};{filledValues[2][i]};{filledValues[3][i]};{filledValues[4][i]}");
        //            }
        //            else
        //            {
        //                sw.WriteLine(
        //                    $"\"\";\"\";\"\";\"\";\"\";\"\"; " +
        //                    $"{filledTimestamps[i]}; {filledValues[0][i]};{filledValues[1][i]};{filledValues[2][i]};{filledValues[3][i]};{filledValues[4][i]}");
        //            }

        //        }
        //        sw.Close();
        //    }
        //    else if (selectedTransformation == "FilterPoints")
        //    {
        //        List<FeatureDescriptor<List<double>>> features = new List<FeatureDescriptor<List<double>>>(
        //            new FeatureDescriptor<List<double>>[]
        //            {
        //                Features.X, Features.Y, Features.Azimuth, Features.Altitude
        //            });

        //        var originalPressureValues = new List<double>(signature.GetFeature(Features.Pressure));
        //        var originalValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            originalValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        var filter = new FilterPoints()
        //        {
        //            InputFeatures = features,
        //            OutputFeatures = features,
        //            KeyFeatureInput = Features.Pressure,
        //            KeyFeatureOutput = Features.Pressure
        //        };
        //        filter.Transform(signature);

        //        var filteredPressureValues = new List<double>(signature.GetFeature(filter.KeyFeatureInput));
        //        var filteredValues = new List<double>[features.Count];
        //        for (int i = 0; i < features.Count; i++)
        //        {
        //            filteredValues[i] = new List<double>(signature.GetFeature(features[i]));
        //        }

        //        string outputFileName = selectedTransformation + "TransformationOutputTest.csv";
        //        StreamWriter sw = new StreamWriter(outputFileName);
        //        sw.WriteLine("OriginalP; OriginalX; OriginalY ; OriginalAz; OriginalAl ;" +
        //            "FilteredP ; FilteredX; FilteredY ;  FilteredAz; FilteredAl ");
        //        var filteredCount = filteredPressureValues.Count;
        //        for (int i = 0; i < originalPressureValues.Count; i++)
        //        {
        //            if (i < filteredCount)
        //            {
        //                sw.WriteLine(
        //                    $"{originalPressureValues[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};" +
        //                    $"{filteredPressureValues[i]}; {filteredValues[0][i]};{filteredValues[1][i]};{filteredValues[2][i]};{filteredValues[3][i]}");
        //            }
        //            else
        //            {
        //                sw.WriteLine(
        //                    $"{originalPressureValues[i]}; {originalValues[0][i]};{originalValues[1][i]};{originalValues[2][i]};{originalValues[3][i]};" +
        //                    $"\"\";\"\";\"\";\"\";\"\";\"\" ");
        //            }

        //        }
        //        sw.Close();
        //    }
        //}

        static void JsonSerializeSignature()
        {
            Signature sig = new Signature();
            sig.ID = "Demo";
            sig.Origin = Origin.Genuine;
            sig.Signer = new Signer()
            {
                ID = "S05"
            };
            //sig.Signer.Signatures.Add(sig);
            FeatureDescriptor<int> heightDescriptor = FeatureDescriptor.Get<int>("Height");
            //var method = typeof(FeatureDescriptor).GetMethod("Get<>", BindingFlags.Public | BindingFlags.Static);
            //var genMethod = method.MakeGenericMethod(type);
            //genMethod.Invoke()


            sig.SetFeature(heightDescriptor, 4);
            var loops = new List<Loop>() { new Loop(1, 1), new Loop(3, 3) };
            System.Drawing.RectangleF bound = new System.Drawing.RectangleF(10, 10, 5, 3);
            loops[0].Bounds = bound;
            sig.SetFeature(MyFeatures.Loop, loops);

            Svc2004Loader loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true);
            var signer = loader.EnumerateSigners(p => p.ID == "01").First();//Load the first signer only
            var signature = signer.Signatures[0];
            signature.Signer.Signatures = null;

            //Serialize to a string
            SerializationHelper.JsonSerializeToFile(sig,@"SignaturSerialized.txt");

            //Deserialize from a string
            Signature desirializedSig = SerializationHelper.DeserializeFromFile<Signature>(@"SignaturSerialized.txt");

            /*foreach (var descriptor in desirializedSig.GetFeatureDescriptors())
            {
                if (!descriptor.IsCollection)
                {
                    Console.WriteLine($"{descriptor.Name}: {desirializedSig[descriptor]}");
                }
                else
                {
                    Console.WriteLine($"{descriptor}:");
                    var items = (IList)desirializedSig[descriptor];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Console.WriteLine($" {i}.) {items[i]}");
                    }
                }
            }*/
        }

        static void JsonSerializeOnlineVerifier()
        {
            var onlineverifier = new Verifier(new SimpleConsoleLogger())
            {
                Pipeline = new SequentialTransformPipeline
                {
                    new ParallelTransformPipeline
                    {
                        new Normalize() { Input = Features.Pressure },
                        new Map(0, 1) { Input = Features.X },
                        new Map(0, 1) { Input = Features.Y },
                        //new TimeReset(),
                    },
                    //new CentroidTranslate(),//is a sequential pipeline of other building blocks
                    new TangentExtraction(),
                    /*new AlignmentNormalization(Alignment.Origin),
                    new Paper13FeatureExtractor(),*/

                },
                Classifier = new OptimalDtwClassifier()
                {
                    Sampler = new FirstNSampler(10),
                    Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure }
                }
            };

            string path = @"OnlineVerifier.json";
            string path3 = @"OnlineVerifier3.json";

            //File serialization example
            SerializationHelper.JsonSerializeToFile(onlineverifier, path);

            Verifier deserializedOV = SerializationHelper.DeserializeFromFile<Verifier>(path);

        }
        static void JsonSerializeOnlineVerifierBenchmark()
        {
            var benchmark = new VerifierBenchmark()
            {
                Loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip".GetPath(), true),
                Verifier = new Verifier()
                {
                    Pipeline = new SequentialTransformPipeline
                    {
                        new NormalizeRotation(){InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY=Features.Y},

                        new Scale() {InputFeature = Features.X, OutputFeature = Features.X},
                             new Scale() {InputFeature = Features.Y, OutputFeature = Features.Y},
                              new FillPenUpDurations()
                              {
                                  InputFeatures = new List<FeatureDescriptor<List<double>>>(){ Features.X, Features.Y, Features.Pressure },
                                  OutputFeatures = new List<FeatureDescriptor<List<double>>>() { Features.X, Features.Y, Features.Pressure },
                                  InterpolationType = typeof(CubicInterpolation),
                                  TimeInputFeature =Features.T,
                                  TimeOutputFeature = Features.T
                              }
                    }
                    ,
                    Classifier = new OptimalDtwClassifier()
                    {
                        Sampler = new FirstNSampler(10),
                        Features = new List<FeatureDescriptor>() { Features.X, Features.Y, Features.Pressure }
                    }
                },
                Sampler = new FirstNSampler(10),
                Logger = new SimpleConsoleLogger(),
            };

            benchmark.ProgressChanged += ProgressPrimary;
            //benchmark.Verifier.ProgressChanged += ProgressSecondary;

            //var result = benchmark.Execute(true);

            //Console.WriteLine($"AER: {result.FinalResult.Aer}");
            SerializationHelper.JsonSerializeToFile(benchmark, @"VerifierBenchmarkSerialized.txt");
            //SerializationHelper.JsonSerializeToFile<BenchmarkResults>(result, @"BenchmarkResultSerialized.txt");
            VerifierBenchmark deserializedBM = SerializationHelper.DeserializeFromFile<VerifierBenchmark>(@"VerifierBenchmarkSerialized.txt");
        }


        static int primaryP = 0;
        static int secondaryP = 0;
        public static void ProgressPrimary(object sender, int progress)
        {
            primaryP = progress;
            SetTitleProgress();
        }
        public static void ProgressSecondary(object sender, int progress)
        {
            secondaryP = progress;
            SetTitleProgress();
        }
        public static void SetTitleProgress()
        {
            Console.Title = $"Progress: {primaryP}% | Sub-progress: {secondaryP}%";
        }
        private static void WriteToConsole(bool[,] arr)
        {
            for (int y = 0; y < arr.GetLength(1); y += 2)
            {
                for (int x = 0; x < arr.GetLength(0); x++)
                {
                    Console.Write(arr[x, arr.GetLength(1) - y - 1] ? "." : " ");
                }
                Console.WriteLine();
            }
        }
    }
}
