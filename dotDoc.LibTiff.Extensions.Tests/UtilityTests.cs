// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

using BitMiracle.LibTiff.Classic;
using dotDoc.LibTiff.Extensions;
using dotDoc.LibTiff.Extensions.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace dotDoc.EntityFrameworkCore.Extensions.Tests
{
    /// <summary>
    /// Tests for the UtilityExtensions extension.
    /// </summary>
    [TestClass]
    public class UtilityTests
    {
        /// <summary>
        /// Test Open (fileName) can open a Tiff.
        /// </summary>
        [TestMethod]
        public void OpenFileTest()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            Assert.IsNotNull(tiff);                
        }

        /// <summary>
        /// Test Open (fileName) fails on a non existant Tiff.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UtilityExtensionsException))]
        public void OpenFileFailTest()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/NonExistentTiff.TIF", "r");
        }

        /// <summary>
        /// Test Open (stream) can handle a stream from a Tiff file.
        /// </summary>
        [TestMethod]
        public void OpenStreamTest()
        {
            FileStream stream = File.OpenRead("SampleImages/CCITT_1.TIF");
            using Tiff tiff = UtilityExtensions.Open(stream, "r");
            Assert.IsNotNull(tiff);
        }

        /// <summary>
        /// Test Open (stream) fails on a closed stream.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UtilityExtensionsException))]
        public void OpenStreamFailTest()
        {
            FileStream stream = File.OpenRead("SampleImages/CCITT_1.TIF");
            stream.Close();
            using Tiff tiff = UtilityExtensions.Open(stream, "r");
        }

        /// <summary>
        /// Test SetPage can set a page in a <see cref="Tiff" image/>.
        /// </summary>
        [TestMethod]
        public void SetPageTest()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            tiff.SetPage(0);
            Assert.AreEqual(0, tiff.CurrentDirectory());
        }

        /// <summary>
        /// Test SetPage fail on an invalid page number.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UtilityExtensionsException))]
        public void SetPageFailTest()
        {
            using Tiff tiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            tiff.SetPage(10);
        }


        /// <summary>
        /// Test CopyPage can copy a page from one <see cref="Tiff" image/> to another.
        /// </summary>
        [TestMethod]
        public void CopyPageTest()
        {
            using Tiff srcTiff = UtilityExtensions.Open("SampleImages/CCITT_1.TIF", "r");
            using Tiff dstTiff = UtilityExtensions.Open("CCITT_1_Copy.TIF", "w");

            srcTiff.SetPage(0);
            srcTiff.CopyPage(dstTiff);

            Assert.AreEqual(1, dstTiff.NumberOfDirectories());
        }
    }
}
