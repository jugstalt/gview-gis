using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace gView.Drawing.Pro.Exif
{
    public sealed class ExifTagCollection : IEnumerable<ExifTag>
    {
        private Dictionary<int, ExifTag> _tags;

        #region Constructors
        public ExifTagCollection(string fileName)
            : this(fileName, true, false)
        {
        }

        public ExifTagCollection(string fileName, bool useEmbeddedColorManagement, bool validateImageData)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                Image image = System.Drawing.Image.FromStream(stream,
                    useEmbeddedColorManagement,
                    validateImageData);

                ReadTags(image.PropertyItems);
            }
        }

        public ExifTagCollection(Image image)
        {
            ReadTags(image.PropertyItems);
        }
        #endregion

        #region Private Methods
        private void ReadTags(PropertyItem[] pitems)
        {
            Encoding ascii = Encoding.ASCII;
            _tags = new Dictionary<int, ExifTag>();

            foreach (PropertyItem pitem in pitems)
            {
                ExifTag tag = SupportedTags.Tag(pitem.Id);

                if (tag == null) continue;

                string value = "";

                if (pitem.Type == 0x1)
                {
                    #region BYTE (8-bit unsigned int)
                    if (pitem.Value.Length == 4)
                        value = "Version " + pitem.Value[0].ToString() + "." + pitem.Value[1].ToString();
                    else if (pitem.Id == 0x5 && pitem.Value[0] == 0)
                        value = "Sea level";
                    else
                        value = pitem.Value[0].ToString();
                    #endregion
                }
                else if (pitem.Type == 0x2)
                {
                    #region ASCII (8 bit ASCII code)

                    value = ascii.GetString(pitem.Value).Trim('\0');

                    if (pitem.Id == 0x1 || pitem.Id == 0x13)
                        if (value == "N") value = "North latitude";
                        else if (value == "S") value = "South latitude";
                        else value = "reserved";

                    if (pitem.Id == 0x3 || pitem.Id == 0x15)
                        if (value == "E") value = "East longitude";
                        else if (value == "W") value = "West longitude";
                        else value = "reserved";

                    if (pitem.Id == 0x9)
                        if (value == "A") value = "Measurement in progress";
                        else if (value == "V") value = "Measurement Interoperability";
                        else value = "reserved";

                    if (pitem.Id == 0xA)
                        if (value == "2") value = "2-dimensional measurement";
                        else if (value == "3") value = "3-dimensional measurement";
                        else value = "reserved";

                    if (pitem.Id == 0xC || pitem.Id == 0x19)
                        if (value == "K") value = "Kilometers per hour";
                        else if (value == "M") value = "Miles per hour";
                        else if (value == "N") value = "Knots";
                        else value = "reserved";

                    if (pitem.Id == 0xE || pitem.Id == 0x10 || pitem.Id == 0x17)
                        if (value == "T") value = "True direction";
                        else if (value == "M") value = "Magnetic direction";
                        else value = "reserved";
                    #endregion
                }
                else if (pitem.Type == 0x3)
                {
                    #region 3 = SHORT (16-bit unsigned int)

                    UInt16 uintval = BitConverter.ToUInt16(pitem.Value, 0);

                    // orientation // lookup table					
                    switch (pitem.Id)
                    {
                        case 0x8827: // ISO speed rating
                            value = "ISO-" + uintval.ToString();
                            break;
                        case 0xA217: // sensing method
                            {
                                switch (uintval)
                                {
                                    case 1: value = "Not defined"; break;
                                    case 2: value = "One-chip color area sensor"; break;
                                    case 3: value = "Two-chip color area sensor"; break;
                                    case 4: value = "Three-chip color area sensor"; break;
                                    case 5: value = "Color sequential area sensor"; break;
                                    case 7: value = "Trilinear sensor"; break;
                                    case 8: value = "Color sequential linear sensor"; break;
                                    default: value = " reserved"; break;
                                }
                            }
                            break;
                        case 0x8822: // Exposure program
                            switch (uintval)
                            {
                                case 0: value = "Not defined"; break;
                                case 1: value = "Manual"; break;
                                case 2: value = "Normal program"; break;
                                case 3: value = "Aperture priority"; break;
                                case 4: value = "Shutter priority"; break;
                                case 5: value = "Creative program (biased toward depth of field)"; break;
                                case 6: value = "Action program (biased toward fast shutter speed)"; break;
                                case 7: value = "Portrait mode (for closeup photos with the background out of focus)"; break;
                                case 8: value = "Landscape mode (for landscape photos with the background in focus)"; break;
                                default: value = "reserved"; break;
                            }
                            break;
                        case 0x9207: // metering mode
                            switch (uintval)
                            {
                                case 0: value = "unknown"; break;
                                case 1: value = "Average"; break;
                                case 2: value = "Center Weighted Average"; break;
                                case 3: value = "Spot"; break;
                                case 4: value = "MultiSpot"; break;
                                case 5: value = "Pattern"; break;
                                case 6: value = "Partial"; break;
                                case 255: value = "Other"; break;
                                default: value = "reserved"; break;
                            }
                            break;
                        case 0x9208: // Light source
                            {
                                switch (uintval)
                                {
                                    case 0: value = "unknown"; break;
                                    case 1: value = "Daylight"; break;
                                    case 2: value = "Fluorescent"; break;
                                    case 3: value = "Tungsten (incandescent light)"; break;
                                    case 4: value = "Flash"; break;
                                    case 9: value = "Fine weather"; break;
                                    case 10: value = "Cloudy weather"; break;
                                    case 11: value = "Shade"; break;
                                    case 12: value = "Daylight fluorescent (D 5700 – 7100K)"; break;
                                    case 13: value = "Day white fluorescent (N 4600 – 5400K)"; break;
                                    case 14: value = "Cool white fluorescent (W 3900 – 4500K)"; break;
                                    case 15: value = "White fluorescent (WW 3200 – 3700K)"; break;
                                    case 17: value = "Standard light A"; break;
                                    case 18: value = "Standard light B"; break;
                                    case 19: value = "Standard light C"; break;
                                    case 20: value = "D55"; break;
                                    case 21: value = "D65"; break;
                                    case 22: value = "D75"; break;
                                    case 23: value = "D50"; break;
                                    case 24: value = "ISO studio tungsten"; break;
                                    case 255: value = "ISO studio tungsten"; break;
                                    default: value = "other light source"; break;
                                }
                            }
                            break;
                        case 0x9209: // Flash
                            {
                                switch (uintval)
                                {
                                    case 0x0: value = "Flash did not fire"; break;
                                    case 0x1: value = "Flash fired"; break;
                                    case 0x5: value = "Strobe return light not detected"; break;
                                    case 0x7: value = "Strobe return light detected"; break;
                                    case 0x9: value = "Flash fired, compulsory flash mode"; break;
                                    case 0xD: value = "Flash fired, compulsory flash mode, return light not detected"; break;
                                    case 0xF: value = "Flash fired, compulsory flash mode, return light detected"; break;
                                    case 0x10: value = "Flash did not fire, compulsory flash mode"; break;
                                    case 0x18: value = "Flash did not fire, auto mode"; break;
                                    case 0x19: value = "Flash fired, auto mode"; break;
                                    case 0x1D: value = "Flash fired, auto mode, return light not detected"; break;
                                    case 0x1F: value = "Flash fired, auto mode, return light detected"; break;
                                    case 0x20: value = "No flash function"; break;
                                    case 0x41: value = "Flash fired, red-eye reduction mode"; break;
                                    case 0x45: value = "Flash fired, red-eye reduction mode, return light not detected"; break;
                                    case 0x47: value = "Flash fired, red-eye reduction mode, return light detected"; break;
                                    case 0x49: value = "Flash fired, compulsory flash mode, red-eye reduction mode"; break;
                                    case 0x4D: value = "Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected"; break;
                                    case 0x4F: value = "Flash fired, compulsory flash mode, red-eye reduction mode, return light detected"; break;
                                    case 0x59: value = "Flash fired, auto mode, red-eye reduction mode"; break;
                                    case 0x5D: value = "Flash fired, auto mode, return light not detected, red-eye reduction mode"; break;
                                    case 0x5F: value = "Flash fired, auto mode, return light detected, red-eye reduction mode"; break;
                                    default: value = "reserved"; break;
                                }
                            }
                            break;
                        case 0x0128: //ResolutionUnit
                            {
                                switch (uintval)
                                {
                                    case 2: value = "Inch"; break;
                                    case 3: value = "Centimeter"; break;
                                    default: value = "No Unit"; break;
                                }
                            }
                            break;
                        case 0xA409: // Saturation
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Normal"; break;
                                    case 1: value = "Low saturation"; break;
                                    case 2: value = "High saturation"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;

                        case 0xA40A: // Sharpness
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Normal"; break;
                                    case 1: value = "Soft"; break;
                                    case 2: value = "Hard"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA408: // Contrast
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Normal"; break;
                                    case 1: value = "Soft"; break;
                                    case 2: value = "Hard"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0x103: // Compression
                            {
                                switch (uintval)
                                {
                                    case 1: value = "Uncompressed"; break;
                                    case 6: value = "JPEG compression (thumbnails only)"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0x106: // PhotometricInterpretation
                            {
                                switch (uintval)
                                {
                                    case 2: value = "RGB"; break;
                                    case 6: value = "YCbCr"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0x112: // Orientation
                            {
                                /*
                                switch (uintval)
                                {
                                    case 1: value = "The 0th row is at the visual top of the image, and the 0th column is the visual left-hand side."; break;
                                    case 2: value = "The 0th row is at the visual top of the image, and the 0th column is the visual right-hand side."; break;
                                    case 3: value = "The 0th row is at the visual bottom of the image, and the 0th column is the visual right-hand side."; break;
                                    case 4: value = "The 0th row is at the visual bottom of the image, and the 0th column is the visual left-hand side."; break;
                                    case 5: value = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual top."; break;
                                    case 6: value = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual top."; break;
                                    case 7: value = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual bottom."; break;
                                    case 8: value = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual bottom."; break;
                                    default: value = "Reserved"; break;
                                }
                                 * */
                                value = uintval.ToString();
                            }
                            break;
                        case 0x213: // YCbCrPositioning
                            {
                                switch (uintval)
                                {
                                    case 1: value = "centered"; break;
                                    case 6: value = "co-sited"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA001: // ColorSpace
                            {
                                switch (uintval)
                                {
                                    case 1: value = "sRGB"; break;
                                    case 0xFFFF: value = "Uncalibrated"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA401: // CustomRendered
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Normal process"; break;
                                    case 1: value = "Custom process"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA402: // ExposureMode
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Auto exposure"; break;
                                    case 1: value = "Manual exposure"; break;
                                    case 2: value = "Auto bracket"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA403: // WhiteBalance
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Auto white balance"; break;
                                    case 1: value = "Manual white balance"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA406: // SceneCaptureType
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Standard"; break;
                                    case 1: value = "Landscape"; break;
                                    case 2: value = "Portrait"; break;
                                    case 3: value = "Night scene"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;

                        case 0xA40C: // SubjectDistanceRange
                            {
                                switch (uintval)
                                {
                                    case 0: value = "unknown"; break;
                                    case 1: value = "Macro"; break;
                                    case 2: value = "Close view"; break;
                                    case 3: value = "Distant view"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0x1E: // GPSDifferential
                            {
                                switch (uintval)
                                {
                                    case 0: value = "Measurement without differential correction"; break;
                                    case 1: value = "Differential correction applied"; break;
                                    default: value = "Reserved"; break;
                                }
                            }
                            break;
                        case 0xA405: // FocalLengthIn35mmFilm
                            value = uintval.ToString() + " mm";
                            break;
                        default://
                            value = uintval.ToString();
                            break;
                    }
                    #endregion
                }
                else if (pitem.Type == 0x4)
                {
                    #region 4 = LONG (32-bit unsigned int)
                    value = BitConverter.ToUInt32(pitem.Value, 0).ToString();
                    #endregion
                }
                else if (pitem.Type == 0x5)
                {
                    #region 5 = RATIONAL (Two LONGs, unsigned)

                    URational rat = new URational(pitem.Value);

                    switch (pitem.Id)
                    {
                        case 0x9202: // ApertureValue
                            value = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                            break;
                        case 0x9205: // MaxApertureValue
                            value = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                            break;
                        case 0x920A: // FocalLength
                            value = rat.ToDouble().ToString() + " mm";
                            break;
                        case 0x829D: // F-number
                            value = "F/" + rat.ToDouble().ToString();
                            break;
                        case 0x11A: // Xresolution
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x11B: // Yresolution
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x829A: // ExposureTime
                            value = rat.ToString() + " sec";
                            break;
                        case 0x2: // GPSLatitude                                
                            value = new GPSRational(pitem.Value).ToString();
                            break;
                        case 0x4: // GPSLongitude
                            value = new GPSRational(pitem.Value).ToString();
                            break;
                        case 0x6: // GPSAltitude
                            value = rat.ToDouble() + " meters";
                            break;
                        case 0xA404: // Digital Zoom Ratio
                            value = rat.ToDouble().ToString();
                            if (value == "0") value = "none";
                            break;
                        case 0xB: // GPSDOP
                            value = rat.ToDouble().ToString();
                            break;
                        case 0xD: // GPSSpeed
                            value = rat.ToDouble().ToString();
                            break;
                        case 0xF: // GPSTrack
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x11: // GPSImgDir
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x14: // GPSDestLatitude
                            value = new GPSRational(pitem.Value).ToString();
                            break;
                        case 0x16: // GPSDestLongitude
                            value = new GPSRational(pitem.Value).ToString();
                            break;
                        case 0x18: // GPSDestBearing
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x1A: // GPSDestDistance
                            value = rat.ToDouble().ToString();
                            break;
                        case 0x7: // GPSTimeStamp                                
                            value = new GPSRational(pitem.Value).ToString(":");
                            break;

                        default:
                            value = rat.ToString();
                            break;
                    }

                    #endregion
                }
                else if (pitem.Type == 0x7)
                {
                    #region UNDEFINED (8-bit)
                    switch (pitem.Id)
                    {
                        case 0xA300: //FileSource
                            {
                                if (pitem.Value[0] == 3)
                                    value = "DSC";
                                else
                                    value = "reserved";
                                break;
                            }
                        case 0xA301: //SceneType
                            if (pitem.Value[0] == 1)
                                value = "A directly photographed image";
                            else
                                value = "reserved";
                            break;
                        case 0x9000:// Exif Version
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0xA000: // Flashpix Version
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            if (value == "0100")
                                value = "Flashpix Format Version 1.0";
                            else value = "reserved";
                            break;
                        case 0x9101: //ComponentsConfiguration
                            value = GetComponentsConfig(pitem.Value);
                            break;
                        case 0x927C: //MakerNote
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x9286: //UserComment
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x1B: //GPS Processing Method
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        case 0x1C: //GPS Area Info
                            value = ascii.GetString(pitem.Value).Trim('\0');
                            break;
                        default:
                            value = "-";
                            break;
                    }
                    #endregion
                }
                else if (pitem.Type == 0x9)
                {
                    #region 9 = SLONG (32-bit int)
                    value = BitConverter.ToInt32(pitem.Value, 0).ToString();
                    #endregion
                }
                else if (pitem.Type == 0xA)
                {
                    #region 10 = SRATIONAL (Two SLONGs, signed)

                    Rational rat = new Rational(pitem.Value);

                    switch (pitem.Id)
                    {
                        case 0x9201: // ShutterSpeedValue
                            value = "1/" + Math.Round(Math.Pow(2, rat.ToDouble()), 2).ToString();
                            break;
                        case 0x9203: // BrightnessValue
                            value = Math.Round(rat.ToDouble(), 4).ToString();
                            break;
                        case 0x9204: // ExposureBiasValue
                            value = Math.Round(rat.ToDouble(), 2).ToString() + " eV";
                            break;
                        default:
                            value = rat.ToString();
                            break;
                    }
                    #endregion
                }

                tag.Value = value;

                _tags.Add(tag.Id, tag);
            }
        }

        private static string GetComponentsConfig(byte[] bytes)
        {
            string s = "";
            string[] vals = new string[] { "", "Y", "Cb", "Cr", "R", "G", "B" };

            foreach (byte b in bytes)
                s += vals[b];

            return s;
        }
        #endregion

        #region IEnumerable<ExifTag> Members

        public IEnumerator<ExifTag> GetEnumerator()
        {
            return _tags.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tags.Values.GetEnumerator();
        }

        #endregion

        #region Indexers
        
        public ExifTag this[int id]
        {
            get
            {
                if (id < 0 || !_tags.ContainsKey(id)) return null;

                return _tags[id];
            }
        }

        public ExifTag this[string name]
        {
            get
            {
                return this[SupportedTags.IdByName(name)];
            }
        }

        #endregion
    }

    internal static class SupportedTags
    {
        private static Dictionary<int, ExifTag> _tags = new Dictionary<int, ExifTag>()
        {
            {0x100, new ExifTag(0x100, "ImageWidth", "Image width")},
            {0x101, new ExifTag(0x101, "ImageHeight", "Image height")},
            {0x0, new ExifTag(0x0, "GPSVersionID", "GPS tag version")},
            {0x5, new ExifTag(0x5, "GPSAltitudeRef", "Altitude reference")},
            {0x111, new ExifTag(0x111, "StripOffsets", "Image data location")},
            {0x116, new ExifTag(0x116, "RowsPerStrip", "Number of rows per strip")},
            {0x117, new ExifTag(0x117, "StripByteCounts", "Bytes per compressed strip")},
            {0xA002, new ExifTag(0xA002, "PixelXDimension", "Valid image width")},
            {0xA003, new ExifTag(0xA003, "PixelYDimension", "Valid image height")},
            {0x102, new ExifTag(0x102, "BitsPerSample", "Number of bits per component")},
            {0x103, new ExifTag(0x103, "Compression", "Compression scheme")},
            {0x106, new ExifTag(0x106, "PhotometricInterpretation", "Pixel composition")},
            {0x112, new ExifTag(0x112, "Orientation", "Orientation of image")},
            {0x115, new ExifTag(0x115, "SamplesPerPixel", "Number of components")},
            {0x11C, new ExifTag(0x11C, "PlanarConfiguration", "Image data arrangement")},
            {0x212, new ExifTag(0x212, "YCbCrSubSampling", "Subsampling ratio of Y to C")},
            {0x213, new ExifTag(0x213, "YCbCrPositioning", "Y and C positioning")},
            {0x128, new ExifTag(0x128, "ResolutionUnit", "Unit of X and Y resolution")},
            {0x12D, new ExifTag(0x12D, "TransferFunction", "Transfer function")},
            {0xA001, new ExifTag(0xA001, "ColorSpace", "Color space information")},
            {0x8822, new ExifTag(0x8822, "ExposureProgram", "Exposure program")},
            {0x8827, new ExifTag(0x8827, "ISOSpeedRatings", "ISO speed rating")},
            {0x9207, new ExifTag(0x9207, "MeteringMode", "Metering mode")},
            {0x9208, new ExifTag(0x9208, "LightSource", "Light source")},
            {0x9209, new ExifTag(0x9209, "Flash", "Flash")},
            {0x9214, new ExifTag(0x9214, "SubjectArea", "Subject area")},
            {0xA210, new ExifTag(0xA210, "FocalPlaneResolutionUnit", "Focal plane resolution unit")},
            {0xA214, new ExifTag(0xA214, "SubjectLocation", "Subject location")},
            {0xA217, new ExifTag(0xA217, "SensingMethod", "Sensing method")},
            {0xA401, new ExifTag(0xA401, "CustomRendered", "Custom image processing")},
            {0xA402, new ExifTag(0xA402, "ExposureMode", "Exposure mode")},
            {0xA403, new ExifTag(0xA403, "WhiteBalance", "White balance")},
            {0xA405, new ExifTag(0xA405, "FocalLengthIn35mmFilm", "Focal length in 35 mm film")},
            {0xA406, new ExifTag(0xA406, "SceneCaptureType", "Scene capture type")},
            {0xA408, new ExifTag(0xA408, "Contrast", "Contrast")},
            {0xA409, new ExifTag(0xA409, "Saturation", "Saturation")},
            {0xA40A, new ExifTag(0xA40A, "Sharpness", "Sharpness")},
            {0xA40C, new ExifTag(0xA40C, "SubjectDistanceRange", "Subject distance range")},
            {0x1E, new ExifTag(0x1E, "GPSDifferential", "GPS differential correction")},
            {0x9201, new ExifTag(0x9201, "ShutterSpeedValue", "Shutter speed")},
            {0x9203, new ExifTag(0x9203, "BrightnessValue", "Brightness")},
            {0x9204, new ExifTag(0x9204, "ExposureBiasValue", "Exposure bias")},
            {0x201, new ExifTag(0x201, "JPEGInterchangeFormat", "Offset to JPEG SOI")},
            {0x202, new ExifTag(0x202, "JPEGInterchangeFormatLength", "Bytes of JPEG data")},
            {0x11A, new ExifTag(0x11A, "XResolution", "Image resolution in width direction")},
            {0x11B, new ExifTag(0x11B, "YResolution", "Image resolution in height direction")},
            {0x13E, new ExifTag(0x13E, "WhitePoint", "White point chromaticity")},
            {0x13F, new ExifTag(0x13F, "PrimaryChromaticities", "Chromaticities of primaries")},
            {0x211, new ExifTag(0x211, "YCbCrCoefficients", "Color space transformation matrix coefficients")},
            {0x214, new ExifTag(0x214, "ReferenceBlackWhite", "Pair of black and white reference values")},
            {0x9102, new ExifTag(0x9102, "CompressedBitsPerPixel", "Image compression mode")},
            {0x829A, new ExifTag(0x829A, "ExposureTime", "Exposure time")},
            {0x829D, new ExifTag(0x829D, "FNumber", "F number")},
            {0x9202, new ExifTag(0x9202, "ApertureValue", "Aperture")},
            {0x9205, new ExifTag(0x9205, "MaxApertureValue", "Maximum lens aperture")},
            {0x9206, new ExifTag(0x9206, "SubjectDistance", "Subject distance")},
            {0x920A, new ExifTag(0x920A, "FocalLength", "Lens focal length")},
            {0xA20B, new ExifTag(0xA20B, "FlashEnergy", "Flash energy")},
            {0xA20E, new ExifTag(0xA20E, "FocalPlaneXResolution", "Focal plane X resolution")},
            {0xA20F, new ExifTag(0xA20F, "FocalPlaneYResolution", "Focal plane Y resolution")},
            {0xA215, new ExifTag(0xA215, "ExposureIndex", "Exposure index")},
            {0xA404, new ExifTag(0xA404, "DigitalZoomRatio", "Digital zoom ratio")},
            {0xA407, new ExifTag(0xA407, "GainControl", "Gain control")},
            {0x2, new ExifTag(0x2, "GPSLatitude", "Latitude")},
            {0x4, new ExifTag(0x4, "GPSLongitude", "Longitude")},
            {0x6, new ExifTag(0x6, "GPSAltitude", "Altitude")},
            {0x7, new ExifTag(0x7, "GPSTimeStamp", "GPS time (atomic clock)")},
            {0xB, new ExifTag(0xB, "GPSDOP", "Measurement precision")},
            {0xD, new ExifTag(0xD, "GPSSpeed", "Speed of GPS receiver")},
            {0xF, new ExifTag(0xF, "GPSTrack", "Direction of movement")},
            {0x11, new ExifTag(0x11, "GPSImgDirection", "Direction of image")},
            {0x14, new ExifTag(0x14, "GPSDestLatitude", "Latitude of destination")},
            {0x16, new ExifTag(0x16, "GPSDestLongitude", "Longitude of destination")},
            {0x18, new ExifTag(0x18, "GPSDestBearing", "Bearing of destination")},
            {0x1A, new ExifTag(0x1A, "GPSDestDistance", "Distance to destination")},
            {0x132, new ExifTag(0x132, "DateTime", "File change date and time")},
            {0x10E, new ExifTag(0x10E, "ImageDescription", "Image title")},
            {0x10F, new ExifTag(0x10F, "Make", "Image input equipment manufacturer")},
            {0x110, new ExifTag(0x110, "Model", "Image input equipment model")},
            {0x131, new ExifTag(0x131, "Software", "Software used")},
            {0x13B, new ExifTag(0x13B, "Artist", "Person who created the image")},
            {0x8298, new ExifTag(0x8298, "Copyright", "Copyright holder")},
            {0xA004, new ExifTag(0xA004, "RelatedSoundFile", "Related audio file")},
            {0x9003, new ExifTag(0x9003, "DateTimeOriginal", "Date and time of original data generation")},
            {0x9004, new ExifTag(0x9004, "DateTimeDigitized", "Date and time of digital data generation")},
            {0x9290, new ExifTag(0x9290, "SubSecTime", "DateTime subseconds")},
            {0x9291, new ExifTag(0x9291, "SubSecTimeOriginal", "DateTimeOriginal subseconds")},
            {0x9292, new ExifTag(0x9292, "SubSecTimeDigitized", "DateTimeDigitized subseconds")},
            {0xA420, new ExifTag(0xA420, "ImageUniqueID", "Unique image ID")},
            {0x8824, new ExifTag(0x8824, "SpectralSensitivity", "Spectral sensitivity")},
            {0x1, new ExifTag(0x1, "GPSLatitudeRef", "North or South Latitude")},
            {0x3, new ExifTag(0x3, "GPSLongitudeRef", "East or West Longitude")},
            {0x8, new ExifTag(0x8, "GPSSatellites", "GPS satellites used for measurement")},
            {0x9, new ExifTag(0x9, "GPSStatus", "GPS receiver status")},
            {0xA, new ExifTag(0xA, "GPSMeasureMode", "GPS measurement mode")},
            {0xC, new ExifTag(0xC, "GPSSpeedRef", "Speed unit")},
            {0xE, new ExifTag(0xE, "GPSTrackRef", "Reference for direction of movement")},
            {0x10, new ExifTag(0x10, "GPSImgDirectionRef", "Reference for direction of image")},
            {0x12, new ExifTag(0x12, "GPSMapDatum", "Geodetic survey data used")},
            {0x13, new ExifTag(0x13, "GPSDestLatitudeRef", "Reference for latitude of destination")},
            {0x15, new ExifTag(0x15, "GPSDestLongitudeRef", "Reference for longitude of destination")},
            {0x17, new ExifTag(0x17, "GPSDestBearingRef", "Reference for bearing of destination")},
            {0x19, new ExifTag(0x19, "GPSDestDistanceRef", "Reference for distance to destination")},
            {0x1D, new ExifTag(0x1D, "GPSDateStamp", "GPS date")},
            {0x8828, new ExifTag(0x8828, "OECF", "Optoelectric conversion factor")},
            {0xA20C, new ExifTag(0xA20C, "SpatialFrequencyResponse", "Spatial frequency response")},
            {0xA300, new ExifTag(0xA300, "FileSource", "File source")},
            {0xA301, new ExifTag(0xA301, "SceneType", "Scene type")},
            {0xA302, new ExifTag(0xA302, "CFAPattern", "CFA pattern")},
            {0xA40B, new ExifTag(0xA40B, "DeviceSettingDescription", "Device settings description")},
            {0x9000, new ExifTag(0x9000, "ExifVersion", "Exif version")},
            {0xA000, new ExifTag(0xA000, "FlashpixVersion", "Supported Flashpix version")},
            {0x9101, new ExifTag(0x9101, "ComponentsConfiguration", "Meaning of each component")},
            {0x927C, new ExifTag(0x927C, "MakerNote", "Manufacturer notes")},
            {0x9286, new ExifTag(0x9286, "UserComment", "User comments")},
            {0x1B, new ExifTag(0x1B, "GPSProcessingMethod", "Name of GPS processing method")},
            {0x1C, new ExifTag(0x1C, "GPSAreaInformation", "Name of GPS area")}
        };
        
        static public int IdByName(string name)
        {
            foreach (var tag in _tags.Values)
            {
                if (tag.FieldName == name)
                    return tag.Id;
            }

            return -1;
        }

        static public ExifTag Tag(int id)
        {
            if (_tags.ContainsKey(id))
                return _tags[id];

            return null;
        }
    }
}
