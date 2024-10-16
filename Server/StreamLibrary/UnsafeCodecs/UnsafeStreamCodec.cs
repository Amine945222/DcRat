﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using StreamLibrary.src;

namespace StreamLibrary.UnsafeCodecs
{
    public class UnsafeStreamCodec : IUnsafeCodec
    {
        private Bitmap decodedBitmap;
        private byte[] EncodeBuffer;
        private PixelFormat EncodedFormat;
        private int EncodedHeight;
        private int EncodedWidth;

        private readonly bool UseJPEG;

        /// <summary>
        ///     Initialize a new object of UnsafeStreamCodec
        /// </summary>
        /// <param name="ImageQuality">The quality to use between 0-100</param>
        public UnsafeStreamCodec(int ImageQuality = 100, bool UseJPEG = true)
            : base(ImageQuality)
        {
            CheckBlock = new Size(50, 1);
            this.UseJPEG = UseJPEG;
        }

        public override ulong CachedSize { get; internal set; }

        public override int BufferCount => 1;

        public override CodecOption CodecOptions => CodecOption.RequireSameSize;

        public Size CheckBlock { get; }
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle ScanArea, Size ImageSize, PixelFormat Format,
            Stream outStream)
        {
            lock (ImageProcessLock)
            {
                var pScan0 = (byte*)Scan0.ToInt32();
                if (!outStream.CanWrite)
                    throw new Exception("Must have access to Write in the Stream");

                var Stride = 0;
                var RawLength = 0;
                var PixelSize = 0;

                switch (Format)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        PixelSize = 3;
                        break;
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        PixelSize = 4;
                        break;
                    default:
                        throw new NotSupportedException(Format.ToString());
                }

                Stride = ImageSize.Width * PixelSize;
                RawLength = Stride * ImageSize.Height;

                if (EncodeBuffer == null)
                {
                    EncodedFormat = Format;
                    EncodedWidth = ImageSize.Width;
                    EncodedHeight = ImageSize.Height;
                    EncodeBuffer = new byte[RawLength];
                    fixed (byte* ptr = EncodeBuffer)
                    {
                        byte[] temp = null;
                        using (var TmpBmp = new Bitmap(ImageSize.Width, ImageSize.Height, Stride, Format, Scan0))
                        {
                            temp = jpgCompression.Compress(TmpBmp);
                        }

                        outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                        outStream.Write(temp, 0, temp.Length);
                        NativeMethods.memcpy(new IntPtr(ptr), Scan0, (uint)RawLength);
                    }

                    return;
                }

                var oldPos = outStream.Position;
                outStream.Write(new byte[4], 0, 4);
                var TotalDataLength = 0;

                if (EncodedFormat != Format)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");

                if (EncodedWidth != ImageSize.Width || EncodedHeight != ImageSize.Height)
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");

                var Blocks = new List<Rectangle>();
                var index = 0;

                var s = new Size(ScanArea.Width, CheckBlock.Height);
                var lastSize = new Size(ScanArea.Width % CheckBlock.Width, ScanArea.Height % CheckBlock.Height);

                var lasty = ScanArea.Height - lastSize.Height;
                var lastx = ScanArea.Width - lastSize.Width;

                var cBlock = new Rectangle();
                var finalUpdates = new List<Rectangle>();

                s = new Size(ScanArea.Width, s.Height);
                fixed (byte* encBuffer = EncodeBuffer)
                {
                    for (var y = ScanArea.Y; y != ScanArea.Height;)
                    {
                        if (y == lasty)
                            s = new Size(ScanArea.Width, lastSize.Height);
                        cBlock = new Rectangle(ScanArea.X, y, ScanArea.Width, s.Height);

                        if (onCodeDebugScan != null)
                            onCodeDebugScan(cBlock);

                        var offset = y * Stride + ScanArea.X * PixelSize;
                        if (NativeMethods.memcmp(encBuffer + offset, pScan0 + offset, (uint)Stride) != 0)
                        {
                            index = Blocks.Count - 1;
                            if (Blocks.Count != 0 && Blocks[index].Y + Blocks[index].Height == cBlock.Y)
                            {
                                cBlock = new Rectangle(Blocks[index].X, Blocks[index].Y, Blocks[index].Width,
                                    Blocks[index].Height + cBlock.Height);
                                Blocks[index] = cBlock;
                            }
                            else
                            {
                                Blocks.Add(cBlock);
                            }
                        }

                        y += s.Height;
                    }

                    for (int i = 0, x = ScanArea.X; i < Blocks.Count; i++)
                    {
                        s = new Size(CheckBlock.Width, Blocks[i].Height);
                        x = ScanArea.X;
                        while (x != ScanArea.Width)
                        {
                            if (x == lastx)
                                s = new Size(lastSize.Width, Blocks[i].Height);

                            cBlock = new Rectangle(x, Blocks[i].Y, s.Width, Blocks[i].Height);
                            var FoundChanges = false;
                            var blockStride = PixelSize * cBlock.Width;

                            for (var j = 0; j < cBlock.Height; j++)
                            {
                                var blockOffset = Stride * (cBlock.Y + j) + PixelSize * cBlock.X;
                                if (NativeMethods.memcmp(encBuffer + blockOffset, pScan0 + blockOffset,
                                        (uint)blockStride) != 0)
                                    FoundChanges = true;
                                NativeMethods.memcpy(encBuffer + blockOffset, pScan0 + blockOffset,
                                    (uint)blockStride); //copy-changes
                            }

                            if (onCodeDebugScan != null)
                                onCodeDebugScan(cBlock);

                            if (FoundChanges)
                            {
                                index = finalUpdates.Count - 1;
                                if (finalUpdates.Count > 0 &&
                                    finalUpdates[index].X + finalUpdates[index].Width == cBlock.X)
                                {
                                    var rect = finalUpdates[index];
                                    var newWidth = cBlock.Width + rect.Width;
                                    cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                                    finalUpdates[index] = cBlock;
                                }
                                else
                                {
                                    finalUpdates.Add(cBlock);
                                }
                            }

                            x += s.Width;
                        }
                    }
                }

                /*int maxHeight = 0;
                int maxWidth = 0;

                for (int i = 0; i < finalUpdates.Count; i++)
                {
                    if (finalUpdates[i].Height > maxHeight)
                        maxHeight = finalUpdates[i].Height;
                    maxWidth += finalUpdates[i].Width;
                }

                Bitmap bmp = new Bitmap(maxWidth+1, maxHeight+1);
                int XOffset = 0;*/

                for (var i = 0; i < finalUpdates.Count; i++)
                {
                    var rect = finalUpdates[i];
                    var blockStride = PixelSize * rect.Width;

                    var TmpBmp = new Bitmap(rect.Width, rect.Height, Format);
                    var TmpData = TmpBmp.LockBits(new Rectangle(0, 0, TmpBmp.Width, TmpBmp.Height),
                        ImageLockMode.ReadWrite, TmpBmp.PixelFormat);
                    for (int j = 0, offset = 0; j < rect.Height; j++)
                    {
                        var blockOffset = Stride * (rect.Y + j) + PixelSize * rect.X;
                        NativeMethods.memcpy((byte*)TmpData.Scan0.ToPointer() + offset, pScan0 + blockOffset,
                            (uint)blockStride); //copy-changes
                        offset += blockStride;
                    }

                    TmpBmp.UnlockBits(TmpData);

                    /*using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(TmpBmp, new Point(XOffset, 0));
                    }
                    XOffset += TmpBmp.Width;*/

                    outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                    outStream.Write(new byte[4], 0, 4);

                    var length = outStream.Length;
                    var OldPos = outStream.Position;

                    if (UseJPEG)
                        jpgCompression.Compress(TmpBmp, ref outStream);
                    else
                        lzwCompression.Compress(TmpBmp, outStream);

                    length = outStream.Position - length;

                    outStream.Position = OldPos - 4;
                    outStream.Write(BitConverter.GetBytes((int)length), 0, 4);
                    outStream.Position += length;
                    TmpBmp.Dispose();
                    TotalDataLength += (int)length + 4 * 5;
                }

                /*if (finalUpdates.Count > 0)
                {
                    byte[] lele = base.jpgCompression.Compress(bmp);
                    byte[] compressed = new SafeQuickLZ().compress(lele, 0, lele.Length, 1);
                    bool Won = lele.Length < outStream.Length;
                    bool CompressWon = compressed.Length < outStream.Length;
                    Console.WriteLine(Won + ", " + CompressWon);
                }
                bmp.Dispose();*/

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);
                Blocks.Clear();
                finalUpdates.Clear();
            }
        }

        public override unsafe Bitmap DecodeData(IntPtr CodecBuffer, uint Length)
        {
            if (Length < 4)
                return decodedBitmap;

            var DataSize = *(int*)CodecBuffer;
            if (decodedBitmap == null)
            {
                var temp = new byte[DataSize];
                fixed (byte* tempPtr = temp)
                {
                    NativeMethods.memcpy(new IntPtr(tempPtr), new IntPtr(CodecBuffer.ToInt32() + 4), (uint)DataSize);
                }

                decodedBitmap = (Bitmap)Image.FromStream(new MemoryStream(temp));
                return decodedBitmap;
            }

            return decodedBitmap;
        }

        public override Bitmap DecodeData(Stream inStream)
        {
            try
            {
                var temp = new byte[4];
                inStream.Read(temp, 0, 4);
                var DataSize = BitConverter.ToInt32(temp, 0);

                if (decodedBitmap == null)
                {
                    temp = new byte[DataSize];
                    inStream.Read(temp, 0, temp.Length);
                    decodedBitmap = (Bitmap)Image.FromStream(new MemoryStream(temp));
                    return decodedBitmap;
                }

                using (var g = Graphics.FromImage(decodedBitmap))
                {
                    while (DataSize > 0)
                    {
                        var tempData = new byte[4 * 5];
                        inStream.Read(tempData, 0, tempData.Length);

                        var rect = new Rectangle(BitConverter.ToInt32(tempData, 0), BitConverter.ToInt32(tempData, 4),
                            BitConverter.ToInt32(tempData, 8), BitConverter.ToInt32(tempData, 12));
                        var UpdateLen = BitConverter.ToInt32(tempData, 16);
                        tempData = null;

                        var buffer = new byte[UpdateLen];
                        inStream.Read(buffer, 0, buffer.Length);

                        if (onDecodeDebugScan != null)
                            onDecodeDebugScan(rect);

                        using (var m = new MemoryStream(buffer))
                        using (var tmp = (Bitmap)Image.FromStream(m))
                        {
                            g.DrawImage(tmp, rect.Location);
                        }

                        buffer = null;
                        DataSize -= UpdateLen + 4 * 5;
                    }
                }

                return decodedBitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}