using System;
using System.Collections.Generic;
using Proj4Net.Datum;
using Proj4Net.Projection;

namespace Proj4Net
{

    ///<summary>
    /// Supplies predefined values for various library classes such as 
    /// <see cref="Ellipsoid"/>, <see cref="Datum"/>, and <see cref="Projection"/>.
    ///</summary>
    public class Registry
    {

        public static readonly Datum.Datum[] Datums =
            {
                Datum.Datum.WGS84,
                Datum.Datum.GGRS87,
                Datum.Datum.NAD27,
                Datum.Datum.NAD83,
                Datum.Datum.Potsdam,
                Datum.Datum.Carthage,
                Datum.Datum.Hermannskogel,
                Datum.Datum.IRE65,
                //Datum.Datum.NZGD49,
                Datum.Datum.OSGB36
            };

        public Datum.Datum GetDatum(String code)
        {
            for (var i = 0; i < Datums.Length; i++)
            {
                if (Datums[i].Code.Equals(code))
                {
                    return Datums[i];
                }
            }
            return null;
        }
        /*
        public static readonly Ellipsoid[] Ellipsoids =
        {
            Ellipsoid.SPHERE,
            new Ellipsoid("MERIT", 6378137.0, 0.0, 298.257, "MERIT 1983"),
            new Ellipsoid("SGS85", 6378136.0, 0.0, 298.257, "Soviet Geodetic System 85"),
            Ellipsoid.Grs80,
            new Ellipsoid("IAU76", 6378140.0, 0.0, 298.257, "IAU 1976"),
            Ellipsoid.AIRY,
            Ellipsoid.ModAIRY,
            new Ellipsoid("APL4.9", 6378137.0, 0.0, 298.25, "Appl. Physics. 1965"),
            new Ellipsoid("NWL9D", 6378145.0, 298.25, 0.0, "Naval Weapons Lab., 1965"),
            new Ellipsoid("andrae", 6377104.43, 300.0, 0.0, "Andrae 1876 (Den., Iclnd.)"),
            new Ellipsoid("aust_SA", 6378160.0, 0.0, 298.25, "Australian Natl & S. Amer. 1969"),
            new Ellipsoid("GRS67", 6378160.0, 0.0, 298.2471674270, "GRS 67 (IUGG 1967)"),
            Ellipsoid.Bessel,
            new Ellipsoid("bess_nam", 6377483.865, 0.0, 299.1528128, "Bessel 1841 (Namibia)"),
            Ellipsoid.Clarke1866,
            Ellipsoid.Clarke1880,
            new Ellipsoid("CPM", 6375738.7, 0.0, 334.29, "Comm. des Poids et Mesures 1799"),
            new Ellipsoid("delmbr", 6376428.0, 0.0, 311.5, "Delambre 1810 (Belgium)"),
            new Ellipsoid("engelis", 6378136.05, 0.0, 298.2566, "Engelis 1985"),
            Ellipsoid.Everest,
            new Ellipsoid("evrst48", 6377304.063, 0.0, 300.8017, "Everest 1948"),
            new Ellipsoid("evrst56", 6377301.243, 0.0, 300.8017, "Everest 1956"),
            new Ellipsoid("evrst69", 6377295.664, 0.0, 300.8017, "Everest 1969"),
            new Ellipsoid("evrstSS", 6377298.556, 0.0, 300.8017, "Everest (Sabah & Sarawak)"),
            new Ellipsoid("fschr60", 6378166.0, 0.0, 298.3, "Fischer (Mercury Datum) 1960"),
            new Ellipsoid("fschr60m", 6378155.0, 0.0, 298.3, "Modified Fischer 1960"),
            new Ellipsoid("fschr68", 6378150.0, 0.0, 298.3, "Fischer 1968"),
            new Ellipsoid("helmert", 6378200.0, 0.0, 298.3, "Helmert 1906"),
            new Ellipsoid("hough", 6378270.0, 0.0, 297.0, "Hough"),
            Ellipsoid.International,
            Ellipsoid.International1967,
            Ellipsoid.Krasovsky,
            new Ellipsoid("kaula", 6378163.0, 0.0, 298.24, "Kaula 1961"),
            new Ellipsoid("lerch", 6378139.0, 0.0, 298.257, "Lerch 1979"),
            new Ellipsoid("mprts", 6397300.0, 0.0, 191.0, "Maupertius 1738"),
            new Ellipsoid("plessis", 6376523.0, 6355863.0, 0.0, "Plessis 1817 France)"),
            new Ellipsoid("SEasia", 6378155.0, 6356773.3205, 0.0, "Southeast Asia"),
            new Ellipsoid("walbeck", 6376896.0, 6355834.8467, 0.0, "Walbeck"),
            Ellipsoid.WGS60,
            Ellipsoid.WGS66,
            Ellipsoid.WGS72,
            Ellipsoid.WGS84,
            new Ellipsoid("NAD27", 6378249.145, 0.0, 293.4663, "NAD27: Clarke 1880 mod."),
            new Ellipsoid("NAD83", 6378137.0, 0.0, 298.257222101, "NAD83: GRS 1980 (IUGG, 1980)"),
        };
        */

