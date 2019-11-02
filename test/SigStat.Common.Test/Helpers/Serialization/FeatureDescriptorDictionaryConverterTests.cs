﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using SigStat.Common.Helpers.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace SigStat.Common.Test.Helpers.Serialization
{
    [TestClass]
    public class FeatureDescriptorDictionaryConverterTests
    {
        public static JsonSerializerSettings GetTestSettings()
        {
            return new JsonSerializerSettings

            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new VerifierResolver(),
                Context = new StreamingContext(StreamingContextStates.All, new FeatureStreamingContextState(false)),
                Converters = new List<JsonConverter> { new FeatureDescriptorDictionaryConverter() }
            };
        }

        [TestMethod]
        public void TestCanConvert()
        {
            var converter = new FeatureDescriptorDictionaryConverter();
            var featureDescriptor = new Dictionary<string, FeatureDescriptor>();
            var convertible = converter.CanConvert(featureDescriptor.GetType());
            Assert.IsTrue(convertible);
            var otherObj = new object();
            var notConvertible = converter.CanConvert(otherObj.GetType());
            Assert.IsFalse(notConvertible);
        }

        [TestMethod]
        public void TestWrite()
        {
            var features = new Dictionary<string, FeatureDescriptor>();
            var jsonSerializerSettings = GetTestSettings();
            features.Add("Pressure", Features.Pressure);
            features.Add("Altitude", Features.Altitude);
            var json = JsonConvert.SerializeObject(features, Formatting.Indented, jsonSerializerSettings);

            var expectedJson = @"[
            ""Pressure | System.Collections.Generic.List`1[[System.Double, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"",
            ""Altitude | System.Collections.Generic.List`1[[System.Double, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e""
            ]";
            JsonAssert.AreEqual(expectedJson, json);
        }


        [TestMethod]
        public void TestRead()
        {
            var features = new Dictionary<string, FeatureDescriptor>();
            var jsonSerializerSettings = GetTestSettings();
            features.Add("Pressure", Features.Pressure);
            features.Add("Altitude", Features.Altitude);
            var json = JsonConvert.SerializeObject(features, Formatting.Indented, jsonSerializerSettings);
            var featuresDeserialized = JsonConvert.DeserializeObject<Dictionary<string,FeatureDescriptor>>(json, jsonSerializerSettings);
            JsonAssert.AreEqual(features, featuresDeserialized);
        }

        [TestMethod]
        public void TestReadWrongJson()
        {
            var json = $"\r\n  \"{Features.Pressure.Key} | {Features.Pressure.FeatureType.AssemblyQualifiedName}\",\r\n  \"{Features.Altitude.Key} | {Features.Altitude.FeatureType.AssemblyQualifiedName}\"\r\n]";
            Assert.ThrowsException<JsonReaderException>(() =>
                JsonConvert.DeserializeObject<Dictionary<string,FeatureDescriptor>>(json, GetTestSettings()));
        }
    }
}
