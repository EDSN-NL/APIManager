using System;
using Framework.Logging;

namespace Framework.Util
{
    /// <summary>
    /// Cardinality represents the cardinality of an association between a source- and target class.
    /// It consists of a lower- and an upper boun dary in which an upper boundary of 0 must be interpreted as 'unlimited'.
    /// </summary>
    internal sealed class Cardinality: IEquatable<Cardinality>
    {
        private int _lowerBoundary;
        private int _upperBoundary;

        /// <summary>
        /// Returns the lower range of the cardinality.
        /// </summary>
        internal int LowerBoundary { get { return this._lowerBoundary; } }

        /// <summary>
        /// Returns the upper range of the cardinality. Returns '0' for 'unlimited'.
        /// </summary>
        internal int UpperBoundary { get { return this._upperBoundary; } }

        /// <summary>
        /// Returns the cardinality as a tuple of two integers, first value is lower range, second is upper range.
        /// A value of '0' must be interpreted as 'unlimited' (only for upper range).
        /// </summary>
        internal Tuple<int,int> CardTuple { get { return new Tuple<int, int>(this._lowerBoundary, this._upperBoundary); } }

        /// <summary>
        /// Returns 'true' in case of a cardinality with a lower boundary > 0.
        /// </summary>
        internal bool IsMandatory { get { return this._lowerBoundary > 0; } }

        /// <summary>
        /// Returns 'true' in case of a cardinality with a lower boundary = 0.
        /// </summary>
        internal bool IsOptional { get { return this._lowerBoundary == 0; } }

        /// <summary>
        /// Returns true in case the Cardinality represents a list (i.e. upper boundary > 1). 
        /// </summary>
        internal bool IsList { get { return this._upperBoundary == 0 || this._upperBoundary > 1; } }

        /// <summary>
        /// Override method that compares a Cardinality with an Object. Returns true if both objects are of 
        /// identical type and ave identical attribute values.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object type and equal attribute values, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objCard = obj as Cardinality;
            return (objCard != null) && Equals(objCard);
        }

        /// <summary>
        /// Compares two Cardinality objects for equality. Both are considered equal if both lower- and upper 
        /// boundaries are equal.
        /// </summary>
        /// <param name="other">Cardinality object to compare against.</param>
        /// <returns>True if equal Cardinalities, false otherwise.</returns>
        public bool Equals(Cardinality other)
        {
            return this._lowerBoundary == other._lowerBoundary && this._upperBoundary == other._upperBoundary;
        }

