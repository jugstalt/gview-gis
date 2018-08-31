//using AForge.Imaging.Filters;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using AForge.Imaging;
//using AForge.Math.Random;
//using AForge;
//using System.Drawing.Imaging;

//namespace gView.Drawing.Pro
//{
public enum ImageProcessingFilters
{
    Default,
    GrayscaleBT709,
    GrayscaleRMY,
    GrayscaleY,
    BayerFilter,
    Channel_Red,
    Channel_Green,
    Channel_Blue,
    RotateChannels,
    RotateChannels2,
    WaterWave,
    Sepia,
    BrightnessCorrection,
    ContrastCorrection,
    SaturationCorrection1,
    SaturationCorrection2,
    SaturationCorrection3,
    Invert,
    Blur,
    AdditiveNoise,
    GammaCorrection,
    HistogramEqualization,
    OrderedDithering,
    Pixallete,
    SimplePosterization,
    Texturer_Textile,
    Texturer_Cloud,
    Texturer_Marble,
    Texturer_Wood,
    Texturer_Labyrinth,
    Drawing,
    DrawingSepia,

    SobelEdgeDetector,
    SobelEdgeDetectorInvert,
    SobelEdgeDetectorSepia,
    SobelEdgeDetectorSepiaCanvas,

    OilCanvas,
    OilCanvasGray,
    OilCanvasSepia
}

//    public enum ImageProcessingFilterCategory
//    {
//        All = 0,
//        Standard = 1,
//        Art = 2,
//        Color = 4,
//        Correction = 8
//    }

//    public class ImageProcessing
//    {
//        #region Filters

//        private static byte[] ApplyFilter(byte[] imageBytes, IFilter filter, ImageFormat format = null)
//        {
//            if (imageBytes == null)
//                return null;

//            try
//            {
//                using (System.Drawing.Bitmap from = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(imageBytes)))
//                {
//                    using (Bitmap bm = filter.Apply(from))
//                    {
//                        return ImageOperations.Image2Bytes(bm, format ?? from.RawFormat);
//                    }

//                }
//            }
//            catch (Exception ex)
//            {
//                string msg = ex.Message;
//            }

//            return null;
//        }

//        public static byte[] ApplyFilter(System.Drawing.Image image, ImageProcessingFilters filter, ImageFormat format = null)
//        {
//            try
//            {
//                return ApplyFilter(ImageOperations.Image2Bytes(image), filter, format);
//            }
//            catch { return null; }
//        }

//        public static byte[] ApplyFilter(byte[] imageBytes, ImageProcessingFilters filter, ImageFormat format = null)
//        {
//            IFilter baseFilter = null;

