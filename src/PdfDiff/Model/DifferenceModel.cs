using System;
using System.IO;
using ImageMagick;

namespace PdfDiff.Model
{
    public class DifferenceModel
    {
        private string _originalImagePath, _modifiedImagePath;
        private MagickReadSettings _magickSettings, _magickSettingsHigh;

        public const int Density = 250;
        public const int ReducedDensity = 10;

        public DifferenceModel(string originalImagePath, string modifiedImagePath)
        {
            _originalImagePath = originalImagePath;
            _modifiedImagePath = modifiedImagePath;

            _magickSettings = new MagickReadSettings(); 
            // Settings the density to 300 dpi will create an image with a better quality
            _magickSettings.Density = new Density(ReducedDensity, ReducedDensity);
            _magickSettings.ColorSpace = ColorSpace.sRGB;

            _magickSettingsHigh = new MagickReadSettings(); 
            // Settings the density to 300 dpi will create an image with a better quality
            _magickSettingsHigh.Density = new Density(Density, Density);
            _magickSettingsHigh.ColorSpace = ColorSpace.sRGB;
        }

        public Stream Compare()
        {
            IMagickImage modifiedHigh = ReadPDF(_modifiedImagePath, _magickSettingsHigh);
            IMagickImage original = ReadPDF(_originalImagePath, _magickSettings);
            IMagickImage modified = ReadPDF(_modifiedImagePath, _magickSettings);
            
            IMagickImage mask = GenerateMask(original, modified);
            mask.Resize(new Percentage((double)Density / (double)ReducedDensity * 100d));
            modifiedHigh.Composite(mask, Gravity.Center, CompositeOperator.Over);

            MemoryStream ms = new MemoryStream();
            modifiedHigh.Write(ms);
            ms.Position = 0;

            return ms;
        }

        private IMagickImage ReadPDF(string path, MagickReadSettings settings)
        {
            var images = new MagickImageCollection();

            // Add all the pages of the pdf file to the collection
            images.Read(path, settings);

            if (images.Count > 1)
                throw new ArgumentOutOfRangeException("Too many pages in pdf");

            IMagickImage newImage = images[0];
            newImage.Format = MagickFormat.Png;
            return newImage;
        }

        private IMagickImage GenerateMask(IMagickImage original, IMagickImage modified)
        {
            int brushWidth = 10;
            int scale = 10;

            IMagickImage mask = new MagickImage(MagickColors.Transparent, original.Width, original.Height);
            mask.Format = MagickFormat.Png;
            
            IPixelCollection originalPC = original.GetPixels();
            IPixelCollection modifiedPC = modified.GetPixels();
            IPixelCollection maskPC = mask.GetPixels();
            
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Pixel originalPixel = originalPC.GetPixel(x, y);
                    Pixel modifiedPixel = modifiedPC.GetPixel(x, y);
                    
                    if (!originalPixel.Equals(modifiedPixel))
                    {
                        Pixel p = maskPC.GetPixel(x, y);
                        p.SetChannel(0, ushort.MaxValue);
                        p.SetChannel(1, 0);
                        p.SetChannel(2, 0);
                        p.SetChannel(3, ushort.MaxValue / 3);
                    }
                }
            }
            
            return mask;
        }
    }
}