        /// <summary>
        /// Determine a hash of the Cardinality. In an attempt to return hashes that are actually unique
        /// across a wide range of cardinality objects, we treat upper- and lower boundaries as 16-bit numbers
        /// (half of the actual size of an integer, which will be accurate for most cardinalities). 
        /// An unlimited upper boundary is considered to be 0xFFFF. 
        /// The hash is now calculated by shifting the upper boundary 'half an integer' in bit-size to the left,
        /// adding the lower boundary and then returning the hash of the resulting integer.
        /// This guarantees that cardinalities n..m and m..n return different values as long as m and n are different.
        /// </summary>
        /// <returns>Hash of Cardinality</returns>
        public override int GetHashCode()
        {
            int seed = this._upperBoundary == 0 ? 0xFFFF: this._upperBoundary;
            seed = (seed << (sizeof(int) / 2) * 8) + this._lowerBoundary;
            return seed.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two Cardinality objects are equal if both are referencing the same object,
        /// are not null and have identical lower- and upper boundaries.
        /// </summary>
        /// <param name="elementa">First ModelElement to compare.</param>
        /// <param name="elementb">Second ModelElement to compare.</param>
        /// <returns>True if both elements share the same implementation object (or neither has an implementation object),
        /// false otherwise.</returns>
        public static bool operator ==(Cardinality elementa, Cardinality elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;

            // We have two different objects, now check whether attributes are equal....
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two Cardinality objects are different when either lower- or upper boundaries are different.
        /// </summary>
        /// <param name="elementa">First Cardinality object to compare.</param>
        /// <param name="elementb">Second Cardinality object to compare.</param>
        /// <returns>True if both elements have different implementation objects, (or one is missing an implementation 
        /// object), false otherwise.</returns>
        public static bool operator !=(Cardinality elementa, Cardinality elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// Override method, returns a string representation of the Cardinality object.
        /// </summary>
        /// <returns>String representation formatted as lower..upper</returns>
        public override string ToString()
        {
            string cardStr = this._lowerBoundary.ToString();
            if (this._upperBoundary == 0 || this._lowerBoundary != this._upperBoundary)
                cardStr += ".." + (this._upperBoundary == 0? "*": this._upperBoundary.ToString());
            return cardStr;
        }

        /// <summary>
        /// Standard constructor, specifying both the lower- and upper boundaries. Any lower boundary value less then
        /// 0 will be interpreted as 0.
        /// </summary>
        /// <param name="lower">Lower boundary.</param>
        /// <param name="upper">Upper boundary, specify 0 for 'unlimited'.</param>
        /// <exception cref="ArgumentException">Thrown in case of illegal cardinality format.</exception>
        internal Cardinality(int lower, int upper)
        {
            this._lowerBoundary = lower < 0 ? 0 : lower;
            if (upper != 0 && upper < this._lowerBoundary)
            {
                string msg = "Framework.Util.Cardinality >> Illegal cardinality '" + lower + ".." + upper + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            this._upperBoundary = upper;
        }

        /// <summary>
        /// Standard constructor, specifying both the lower- and upper boundaries as a Tuple of two integers.
        /// </summary>
        /// <param name="range">Upper- and lower boundary.</param>
        /// <exception cref="ArgumentException">Thrown in case of illegal cardinality format.</exception>
        internal Cardinality(Tuple<int, int> range) : this(range.Item1, range.Item2) { }

        /// <summary>
        /// Creates a cardinality of either 0-1 or 1-1 (1-1 in case 'isMandatory' is true).
        /// The constructor can also be used as a default constructor, which results in a card of 0-1.
        /// </summary>
        /// <param name="isMandatory">When true, the lower range is set to '1'.</param>
        internal Cardinality(bool isMandatory = false)
        {
            this._lowerBoundary = isMandatory ? 1 : 0;
            this._upperBoundary = 1;
        }

        /// <summary>
        /// Builds an integer representation of a cardinality string. In theory, this string can contain literally anything. In our particular 
        /// case, we support:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to 
        ///   minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is interpreted as 'unbounded').
        /// All other formats will result in an Argument Exception.
        /// </summary>
        /// <param name="range">Contains the cardinality string.</param>
        internal Cardinality(string range)
        {
            try
            {
                // In case of empty or invalid cardinality, return illegal tuple...
                if (string.IsNullOrEmpty(range))
                {
                    string msg = "Framework.Util.Cardinality >> Empty cardinality string!";
                    Logger.WriteError(msg);
                    throw new ArgumentException(msg);
                }

                if (range.Contains(".."))
                {
                    // Different lower- and upper boundaries...
                    string lowerBound = range.Substring(0, range.IndexOf('.'));
                    string upperBound = range.Substring(range.LastIndexOf('.') + 1);
                    this._lowerBoundary = Convert.ToInt16(lowerBound);
                    if (!upperBound.Contains("*"))
                    {
                        // If we have a 0..0 cardinality, this is translated to 'optional 0 or 1'.
                        // And if maxOcc < minOcc (and not unbounded), we raise an exception.
                        this._upperBoundary = Convert.ToInt16(upperBound);
                        if ((this._upperBoundary > 0) && (this._upperBoundary < this._lowerBoundary))
                        {
                            string msg = "Framework.Util.Cardinality >> Unsupported format in cardinality '" + range + "'!";
                            Logger.WriteError(msg);
                            throw new ArgumentException(msg);
                        }
                        if (this._upperBoundary == 0) this._upperBoundary = 1;
                    }
                    else this._upperBoundary = 0;   // Treated as 'unlimited'.
                }
                else
                {
                    // Upper- and lower boundaries are equal...
                    if (range.Trim() == "*")
                    {
                        // A single '*' character is interpreted as: 0 to unbounded, which translates to an upper boundary of 0.
                        this._lowerBoundary = 0;
                        this._upperBoundary = 0;
                    }
                    else
                    {
                        // A single character is translated to 'exactly n', with the exception of '0', which is translated to 'optional 0 or 1'.
                        this._lowerBoundary = Convert.ToInt16(range);
                        this._upperBoundary = (this._lowerBoundary == 0) ? 1 : this._lowerBoundary;
                    }
                }
            }
            catch (FormatException)
            {
                string msg = "Framework.Util.Cardinality >> Unsupported format in cardinality '" + range + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
        }
    }
}
