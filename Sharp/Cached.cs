using System;
using System.Collections.Generic;
using System.Web.Caching;
using System.Web;
using System.IO;

namespace Sharp
{
    public class Cached
    {
        /// <summary>Gets a file</summary>
        /// <param name="file">Virtual Path</param>
        /// <returns></returns>
        public static String GetFile(String file)
        {
            if (!file.Contains(":"))
                file = HttpContext.Current.Server.MapPath(file);

            if (HttpContext.Current.Cache["File: " + file] == null)
            {
                if (File.Exists(file))
                    HttpContext.Current.Cache.Add("File: " + file, File.ReadAllText(file), new System.Web.Caching.CacheDependency(file), DateTime.MaxValue, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.BelowNormal, null);
                else
                {
                    throw new FileNotFoundException("File Not Found:" + file);
                }
            }
            return (String)HttpContext.Current.Cache["File: " + file];
        }

        public delegate object Del<U>(U url);
        public static T CachedByDate<T, U>(string key, U arg, int days, Del<U> whattocache)
        {
            if (HttpContext.Current.Cache[key] == null)
            {
                HttpContext.Current.Cache.Add(key, whattocache(arg), null, DateTime.Now.AddDays(days), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.BelowNormal, null);
            }
            return (T)HttpContext.Current.Cache[key];
        }

        ///// <summary>
        ///// Gets a 2D array of states [abbreviation, statename]
        ///// </summary>
        ///// <returns></returns>
        //public static String[][] Get2DArray(String StoredProc)
        //        {
        //            HttpContext context = HttpContext.Current;
        //            if (context.Cache["SP: " + StoredProc] == null)
        //            {
        //                List<string[]> statelist = new List<string[]>();
        //                using (DB states = new DB(StoredProc))
        //                {
        //                    StateBag sb = new StateBag();
        //                    states.ExecuteReader(sb);
        //                    if (sb.Valid)
        //                    {
        //                        while (states.dr.Read())
        //                        {
        //                            if (states.dr.FieldCount > 1)
        //                                statelist.Add(new string[] { states.dr.GetString(0), states.dr.GetString(1) });
        //                            else
        //                                statelist.Add(new string[] { states.dr.GetString(0), states.dr.GetString(0) });
        //
        //                        }
        //                    }
        //                }
        //                string[][] jaggedarray = statelist.ToArray();
        //                context.Cache.Add("SP: " + StoredProc, jaggedarray, null, Cache.NoAbsoluteExpiration, new TimeSpan(7, 0, 0, 0, 0), CacheItemPriority.BelowNormal, null);
        //            }
        //            return (string[][])context.Cache["SP: " + StoredProc];
        //        }



    }





    public class Countries
    {
        public Countries() { }