//            switch (filter)
//            {
//                case ImageProcessingFilters.Default:
//                    return imageBytes;
//                case ImageProcessingFilters.GrayscaleBT709:
//                    baseFilter = new GrayscaleBT709();
//                    break;
//                case ImageProcessingFilters.GrayscaleRMY:
//                    baseFilter = new GrayscaleRMY();
//                    break;
//                case ImageProcessingFilters.GrayscaleY:
//                    baseFilter = new GrayscaleY();
//                    break;
//                case ImageProcessingFilters.BayerFilter:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new ExtractChannel(RGB.B));
//                    ((FiltersSequence)baseFilter).Add(new BayerFilter());
//                    break;
//                /*
//            case ImageProcessingFilters.ImageWarp:
//                baseFilter = new ImageWarp(
//                break;
//                 * */
//                case ImageProcessingFilters.Channel_Red:
//                    baseFilter = new ExtractChannel(RGB.R);
//                    break;
//                case ImageProcessingFilters.Channel_Green:
//                    baseFilter = new ExtractChannel(RGB.G);
//                    break;
//                case ImageProcessingFilters.Channel_Blue:
//                    baseFilter = new ExtractChannel(RGB.B);
//                    break;
//                case ImageProcessingFilters.WaterWave:
//                    baseFilter = new WaterWave();
//                    ((WaterWave)baseFilter).HorizontalWavesCount = 10;
//                    ((WaterWave)baseFilter).HorizontalWavesAmplitude = 5;
//                    ((WaterWave)baseFilter).VerticalWavesCount = 3;
//                    ((WaterWave)baseFilter).VerticalWavesAmplitude = 15;
//                    break;
//                case ImageProcessingFilters.Sepia:
//                    baseFilter = new Sepia();
//                    break;
//                case ImageProcessingFilters.BrightnessCorrection:
//                    baseFilter = new BrightnessCorrection(-50);
//                    break;
//                case ImageProcessingFilters.ContrastCorrection:
//                    baseFilter = new ContrastCorrection(15);
//                    break;
//                case ImageProcessingFilters.SaturationCorrection1:
//                    baseFilter = new SaturationCorrection(-0.5f);
//                    break;
//                case ImageProcessingFilters.SaturationCorrection2:
//                    baseFilter = new SaturationCorrection(-.25f);
//                    break;
//                case ImageProcessingFilters.SaturationCorrection3:
//                    baseFilter = new SaturationCorrection(+0.5f);
//                    break;
//                case ImageProcessingFilters.Invert:
//                    baseFilter = new Invert();
//                    break;
//                case ImageProcessingFilters.Blur:
//                    baseFilter = new Blur();
//                    break;
//                case ImageProcessingFilters.RotateChannels:
//                    baseFilter = new RotateChannels();
//                    break;
//                case ImageProcessingFilters.RotateChannels2:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new RotateChannels());
//                    ((FiltersSequence)baseFilter).Add(new RotateChannels());
//                    break;
//                case ImageProcessingFilters.AdditiveNoise:
//                    IRandomNumberGenerator generator = new UniformGenerator(new Range(-50, 50));
//                    baseFilter = new AdditiveNoise(generator);
//                    break;
//                case ImageProcessingFilters.GammaCorrection:
//                    baseFilter = new GammaCorrection(0.5);
//                    break;
//                case ImageProcessingFilters.HistogramEqualization:
//                    baseFilter = new HistogramEqualization();
//                    break;
//                case ImageProcessingFilters.OrderedDithering:
//                    byte[,] matrix = new byte[4, 4]
//                    {
//                        {  95, 233, 127, 255 },
//                        { 159,  31, 191,  63 },
//                        { 111, 239,  79, 207 },
//                        { 175,  47, 143,  15 }
//                    };
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleBT709());
//                    ((FiltersSequence)baseFilter).Add(new OrderedDithering(matrix));
//                    break;
//                case ImageProcessingFilters.Pixallete:
//                    baseFilter = new Pixellate();
//                    break;
//                case ImageProcessingFilters.SimplePosterization:
//                    baseFilter = new SimplePosterization();
//                    break;
//                case ImageProcessingFilters.Texturer_Textile:
//                    baseFilter = new Texturer(new AForge.Imaging.Textures.TextileTexture(), 0.3, 0.7);
//                    break;
//                case ImageProcessingFilters.Texturer_Cloud:
//                    baseFilter = new Texturer(new AForge.Imaging.Textures.CloudsTexture(), 0.3, 0.7);
//                    break;
//                case ImageProcessingFilters.Texturer_Marble:
//                    baseFilter = new Texturer(new AForge.Imaging.Textures.MarbleTexture(), 0.3, 0.7);
//                    break;
//                case ImageProcessingFilters.Texturer_Wood:
//                    baseFilter = new Texturer(new AForge.Imaging.Textures.WoodTexture(), 0.3, 0.7);
//                    break;
//                case ImageProcessingFilters.Texturer_Labyrinth:
//                    baseFilter = new Texturer(new AForge.Imaging.Textures.LabyrinthTexture(), 0.3, 0.7);
//                    break;
//                case ImageProcessingFilters.SobelEdgeDetector:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new ExtractChannel(RGB.R));
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    break;
//                case ImageProcessingFilters.SobelEdgeDetectorInvert:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new ExtractChannel(RGB.R));
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    ((FiltersSequence)baseFilter).Add(new Invert());
//                    break;
//                case ImageProcessingFilters.SobelEdgeDetectorSepia:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new ExtractChannel(RGB.R));
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleToRGB());
//                    ((FiltersSequence)baseFilter).Add(new Sepia());
//                    break;
//                case ImageProcessingFilters.SobelEdgeDetectorSepiaCanvas:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new ExtractChannel(RGB.R));
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleToRGB());
//                    ((FiltersSequence)baseFilter).Add(new Sepia());
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    ((FiltersSequence)baseFilter).Add(new Texturer(new AForge.Imaging.Textures.TextileTexture(), 0.3, 0.7));
//                    break;
//                case ImageProcessingFilters.Drawing:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleBT709());
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    ((FiltersSequence)baseFilter).Add(new Invert());
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    break;
//                case ImageProcessingFilters.DrawingSepia:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleBT709());
//                    ((FiltersSequence)baseFilter).Add(new SobelEdgeDetector());
//                    ((FiltersSequence)baseFilter).Add(new Invert());
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleToRGB());
//                    ((FiltersSequence)baseFilter).Add(new Sepia());
//                    break;
//                case ImageProcessingFilters.OilCanvas:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    ((FiltersSequence)baseFilter).Add(new Texturer(new AForge.Imaging.Textures.TextileTexture(), 0.3, 0.7));
//                    break;
//                case ImageProcessingFilters.OilCanvasGray:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    ((FiltersSequence)baseFilter).Add(new Texturer(new AForge.Imaging.Textures.TextileTexture(), 0.3, 0.7));
//                    ((FiltersSequence)baseFilter).Add(new GrayscaleBT709());
//                    break;
//                case ImageProcessingFilters.OilCanvasSepia:
//                    baseFilter = new FiltersSequence();
//                    ((FiltersSequence)baseFilter).Add(new SimplePosterization());
//                    ((FiltersSequence)baseFilter).Add(new Texturer(new AForge.Imaging.Textures.TextileTexture(), 0.3, 0.7));
//                    ((FiltersSequence)baseFilter).Add(new Sepia());
//                    break;
//            }

