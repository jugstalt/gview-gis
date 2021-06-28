using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.GDAL
{
    public enum ColorInterp
    {
        Undefined = 0,
        GrayIndex = 1,
        PaletteIndex = 2,
        RedBand = 3,
        GreenBand = 4,
        BlueBand = 5,
        AlphaBand = 6,
        HueBand = 7,
        SaturationBand = 8,
        LightnessBand = 9,
        CyanBand = 10,
        MagentaBand = 11,
        YellowBand = 12,
        BlackBand = 13,
        YCbCr_YBand = 14,
        YCbCr_CbBand = 15,
        YCbCr_CrBand = 16,
        Max = 16
    }

    public enum GDALDataType
    {
        /*! Eight bit unsigned integer */
        GDT_Byte = 1,
        /*! Sixteen bit unsigned integer */
        GDT_UInt16 = 2,
        /*! Sixteen bit signed integer */
        GDT_Int16 = 3,
        /*! Thirty two bit unsigned integer */
        GDT_UInt32 = 4,
        /*! Thirty two bit signed integer */
        GDT_Int32 = 5,
        /*! Thirty two bit floating point */
        GDT_Float32 = 6,
        /*! Sixty four bit floating point */
        GDT_Float64 = 7,
        /*! Complex Int16 */
        GDT_CInt16 = 8,
        /*! Complex Int32 */
        GDT_CInt32 = 9,
        /*! Complex Float32 */
        GDT_CFloat32 = 10,
        /*! Complex Float64 */
        GDT_CFloat64 = 11
    }
}
