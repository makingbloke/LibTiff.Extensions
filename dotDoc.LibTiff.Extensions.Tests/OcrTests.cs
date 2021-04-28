// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using BitMiracle.LibTiff.Classic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace dotDoc.LibTiff.Extensions.Tests
{
    /// <summary>
    /// Tests for the OcrExtensions extension.
    /// </summary>
    [TestClass]
    public class OcrTests
    {
        /// <summary>
        /// Test OcrPagesAsync can OCR a <see cref="Tiff"/> object.
        /// </summary>
        [TestMethod]
        public async Task OcrPagesTestAsync()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            IProgress<OcrProgress> progress = new Progress<OcrProgress>(op => Console.WriteLine($"Page: {op.Page} Stage: {op.OcrStage}"));

            IDictionary<int, IList<OcrWord>> ocrResult = await tiff.OcrPagesAsync(progress: progress).ConfigureAwait(false);

            Assert.AreEqual(1, ocrResult.Count);
            Assert.IsTrue(ocrResult.ContainsKey(0));
            Assert.AreEqual(187, ocrResult[0].Count);
        }

        /// <summary>
        /// Test CreateSearchablePdfAsync can create a PDF from a <see cref="Tiff"/> object.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CreateSearchablePdfTestAsync()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            IProgress<OcrProgress> progress = new Progress<OcrProgress>(op => Console.WriteLine($"Page: {op.Page} Stage: {op.OcrStage}"));

            string fileName = "CCITT_1.PDF";
            await tiff.CreateSearchablePdfAsync(fileName, progress: progress).ConfigureAwait(false);

            Assert.IsTrue(File.Exists(fileName));
        }
    }
}