//            if (baseFilter == null)
//                return null;

//            return ApplyFilter(imageBytes, baseFilter, format);
//        }

//        public static ImageProcessingFilters[] Filters(ImageProcessingFilterCategory cat)
//        {
//            List<ImageProcessingFilters> ret = new List<ImageProcessingFilters>();
//            if (cat == ImageProcessingFilterCategory.All)
//            {
//                cat = ImageProcessingFilterCategory.Standard | ImageProcessingFilterCategory.Color | ImageProcessingFilterCategory.Correction | ImageProcessingFilterCategory.Art;
//                /*
//                foreach (var f in Enum.GetValues(typeof(ImageProcessingFilters)))
//                {
//                    ret.Add((ImageProcessingFilters)f);
//                }
//                 * */
//            }
//            if (((int)cat & (int)ImageProcessingFilterCategory.Standard) == (int)ImageProcessingFilterCategory.Standard)
//            {
//                if (!ret.Contains(ImageProcessingFilters.GrayscaleBT709)) ret.Add(ImageProcessingFilters.GrayscaleBT709);
//                if (!ret.Contains(ImageProcessingFilters.Sepia)) ret.Add(ImageProcessingFilters.Sepia);
//                if (!ret.Contains(ImageProcessingFilters.Blur)) ret.Add(ImageProcessingFilters.Blur);
//                if (!ret.Contains(ImageProcessingFilters.Invert)) ret.Add(ImageProcessingFilters.Invert);
//            }
//            if (((int)cat & (int)ImageProcessingFilterCategory.Color) == (int)ImageProcessingFilterCategory.Color)
//            {
//                if (!ret.Contains(ImageProcessingFilters.GrayscaleBT709)) ret.Add(ImageProcessingFilters.GrayscaleBT709);
//                if (!ret.Contains(ImageProcessingFilters.GrayscaleRMY)) ret.Add(ImageProcessingFilters.GrayscaleRMY);
//                if (!ret.Contains(ImageProcessingFilters.GrayscaleY)) ret.Add(ImageProcessingFilters.GrayscaleY);
//                if (!ret.Contains(ImageProcessingFilters.Sepia)) ret.Add(ImageProcessingFilters.Sepia);
//                if (!ret.Contains(ImageProcessingFilters.Channel_Red)) ret.Add(ImageProcessingFilters.Channel_Red);
//                if (!ret.Contains(ImageProcessingFilters.Channel_Green)) ret.Add(ImageProcessingFilters.Channel_Green);
//                if (!ret.Contains(ImageProcessingFilters.Channel_Blue)) ret.Add(ImageProcessingFilters.Channel_Blue);
//                if (!ret.Contains(ImageProcessingFilters.RotateChannels)) ret.Add(ImageProcessingFilters.RotateChannels);
//                if (!ret.Contains(ImageProcessingFilters.RotateChannels2)) ret.Add(ImageProcessingFilters.RotateChannels2);
//            }
//            if (((int)cat & (int)ImageProcessingFilterCategory.Correction) == (int)ImageProcessingFilterCategory.Correction)
//            {
//                if (!ret.Contains(ImageProcessingFilters.BrightnessCorrection)) ret.Add(ImageProcessingFilters.BrightnessCorrection);
//                if (!ret.Contains(ImageProcessingFilters.ContrastCorrection)) ret.Add(ImageProcessingFilters.ContrastCorrection);
//                if (!ret.Contains(ImageProcessingFilters.GammaCorrection)) ret.Add(ImageProcessingFilters.GammaCorrection);
//                if (!ret.Contains(ImageProcessingFilters.SaturationCorrection1)) ret.Add(ImageProcessingFilters.SaturationCorrection1);
//                if (!ret.Contains(ImageProcessingFilters.SaturationCorrection2)) ret.Add(ImageProcessingFilters.SaturationCorrection2);
//                if (!ret.Contains(ImageProcessingFilters.SaturationCorrection3)) ret.Add(ImageProcessingFilters.SaturationCorrection3);
//            }
//            if (((int)cat & (int)ImageProcessingFilterCategory.Art) == (int)ImageProcessingFilterCategory.Art)
//            {
//                if (!ret.Contains(ImageProcessingFilters.SimplePosterization)) ret.Add(ImageProcessingFilters.SimplePosterization);
//                if (!ret.Contains(ImageProcessingFilters.OilCanvas)) ret.Add(ImageProcessingFilters.OilCanvas);
//                if (!ret.Contains(ImageProcessingFilters.OilCanvasGray)) ret.Add(ImageProcessingFilters.OilCanvasGray);
//                if (!ret.Contains(ImageProcessingFilters.OilCanvasSepia)) ret.Add(ImageProcessingFilters.OilCanvasSepia);
//                if (!ret.Contains(ImageProcessingFilters.Drawing)) ret.Add(ImageProcessingFilters.Drawing);
//                if (!ret.Contains(ImageProcessingFilters.DrawingSepia)) ret.Add(ImageProcessingFilters.DrawingSepia);
//                if (!ret.Contains(ImageProcessingFilters.SobelEdgeDetector)) ret.Add(ImageProcessingFilters.SobelEdgeDetector);
//                if (!ret.Contains(ImageProcessingFilters.SobelEdgeDetectorInvert)) ret.Add(ImageProcessingFilters.SobelEdgeDetectorInvert);
//                if (!ret.Contains(ImageProcessingFilters.SobelEdgeDetectorSepia)) ret.Add(ImageProcessingFilters.SobelEdgeDetectorSepia);
//                if (!ret.Contains(ImageProcessingFilters.SobelEdgeDetectorSepiaCanvas)) ret.Add(ImageProcessingFilters.SobelEdgeDetectorSepiaCanvas);
//                if (!ret.Contains(ImageProcessingFilters.OrderedDithering)) ret.Add(ImageProcessingFilters.OrderedDithering);
//                if (!ret.Contains(ImageProcessingFilters.WaterWave)) ret.Add(ImageProcessingFilters.WaterWave);
//            }

//            return ret.ToArray();
//        }
//        #endregion
//    }
//}