        public static string[][] AllCountries
        {
            get
            {
                return new string[][] {   new string[] {"AFG", "AFGHANISTAN"},   new string[] {"ALB", "ALBANIA"},   new string[] {"DZA", "ALGERIA"},   new string[] {"ASM", "AMERICAN SAMOA"},   new string[] {"AND", "ANDORRA"},   new string[] {"AGO", "ANGOLA"},   new string[] {"AIA", "ANGUILLA"},   new string[] {"ATA", "ANTARCTICA"},   new string[] {"ATG", "ANTIGUA AND BARBUDA"},   new string[] {"ARG", "ARGENTINA"},   new string[] {"ARM", "ARMENIA"},   new string[] {"ABW", "ARUBA"},   new string[] {"AUS", "AUSTRALIA"},   new string[] {"AUT", "AUSTRIA"},   new string[] {"AZE", "AZERBAIJAN"},   new string[] {"BHS", "BAHAMAS"},   new string[] {"BHR", "BAHRAIN"},   new string[] {"BGD", "BANGLADESH"},   new string[] {"BRB", "BARBADOS"},   new string[] {"BLR", "BELARUS"},   new string[] {"BEL", "BELGIUM"},   new string[] {"BLZ", "BELIZE"},   new string[] {"BEN", "BENIN"},   new string[] {"BMU", "BERMUDA"},   new string[] {"BTN", "BHUTAN"},   new string[] {"BOL", "BOLIVIA"},   new string[] {"BIH", "BOSNIA AND HERZEGOWINA"},   new string[] {"BWA", "BOTSWANA"},   new string[] {"BVT", "BOUVET ISLAND"},   new string[] {"BRA", "BRAZIL"},   new string[] {"IOT", "BRITISH INDIAN OCEAN TERRITORY"},   new string[] {"BRN", "BRUNEI DARUSSALAM"},   new string[] {"BGR", "BULGARIA"},   new string[] {"BFA", "BURKINA FASO"},   new string[] {"BDI", "BURUNDI"},   new string[] {"KHM", "CAMBODIA"},   new string[] {"CMR", "CAMEROON"},   new string[] {"CAN", "CANADA"},   new string[] {"CPV", "CAPE VERDE"},   new string[] {"CYM", "CAYMAN ISLANDS"},   new string[] {"CAF", "CENTRAL AFRICAN REPUBLIC"},   new string[] {"TCD", "CHAD"},   new string[] {"CHL", "CHILE"},   new string[] {"CHN", "CHINA"},   new string[] {"CXR", "CHRISTMAS ISLAND"},   new string[] {"CCK", "COCOS (KEELING) ISLANDS"},   new string[] {"COL", "COLOMBIA"},   new string[] {"COM", "COMOROS"},   new string[] {"COG", "CONGO"},   new string[] {"COD", "CONGO, THE DRC"},   new string[] {"COK", "COOK ISLANDS"},   new string[] {"CRI", "COSTA RICA"},   new string[] {"CIV", "COTE D'IVOIRE"},   new string[] {"HRV", "CROATIA (local name: Hrvatska)"},   new string[] {"CUB", "CUBA"},   new string[] {"CYP", "CYPRUS"},   new string[] {"CZE", "CZECH REPUBLIC"},   new string[] {"DNK", "DENMARK"},   new string[] {"DJI", "DJIBOUTI"},   new string[] {"DMA", "DOMINICA"},   new string[] {"DOM", "DOMINICAN REPUBLIC"},   new string[] {"TMP", "EAST TIMOR"},   new string[] {"ECU", "ECUADOR"},   new string[] {"EGY", "EGYPT"},   new string[] {"SLV", "EL SALVADOR"},   new string[] {"GNQ", "EQUATORIAL GUINEA"},    new string[] {"ERI", "ERITREA"},   new string[] {"EST", "ESTONIA"},   new string[] {"ETH", "ETHIOPIA"},   new string[] {"FLK", "FALKLAND ISLANDS (MALVINAS)"},   new string[] {"FRO", "FAROE ISLANDS"},   new string[] {"FJI", "FIJI"},   new string[] {"FIN", "FINLAND"},   new string[] {"FRA", "FRANCE"},   new string[] {"FXX", "FRANCE, METROPOLITAN"},   new string[] {"GUF", "FRENCH GUIANA"},   new string[] {"PYF", "FRENCH POLYNESIA"},   new string[] {"ATF", "FRENCH SOUTHERN TERRITORIES"},   new string[] {"GAB", "GABON"},   new string[] {"GMB", "GAMBIA"},   new string[] {"GEO", "GEORGIA"},   new string[] {"DEU", "GERMANY"},   new string[] {"GHA", "GHANA"},   new string[] {"GIB", "GIBRALTAR"},   new string[] {"GRC", "GREECE"},   new string[] {"GRL", "GREENLAND"},   new string[] {"GRD", "GRENADA"},   new string[] {"GLP", "GUADELOUPE"},   new string[] {"GUM", "GUAM"},   new string[] {"GTM", "GUATEMALA"},   new string[] {"GIN", "GUINEA"},   new string[] {"GNB", "GUINEA-BISSAU"},   new string[] {"GUY", "GUYANA"},   new string[] {"HTI", "HAITI"},   new string[] {"HMD", "HEARD AND MC DONALD ISLANDS"},   new string[] {"VAT", "HOLY SEE (VATICAN CITY STATE)"},   new string[] {"HND", "HONDURAS"},   new string[] {"HKG", "HONG KONG"},   new string[] {"HUN", "HUNGARY"},   new string[] {"ISL", "ICELAND"},   new string[] {"IND", "INDIA"},   new string[] {"IDN", "INDONESIA"},   new string[] {"IRN", "IRAN"},   new string[] {"IRQ", "IRAQ"},   new string[] {"IRL", "IRELAND"},   new string[] {"ISR", "ISRAEL"},   new string[] {"ITA", "ITALY"},   new string[] {"JAM", "JAMAICA"},   new string[] {"JPN", "JAPAN"},   new string[] {"JOR", "JORDAN"},   new string[] {"KAZ", "KAZAKHSTAN"},   new string[] {"KEN", "KENYA"},   new string[] {"KIR", "KIRIBATI"},   new string[] {"PRK", "KOREA, D.P.R.O."},   new string[] {"KOR", "KOREA, REPUBLIC OF"},   new string[] {"KWT", "KUWAIT"},   new string[] {"KGZ", "KYRGYZSTAN"},   new string[] {"LAO", "LAOS "},   new string[] {"LVA", "LATVIA"},   new string[] {"LBN", "LEBANON"},   new string[] {"LSO", "LESOTHO"},   new string[] {"LBR", "LIBERIA"},   new string[] {"LBY", "LIBYAN ARAB JAMAHIRIYA"},   new string[] {"LIE", "LIECHTENSTEIN"},   new string[] {"LTU", "LITHUANIA"},   new string[] {"LUX", "LUXEMBOURG"},   new string[] {"MAC", "MACAU"},   new string[] {"MKD", "MACEDONIA"},   new string[] {"MDG", "MADAGASCAR"},   new string[] {"MWI", "MALAWI"},   new string[] {"MYS", "MALAYSIA"},   new string[] {"MDV", "MALDIVES"},   new string[] {"MLI", "MALI"},   new string[] {"MLT", "MALTA"},   new string[] {"MHL", "MARSHALL ISLANDS"},   new string[] {"MTQ", "MARTINIQUE"},   new string[] {"MRT", "MAURITANIA"},   new string[] {"MUS", "MAURITIUS"},   new string[] {"MYT", "MAYOTTE"},   new string[] {"MEX", "MEXICO"},   new string[] {"FSM", "MICRONESIA, FEDERATED STATES OF"},   new string[] {"MDA", "MOLDOVA, REPUBLIC OF"},   new string[] {"MCO", "MONACO"},   new string[] {"MNG", "MONGOLIA"},   new string[] {"MSR", "MONTSERRAT"},   new string[] {"MAR", "MOROCCO"},   new string[] {"MOZ", "MOZAMBIQUE"},   new string[] {"MMR", "MYANMAR (Burma) "},   new string[] {"NAM", "NAMIBIA"},   new string[] {"NRU", "NAURU"},   new string[] {"NPL", "NEPAL"},   new string[] {"NLD", "NETHERLANDS"},   new string[] {"ANT", "NETHERLANDS ANTILLES"},   new string[] {"NCL", "NEW CALEDONIA"},    new string[] {"NZL", "NEW ZEALAND"},   new string[] {"NIC", "NICARAGUA"},   new string[] {"NER", "NIGER"},   new string[] {"NGA", "NIGERIA"},   new string[] {"NIU", "NIUE"},   new string[] {"NFK", "NORFOLK ISLAND"},   new string[] {"MNP", "NORTHERN MARIANA ISLANDS"},   new string[] {"NOR", "NORWAY"},   new string[] {"OMN", "OMAN"},   new string[] {"PAK", "PAKISTAN"},   new string[] {"PLW", "PALAU"},   new string[] {"PAN", "PANAMA"},   new string[] {"PNG", "PAPUA NEW GUINEA"},   new string[] {"PRY", "PARAGUAY"},   new string[] {"PER", "PERU"},   new string[] {"PHL", "PHILIPPINES"},   new string[] {"PCN", "PITCAIRN"},   new string[] {"POL", "POLAND"},   new string[] {"PRT", "PORTUGAL"},   new string[] {"PRI", "PUERTO RICO"},   new string[] {"QAT", "QATAR"},   new string[] {"REU", "REUNION"},   new string[] {"ROM", "ROMANIA"},   new string[] {"RUS", "RUSSIAN FEDERATION"},   new string[] {"RWA", "RWANDA" },   new string[] {"LCA", "SAINT LUCIA"},   new string[] {"VCT", "SAINT VINCENT AND THE GRENADINES"},   new string[] {"WSM", "SAMOA"},   new string[] {"SMR", "SAN MARINO"},   new string[] {"STP", "SAO TOME AND PRINCIPE"},   new string[] {"SAU", "SAUDI ARABIA"},   new string[] {"SEN", "SENEGAL"},   new string[] {"SYC", "SEYCHELLES"},   new string[] {"SLE", "SIERRA LEONE"},   new string[] {"SGP", "SINGAPORE"},   new string[] {"SVK", "SLOVAKIA (Slovak Republic)"},   new string[] {"SVN", "SLOVENIA"},   new string[] {"SLB", "SOLOMON ISLANDS"},   new string[] {"SOM", "SOMALIA"},   new string[] {"ZAF", "SOUTH AFRICA"},   new string[] {"SGS", "SOUTH GEORGIA AND SOUTH S.S."},   new string[] {"ESP", "SPAIN"},   new string[] {"LKA", "SRI LANKA"},   new string[] {"SHN", "ST. HELENA"},   new string[] {"SPM", "ST. PIERRE AND MIQUELON"},   new string[] {"SDN", "SUDAN"},   new string[] {"SUR", "SURINAME"},   new string[] {"SJM", "SVALBARD AND JAN MAYEN ISLANDS"},   new string[] {"SWZ", "SWAZILAND"},   new string[] {"SWE", "SWEDEN"},   new string[] {"CHE", "SWITZERLAND"},   new string[] {"SYR", "SYRIAN ARAB REPUBLIC"},   new string[] {"TWN", "TAIWAN, PROVINCE OF CHINA"},   new string[] {"TJK", "TAJIKISTAN"},   new string[] {"TZA", "TANZANIA, UNITED REPUBLIC OF"},   new string[] {"THA", "THAILAND"},   new string[] {"TGO", "TOGO"},   new string[] {"TKL", "TOKELAU"},   new string[] {"TON", "TONGA"},   new string[] {"TTO", "TRINIDAD AND TOBAGO"},   new string[] {"TUN", "TUNISIA"},   new string[] {"TUR", "TURKEY"},   new string[] {"TKM", "TURKMENISTAN"},   new string[] {"TCA", "TURKS AND CAICOS ISLANDS"},   new string[] {"TUV", "TUVALU"},   new string[] {"UGA", "UGANDA"},   new string[] {"UKR", "UKRAINE"},   new string[] {"ARE", "UNITED ARAB EMIRATES"},   new string[] {"GBR", "UNITED KINGDOM"},   new string[] {"USA", "UNITED STATES"},   new string[] {"UMI", "U.S. MINOR ISLANDS"},   new string[] {"URY", "URUGUAY"},   new string[] {"UZB", "UZBEKISTAN"},   new string[] {"VUT", "VANUATU"},   new string[] {"VEN", "VENEZUELA"},   new string[] {"VNM", "VIET NAM"},   new string[] {"VGB", "VIRGIN ISLANDS (BRITISH)"},   new string[] {"VIR", "VIRGIN ISLANDS (U.S.)"},   new string[] {"WLF", "WALLIS AND FUTUNA ISLANDS"},   new string[] {"ESH", "WESTERN SAHARA"},   new string[] {"YEM", "YEMEN"},   new string[] {"YUG", "Yugoslavia (Serbia and Montenegro)"},   new string[] {"ZMB", "ZAMBIA"},   new string[] {"ZWE", "ZIMBABWE"}
                };
            }
        }

    }
}