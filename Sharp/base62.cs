using System;
using System.Text;

namespace Sharp
{
 
    public class InvalidrBase62ValueException : System.Exception
    {
        public InvalidrBase62ValueException(object o)
        {

        }
    }
    public class InvalidrBase62DigitException : System.Exception
    {
        public InvalidrBase62DigitException(object o)
        {

        }
    }
    public class InvalidrBase62NumberException : System.Exception
    {
        public InvalidrBase62NumberException(object o)
        {

        }
    }
    public class InvalidrBase62DigitValueException : System.Exception
    {
        public InvalidrBase62DigitValueException(object o)
        {

        }
    }

    /// <summary>
    /// Class representing a rBase62 number
    /// </summary>
    public struct rBase62
    {
        #region Constants (and pseudo-constants)

        /// <summary>
        /// rBase62 containing the maximum supported value for this type
        /// </summary>
        public static readonly rBase62 MaxValue = new rBase62(long.MaxValue);
        /// <summary>
        /// rBase62 containing the minimum supported value for this type
        /// </summary>
        public static readonly rBase62 MinValue = new rBase62(long.MinValue + 1);

        //public static readonly String Alphabet = "2NOI6BQZRX4CEGSKFPYTW73VDUHJM9A";
        public static readonly String Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static string alphabet;

        #endregion

        #region Fields

        private long numericValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a rBase62 number from a long value
        /// </summary>
        /// <param name="NumericValue">The long value to give to the rBase62 number</param>
        public rBase62(long NumericValue)
        {
            numericValue = 0; //required by the struct.
            this.NumericValue = NumericValue;
        }


        /// <summary>
        /// Instantiate a rBase62 number from a rBase62 string
        /// </summary>
        /// <param name="Value">The value to give to the rBase62 number</param>
        public rBase62(string Value)
        {
            numericValue = 0; //required by the struct.
            this.Value = Value;
        }


        #endregion

        #region Properties

        /// <summary>
        /// Get or set the value of the type using a base-10 long integer
        /// </summary>
        public long NumericValue
        {
            get
            {
                return numericValue;
            }
            set
            {
                //Make sure value is between allowed ranges
                if (value <= long.MinValue || value > long.MaxValue)
                {
                    throw new InvalidrBase62ValueException(value);
                }

                numericValue = value;
            }
        }


        /// <summary>
        /// Get or set the value of the type using a rBase62 string
        /// </summary>
        public string Value
        {
            get
            {
                return rBase62.NumberTorBase62(numericValue);
            }
            set
            {
                try
                {
                    numericValue = rBase62.rBase62ToNumber(value);
                }
                catch
                {
                    //Catch potential errors
                    throw new InvalidrBase62NumberException(value);
                }
            }
        }


        #endregion

        #region Public Static Methods

        /// <summary>
        /// Static method to convert a rBase62 string to a long integer (base-10)
        /// </summary>
        /// <param name="rBase62Value">The number to convert from</param>
        /// <returns>The long integer</returns>
        public static long rBase62ToNumber(string rBase62Value)
        {
            //Make sure we have passed something
            if (rBase62Value == "")
            {
                throw new InvalidrBase62NumberException(rBase62Value);
            }

            //Account for negative values:
            bool isNegative = false;

            if (rBase62Value[0] == '-')
            {
                rBase62Value = rBase62Value.Substring(1);
                isNegative = true;
            }

            //Loop through our string and calculate its value
            try
            {
                //Keep a running total of the value 
                long returnValue = 0;
                alphabet = Alphabet;
                long salt = rBase62DigitToNumber(rBase62Value[rBase62Value.Length - 1]);
                SaltAlphabet(salt);

                //Loop through the character in the string (right to left) and add
                //up increasing powers as we go.
                for (int i = 1; i < rBase62Value.Length; i++)
                {
                    returnValue += ((long)Math.Pow(62, i - 1) * rBase62DigitToNumber(rBase62Value[rBase62Value.Length - (i + 1)]));
                }

                //Do negative correction if required:
                if (isNegative)
                {
                    return returnValue * -1;
                }
                else
                {
                    return returnValue;
                }
            }
            catch
            {
                //If something goes wrong, this is not a valid number
                throw new InvalidrBase62NumberException(rBase62Value);
            }
        }  

        /// <summary>
        /// Public static method to convert a long integer (base-10) to a rBase62 number
        /// </summary>
        /// <param name="NumericValue">The base-10 long integer</param>
        /// <returns>A rBase62 representation</returns>
        public static string NumberTorBase62(long NumericValue)
        {
            alphabet = Alphabet;
            string salt = PositiveNumberTorBase62(NumericValue % 62);
            SaltAlphabet(NumericValue % 62);

            return DoNumberTorBase62(NumericValue) + salt;
        }

        private static string DoNumberTorBase62(long NumericValue)
        {
            try
            {
                //Handle negative values:
                if (NumericValue < 0)
                {
                    return string.Concat("-", PositiveNumberTorBase62(Math.Abs(NumericValue)));
                }
                else
                {
                    return PositiveNumberTorBase62(NumericValue);
                }
            }
            catch
            {
                throw new InvalidrBase62ValueException(NumericValue);
            }
        }


        #endregion

        #region Private Static Methods

        private static void SaltAlphabet(long salt)
        {
            if (salt > 0 && salt < 62)
                alphabet = Alphabet.Substring((int)salt) + Alphabet.Substring(0, (int)salt);
            else
                alphabet = Alphabet;
        }

