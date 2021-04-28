// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using BitMiracle.LibTiff.Classic;
using BitMiracle.TiffCP;
using dotDoc.LibTiff.Extensions.Exceptions;
using System.IO;

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// LibTiff Utility Extension Methods.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Open a <see cref="Tiff"/> from a file.
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        /// <param name="mode">The mode in which to open the file <seealso cref="Tiff.Open"/>.</param>
        /// <returns></returns>
        public static Tiff Open(string fileName, string mode) =>
            Tiff.Open(fileName, mode) ?? throw new UtilityExtensionsException($"Unable to open Tiff file: {fileName}");

        /// <summary>
        /// Open a <see cref="Tiff"/> from a stream. 
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to use.</param>
        /// <param name="mode">The mode in which to open the file <seealso cref="Tiff.Open"/>.</param>
        /// <param name="name">An optional arbitary name which can be used as an id.</param>
        /// <returns></returns>
        public static Tiff Open(Stream stream, string mode, string name = null) =>
            Tiff.ClientOpen(name ?? "", mode, stream, new TiffStream()) ?? throw new UtilityExtensionsException($"Unable to open Tiff stream name: {name}");

        /// <summary>
        /// Set the <see cref="Tiff"/> page number.
        /// </summary>
        /// <param name="tiff">The <see cref="Tiff"/> where to set the page.</param>
        /// <param name="page">The page number.</param>
        public static void SetPage(this Tiff tiff, int page)
        {
            if (!tiff.SetDirectory((short)page)) throw new UtilityExtensionsException("Unable to set Tiff Directory");
        }

        /// <summary>
        /// Copy a page from one <see cref="Tiff"/> to another.
        /// </summary>
        /// <param name="srcTiff">The source <see cref="Tiff"/>.</param>
        /// <param name="dstTiff">The destination <see cref="Tiff"/>.</param>
        public static void CopyPage(this Tiff srcTiff, Tiff dstTiff)
        {
            // libtiff does not support writing old style jpegs.
            if ((Compression)srcTiff.GetField(TiffTag.COMPRESSION)[0].ToInt() == Compression.OJPEG) throw new UtilityExtensionsException("Cannot copy a page with old style JPEG compression.");

            // use the copier class from tiffp to copy the page
            Copier copier = new()
            {
                m_config = PlanarConfig.UNKNOWN,
                m_fillorder = 0,
                m_rowsperstrip = 0,
                m_tilewidth = -1,
                m_tilelength = -1,
                m_quality = 90
            };

            copier.m_compression = copier.m_defcompression;
            copier.m_predictor = copier.m_defpredictor;
            copier.m_g3opts = copier.m_defg3opts;

            if (!copier.Copy(srcTiff, dstTiff)) throw new UtilityExtensionsException("Failure copying page.");

            dstTiff.WriteDirectory();
        }
    }
}