        public Ellipsoid GetEllipsoid(String name)
        {
            for (int i = 0; i < Ellipsoid.Ellipsoids.Length; i++)
            {
                if (Ellipsoid.Ellipsoids[i].ShortName.Equals(name))
                {
                    return Ellipsoid.Ellipsoids[i];
                }
            }
            return null;
        }

        private 
#if !SILVERLIGHT
            SortedDictionary<String, Type> 
#else
            Dictionary<String, Type>
#endif
            _projRegistry;
         
        private void Register(String name, Type cls, String description)
        {
            _projRegistry.Add(name, cls);
        }

        public Projection.Projection GetProjection(String name, IDictionary<string, string> parameters)
        {
            Type cls;
            if (_projRegistry == null)
                Initialize();

            if (_projRegistry.TryGetValue(name, out cls))
            {
                try
                {
                    Projection.Projection projection = Activator.CreateInstance(cls) as Projection.Projection;//, parameters) as Projection.Projection;
                    if (projection != null)
                        projection.Name = name;
                    return projection;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                //catch (IllegalAccessException e)
                //{
                //    //e.printStackTrace();
                //}
                //catch (InstantiationException e)
                //{
                //    //e.printStackTrace();
                //}
            }
            return null;
        }

        private void Initialize()
        {

            if (_projRegistry != null) return;

            _projRegistry = new 
#if !SILVERLIGHT
                SortedDictionary<String, Type>();
#else
                Dictionary<String, Type>();
#endif
            Register("aea", typeof (AlbersProjection), "Albers Equal Area");
            Register("aeqd", typeof (EquidistantAzimuthalProjection), "Azimuthal Equidistant");
            Register("airy", typeof(AiryProjection), "Airy");
            Register("aitoff", typeof (AitoffProjection), "Aitoff");
            Register("alsk", typeof (Projection.Projection), "Mod. Stereographics of Alaska");
            Register("apian", typeof (Projection.Projection), "Apian Globular I");
            Register("august", typeof (AugustProjection), "August Epicycloidal");
            Register("bacon", typeof(Projection.Projection), "Bacon Globular");
            Register("bipc", typeof (BipolarProjection), "Bipolar conic of western hemisphere");
            Register("boggs", typeof (BoggsProjection), "Boggs Eumorphic");
            Register("bonne", typeof (BonneProjection), "Bonne (Werner lat_1=90)");
            Register("cass", typeof (CassiniProjection), "Cassini");
            Register("cc", typeof (CentralCylindricalProjection), "Central Cylindrical");
            Register("cea", typeof(Projection.Projection), "Equal Area Cylindrical");
            ////Register( "chamb", typeof(Projection), "Chamberlin Trimetric" );
            Register("collg", typeof (CollignonProjection), "Collignon");
            Register("crast", typeof (CrasterProjection), "Craster Parabolic (Putnins P4)");
            Register("denoy", typeof (DenoyerProjection), "Denoyer Semi-Elliptical");
            Register("eck1", typeof (Eckert1Projection), "Eckert I");
            Register("eck2", typeof (Eckert2Projection), "Eckert II");
            ////Register( "eck3", typeof(Eckert3Projection), "Eckert III" );
            Register("eck4", typeof (Eckert4Projection), "Eckert IV");
            Register("eck5", typeof (Eckert5Projection), "Eckert V");
            Register("eck6", typeof (Eckert6Projection), "Eckert VI" );
            Register("eqc", typeof (PlateCarreeProjection), "Equidistant Cylindrical (Plate Caree)");
            Register("eqdc", typeof (EquidistantConicProjection), "Equidistant Conic");
            Register("euler", typeof (EulerProjection), "Euler");
            Register("fahey", typeof (FaheyProjection), "Fahey");
            Register("fouc", typeof (FoucautProjection), "Foucaut");
            Register("fouc_s", typeof (FoucautSinusoidalProjection), "Foucaut Sinusoidal");
            Register("gall", typeof (GallProjection), "Gall (Gall Stereographic)");
            //Register( "gins8", typeof(Projection), "Ginsburg VIII (TsNIIGAiK)" );
            //Register( "gn_sinu", typeof(Projection), "General Sinusoidal Series" );
            Register("gnom", typeof (GnomonicAzimuthalProjection), "Gnomonic");
            Register("goode", typeof (GoodeProjection), "Goode Homolosine");
            //Register( "gs48", typeof(Projection), "Mod. Stererographics of 48 U.S." );
            //Register( "gs50", typeof(Projection), "Mod. Stererographics of 50 U.S." );
            Register("hammer", typeof (HammerProjection), "Hammer & Eckert-Greifendorff");
            Register("hatano", typeof (HatanoProjection), "Hatano Asymmetrical Equal Area");
            //Register( "imw_p", typeof(Projection), "Internation Map of the World Polyconic" );
            Register("kav5", typeof (KavraiskyVProjection), "Kavraisky V");
            //Register( "kav7", typeof(Projection), "Kavraisky VII" );
            //Register( "labrd",typeof( Projection), "Laborde" );
            Register( "laea", typeof(LambertAzimuthalEqualAreaProjection), "Lambert Azimuthal Equal Area" );
            Register("lagrng", typeof (LagrangeProjection), "Lagrange");
            Register("larr", typeof (LarriveeProjection), "Larrivee");
            Register("lask", typeof (LaskowskiProjection), "Laskowski");
            Register("latlong", typeof (LongLatProjection), "Lat/Long");
            Register("longlat", typeof (LongLatProjection), "Lat/Long");
            Register("lcc", typeof (LambertConformalConicProjection), "Lambert Conformal Conic");
            Register("leac", typeof (LambertEqualAreaConicProjection), "Lambert Equal Area Conic");
            ////Register( "lee_os", typeof(Projection), "Lee Oblated Stereographic" );
            Register("loxim", typeof (LoximuthalProjection), "Loximuthal");
            Register("lsat", typeof (LandsatProjection), "Space oblique for LANDSAT");
            ////Register( "mbt_s", typeof(Projection), "McBryde-Thomas Flat-Polar Sine" );
            Register("mbt_fps", typeof (McBrydeThomasFlatPolarSine2Projection), "McBryde-Thomas Flat-Pole Sine (No. 2)");
            Register("mbtfpp", typeof (McBrydeThomasFlatPolarParabolicProjection), "McBride-Thomas Flat-Polar Parabolic");
            Register("mbtfpq", typeof (McBrydeThomasFlatPolarQuarticProjection), "McBryde-Thomas Flat-Polar Quartic");
            ////Register( "mbtfps", typeof(Projection), "McBryde-Thomas Flat-Polar Sinusoidal" );
            Register("merc", typeof (MercatorProjection), "Mercator");
            ////Register( "mil_os", typeof(Projection), "Miller Oblated Stereographic" );
            Register("mill", typeof (MillerProjection), "Miller Cylindrical");
            ////Register( "mpoly", typeof(Projection), "Modified Polyconic" );
            Register("moll", typeof (MolleweideProjection), "Mollweide");
            Register("murd1", typeof (Murdoch1Projection), "Murdoch I");
            Register("murd2", typeof (Murdoch2Projection), "Murdoch II");
            Register("murd3", typeof (Murdoch3Projection), "Murdoch III");
            Register("nell", typeof (NellProjection), "Nell");
            ////Register( "nell_h", typeof(Projection), "Nell-Hammer" );
            Register("nicol", typeof (NicolosiProjection), "Nicolosi Globular");
            Register("nsper", typeof (PerspectiveProjection), "Near-sided perspective");
            ////Register( "nzmg", typeof(Projection), "New Zealand Map Grid" );
            ////Register( "ob_tran", typeof(Projection), "General Oblique Transformation" );
            ////Register( "ocea", typeof(Projection), "Oblique Cylindrical Equal Area" );
            ////Register( "oea", typeof(Projection), "Oblated Equal Area" );
            Register("omerc", typeof (ObliqueMercatorProjection), "Oblique Mercator");
            Register("sterea", typeof (ObliqueStereographicAlternativeProjection), "Oblique Stereographic Alternative");
            ////Register( "ortel", typeof(Projection), "Ortelius Oval" );
            //Register("ortho", typeof (OrthographicAzimuthalProjection), "Orthographic");
            Register("pconic", typeof (PerspectiveConicProjection), "Perspective Conic");
            Register("poly", typeof (PolyconicProjection), "Polyconic (American)");
            ////Register( "putp1",typeof( Projection), "Putnins P1" );
            Register("putp2", typeof (PutninsP2Projection), "Putnins P2");
            ////Register( "putp3", typeof(Projection), "Putnins P3" );
            ////Register( "putp3p", typeof(Projection), "Putnins P3'" );
            Register("putp4p", typeof (PutninsP4Projection), "Putnins P4'");
            Register("putp5", typeof (PutninsP5Projection), "Putnins P5");
            Register("putp5p", typeof (PutninsP5PProjection), "Putnins P5'");
            ////Register( "putp6", typeof(Projection), "Putnins P6" );
            ////Register( "putp6p", typeof(Projection), "Putnins P6'" );
            Register("qua_aut", typeof (QuarticAuthalicProjection), "Quartic Authalic");
            Register("robin", typeof (RobinsonProjection), "Robinson");
            Register("rpoly", typeof (RectangularPolyconicProjection), "Rectangular Polyconic");
            Register("sinu", typeof (SinusoidalProjection), "Sinusoidal (Sanson-Flamsteed)");
            Register("somerc", typeof(SwissObliqueMercatorProjection), "Swiss Oblique Mercator");
            Register("stere", typeof (StereographicAzimuthalProjection), "Stereographic");
            Register("tcc", typeof (TransverseCentralCylindricalProjection), "Transverse Central Cylindrical");
            Register("tcea", typeof (TransverseCylindricalEqualArea), "Transverse Cylindrical Equal Area");
            ////Register( "tissot", typeof(TissotProjection), "Tissot Conic" );
            Register("tmerc", typeof(TransverseMercatorProjection), "Transverse Mercator");
            ////Register( "tpeqd", typeof(Projection), "Two Point Equidistant" );
            ////Register( "tpers", typeof(Projection), "Tilted perspective" );
            ////Register( "ups", typeof(Projection), "Universal Polar Stereographic" );
            ////Register( "urm5", typeof(Projection), "Urmaev V" );
            Register("urmfps", typeof (UrmaevFlatPolarSinusoidalProjection), "Urmaev Flat-Polar Sinusoidal");
            Register("utm", typeof(TransverseMercatorProjection), "Universal Transverse Mercator (UTM)");
            Register("vandg", typeof (VanDerGrintenProjection), "van der Grinten (I)");
            ////Register( "vandg2", typeof(Projection), "van der Grinten II" );
            ////Register( "vandg3", typeof(Projection), "van der Grinten III" );
            ////Register( "vandg4", typeof(Projection), "van der Grinten IV" );
            Register("vitk1", typeof (VitkovskyProjection), "Vitkovsky I");
            Register("wag1", typeof (Wagner1Projection), "Wagner I (Kavraisky VI)");
            Register("wag2", typeof (Wagner2Projection), "Wagner II");
            Register("wag3", typeof (Wagner3Projection), "Wagner III");
            Register("wag4", typeof (Wagner4Projection), "Wagner IV");
            Register("wag5", typeof (Wagner5Projection), "Wagner V");
            ////Register( "wag6", typeof(Projection), "Wagner VI" );
            Register("wag7", typeof (Wagner7Projection), "Wagner VII");
            Register("weren", typeof (WerenskioldProjection), "Werenskiold I");
            ////Register( "wink1", typeof(Projection), "Winkel I" );
            ////Register( "wink2", typeof(Projection), "Winkel II" );
            Register("wintri", typeof (WinkelTripelProjection), "Winkel Tripel");
        }


    }
}