        private static string PositiveNumberTorBase62(long NumericValue)
        {
            //This is a clever recursively called function that builds
            //the base-62 string representation of the long base-10 value
            if (NumericValue < 62)
            {
                //The get out clause; fires when we reach a number less than 
                //62 - this means we can add the last digit.
                return NumberTorBase62Digit((byte)NumericValue).ToString();
            }
            else
            {
                //Add digits from left to right in powers of 62 
                //(recursive)
                return string.Concat(PositiveNumberTorBase62(NumericValue / 62), NumberTorBase62Digit((byte)(NumericValue % 62)).ToString());
            }
        }


        private static byte rBase62DigitToNumber(char rBase62Digit)
        {
            return (byte)alphabet.IndexOf(rBase62Digit.ToString());
        }

        private static char NumberTorBase62Digit(byte NumericValue)
        {
            return alphabet[NumericValue];
        }


        #endregion

        #region Operator Overloads

        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator >(rBase62 LHS, rBase62 RHS)
        {
            return LHS.numericValue > RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator <(rBase62 LHS, rBase62 RHS)
        {
            return LHS.numericValue < RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator >=(rBase62 LHS, rBase62 RHS)
        {
            return LHS.numericValue >= RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator <=(rBase62 LHS, rBase62 RHS)
        {
            return LHS.numericValue <= RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator ==(rBase62 LHS, rBase62 RHS)
        {
            return LHS.numericValue == RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator !=(rBase62 LHS, rBase62 RHS)
        {
            return !(LHS == RHS);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static rBase62 operator +(rBase62 LHS, rBase62 RHS)
        {
            return new rBase62(LHS.numericValue + RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static rBase62 operator -(rBase62 LHS, rBase62 RHS)
        {
            return new rBase62(LHS.numericValue - RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static rBase62 operator ++(rBase62 Value)
        {
            return new rBase62(Value.numericValue++);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static rBase62 operator --(rBase62 Value)
        {
            return new rBase62(Value.numericValue--);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static rBase62 operator *(rBase62 LHS, rBase62 RHS)
        {
            return new rBase62(LHS.numericValue * RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static rBase62 operator /(rBase62 LHS, rBase62 RHS)
        {
            return new rBase62(LHS.numericValue / RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static rBase62 operator %(rBase62 LHS, rBase62 RHS)
        {
            return new rBase62(LHS.numericValue % RHS.numericValue);
        }


        /// <summary>
        /// Converts type rBase62 to a base-10 long
        /// </summary>
        /// <param name="Value">The rBase62 object</param>
        /// <returns>The base-10 long integer</returns>
        public static implicit operator long(rBase62 Value)
        {
            return Value.numericValue;
        }


        /// <summary>
        /// Converts type rBase62 to a base-10 integer
        /// </summary>
        /// <param name="Value">The rBase62 object</param>
        /// <returns>The base-10 integer</returns>
        public static implicit operator int(rBase62 Value)
        {
            try
            {
                return (int)Value.numericValue;
            }
            catch
            {
                throw new OverflowException("Overflow: Value too large to return as an integer");
            }
        }


        /// <summary>
        /// Converts type rBase62 to a base-10 short
        /// </summary>
        /// <param name="Value">The rBase62 object</param>
        /// <returns>The base-10 short</returns>
        public static implicit operator short(rBase62 Value)
        {
            try
            {
                return (short)Value.numericValue;
            }
            catch
            {
                throw new OverflowException("Overflow: Value too large to return as a short");
            }
        }


        /// <summary>
        /// Converts a long (base-10) to a rBase62 type
        /// </summary>
        /// <param name="Value">The long to convert</param>
        /// <returns>The rBase62 object</returns>
        public static implicit operator rBase62(long Value)
        {
            return new rBase62(Value);
        }


        /// <summary>
        /// Converts type rBase62 to a string; must be explicit, since
        /// rBase62 > string is dangerous!
        /// </summary>
        /// <param name="Value">The rBase62 type</param>
        /// <returns>The string representation</returns>
        public static explicit operator string(rBase62 Value)
        {
            return Value.Value;
        }


        /// <summary>
        /// Converts a string to a rBase62
        /// </summary>
        /// <param name="Value">The string (must be a rBase62 string)</param>
        /// <returns>A rBase62 type</returns>
        public static implicit operator rBase62(string Value)
        {
            return new rBase62(Value);
        }


        #endregion

        #region Public Override Methods

        /// <summary>
        /// Returns a string representation of the rBase62 number
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString()
        {
            return rBase62.NumberTorBase62(numericValue);
        }


        /// <summary>
        /// A unique value representing the value of the number
        /// </summary>
        /// <returns>The unique number</returns>
        public override int GetHashCode()
        {
            return numericValue.GetHashCode();
        }


        /// <summary>
        /// Determines if an object has the same value as the instance
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the values are the same</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is rBase62))
            {
                return false;
            }
            else
            {
                return this == (rBase62)obj;
            }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation padding the leading edge with
        /// zeros if necessary to make up the number of characters
        /// </summary>
        /// <param name="MinimumDigits">The minimum number of digits that the string must contain</param>
        /// <returns>The padded string representation</returns>
        public string ToString(int MinimumDigits)
        {
            string rBase62Value = rBase62.NumberTorBase62(numericValue);

            if (rBase62Value.Length >= MinimumDigits)
                return rBase62Value;
            else
                return rBase62Value.PadLeft(MinimumDigits, '0');
        }


        #endregion

    }
}