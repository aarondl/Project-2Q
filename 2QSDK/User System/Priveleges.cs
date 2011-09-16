using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Project2Q.SDK.UserSystem {

    #region Custom Attributes

    /// <summary>
    /// Tags a method with a required set of priveleges for execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PrivelegeRequiredAttribute : Attribute {
        private Priveleges requiredPrivelege;
        private string errormessage;

        /// <summary>
        /// Gets the required priveleges.
        /// </summary>
        public Priveleges Required {
            get { return requiredPrivelege; }
        }
        /// <summary>
        /// Gets the error message to display if privelege check fails.
        /// </summary>
        public string ErrorMessage {
            get { return errormessage; }
        }

        public PrivelegeRequiredAttribute(Priveleges required)
            : this(required, 
            (char)2 + "Error:" + (char)2 + " You require the priveleges [" + PrivelegeContainer.GetPrivelegeString((ulong)required) + "] to use that command." ) {
        }

        public PrivelegeRequiredAttribute(Priveleges required, string errormsg) {
            requiredPrivelege = required;
            errormessage = errormsg;
        }
    }

    /// <summary>
    /// Tags a method with a required user level for execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class UserLevelRequiredAttribute : Attribute {
        private uint userLevel;
        private string errormessage;

        /// <summary>
        /// Gets the required userlevel.
        /// </summary>
        public uint Required {
            get { return userLevel; }
        }
        /// <summary>
        /// Gets the error message to display if privelege check fails.
        /// </summary>
        public string ErrorMessage {
            get { return errormessage; }
        }

        public UserLevelRequiredAttribute(uint required)
        : this(required, 
            (char)2 + "Error:" + (char)2 + " You require the access level [" + required.ToString() + "] to use that command." ) {
        }

        public UserLevelRequiredAttribute(uint required, string errormsg) {
            userLevel = required;
            this.errormessage = errormsg;
        }
    }

    #endregion

    /// <summary>
    /// An enumeration depicting different hardcoded priveleges.
    /// </summary>
    [Flags]
    [Serializable]
    public enum Priveleges : ulong {

        /* For each item in this collection, a letter must be defined with it.
         * 
         * It will take the offset of the letter.
         * Letter: a  b  c  d  e  f  g  h  i  j  k  l  m  n  o  p  q  r  s  t  u  v  w  x  y  z
         * 2^x x=: 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57
         * Letter: A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z
         * 2^x x=: 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25
         * In Ascii:
         * A = 65 (0x41), Z = 90 (0x5A)
         * a = 97 (0x61), z = 122(0x7A)
         * In our system with bitmasks we make it so:
         * None = 0x0, A = 0x1 (2^0), B = 0x2 (2^1), C = 0x4 (2^2) ...
         * A = 0x1 (2^0), Z = 0x2000000 (2^25)
         * a = 0x100000000 (2^32), z = 0x200000000000000 (2^57)
         * 
         * And therefore if we were to make "SuperUser" privelege the 's' character.
         * s = 50 from our table, so we have to make it the 50th bit. (we're keeping flags in a 64-bit integer)
         * 
         * 51st bit is 2^50 or 0x4000000000000 in hex notation.
         * 
         * Therefore we can write:
         * SuperUser = 0x4000000000000,
         * 
         * If we were going to make J equal to "JumpingUser".
         * J = 9th bit. 2^9 = 512 = 0x200
         * So we can write: JumpingUser = 0x200,
         * 
         * Then setting and retrieving modes is quite easy:
         * 
         * numerical = (ulong)0x1 << ( (int)modechar-(int)'A' )
         */

        /// <summary>
        /// No flags set means no priveleges.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Access to in-channel commands. Mode char C = 2^2
        /// </summary>
        CommandAccess = 0x4,
        /// <summary>
        /// All Priveleges. Mode char s = 2^50
        /// </summary>
        SuperUser = 0x4000000000000,
        /// <summary>
        /// Access to remote console. Mode char r = 2^49
        /// </summary>
        RemoteConsole = 0x2000000000000,
        /// <summary>
        /// Access to the user system. Mode char U = 2^20
        /// </summary>
        UserSystem = 0x100000,
    }

    /// <summary>
    /// Describes and contains privelege levels for a user.
    /// </summary>
    [Serializable]
    public class PrivelegeContainer {

        #region Variables + Properties

        private uint numericalLevel;
        private Priveleges userFlags;

        /// <summary>
        /// A numerical level of access.
        /// </summary>
        public uint NumericalLevel {
            get { return numericalLevel; }
            set { numericalLevel = value; }
        }

        #endregion

        #region Constructor

        public PrivelegeContainer() {
            numericalLevel = 0;
            userFlags = 0;
        }

        #endregion

        #region Privelege System

        /// <summary>
        /// Does bit math to determine which bit flag corresponds
        /// to the inputted character in the privelege system.
        /// </summary>
        /// <param name="modeChar">The mode character.</param>
        /// <returns>The bit mask corresponding to the modechar.</returns>
        public static ulong GetModeMask(char modeChar) {
            //Special case:
            if ( modeChar == '*' )
                return 0;
            return (ulong)0x1 << ( (int)modeChar - (int)'A' );
        }

        /// <summary>
        /// Does bit math to determine which character corresponds
        /// to the inputted mask in the privelege system.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <returns>The mode character.</returns>
        public static char GetModeChar(ulong mask) {
            //Special case:
            if ( mask == 0 )
                return '*';
            int result = (int)Math.Round(Math.Log( (double)mask, 2.0 ));
            return (char)( result + (int)'A' );
        }

        #region Checking Priveleges

        /// <summary>
        /// Does the current instance of priveleges have the queried for priveleges?
        /// </summary>
        /// <param name="query">The priveleges to check.</param>
        /// <returns>True or false.</returns>
        public bool HasPrivelege(Priveleges query) {
            return ((userFlags & query) > 0);
        }

        /// <summary>
        /// Does the current instance of priveleges have
        /// </summary>
        /// <param name="query">The priveleges to check.</param>
        /// <returns>True or false.</returns>
        public bool HasPrivelege(ulong query) {
            return ( ( (ulong)userFlags & query ) > 0 );
        }

        /// <summary>
        /// Does the current instance of priveleges have
        /// </summary>
        /// <param name="query">The priveleges to check.</param>
        /// <returns>True or false.</returns>
        public bool HasPrivelege(char query) {
            return ( ( (ulong)userFlags & GetModeMask(query) ) > 0 );
        }

        /// <summary>
        /// Gets the privelege string for the current privelege set.
        /// </summary>
        public string PrivelegeString {
            get { return GetPrivelegeString((ulong)this.userFlags); }
        }

        /// <summary>
        /// Returns a string with all priveleges found inside it.
        /// </summary>
        /// <returns>A string with all priveleges found inside it.</returns>
        public static string GetPrivelegeString(ulong usermask) {

            int stop = sizeof( ulong ) * 8;

            ulong mask = 0x1;
            ulong isomask = 0;

            StringBuilder sb = new StringBuilder();

            while ( stop >= 0 ) {
                //Isolate the bit:
                isomask = mask & usermask;
                if ( isomask > 0 )
                    sb.Append( GetModeChar( isomask ) );

                mask <<= 1;
                stop--;
            }

            return sb.ToString();

        }

        #endregion

        #region Adding Priveleges

        /// <summary>
        /// Adds a privelege.
        /// </summary>
        /// <param name="add">The priveleges to add.</param>
        public void AddPriveleges(Priveleges add) {
            this.userFlags |= add;
        }

        /// <summary>
        /// Adds a privelege.
        /// </summary>
        /// <param name="add">The priveleges to add.</param>
        public void AddPriveleges(ulong add) {
            this.userFlags |= (Priveleges)add;
        }

        /// <summary>
        /// Adds a privelege.
        /// </summary>
        /// <param name="add">The priveleges to add.</param>
        public void AddPriveleges(char add) {
            this.userFlags |= (Priveleges)GetModeMask(add);
        }

        #endregion

        #region Removing Priveleges

        /// <summary>
        /// Removes a privelege.
        /// </summary>
        /// <param name="add">The priveleges to remove.</param>
        public void RemovePriveleges(Priveleges remove) {
            this.userFlags &= ~remove;
        }

        /// <summary>
        /// Removes a privelege.
        /// </summary>
        /// <param name="add">The priveleges to remove.</param>
        public void RemovePriveleges(ulong remove) {
            this.userFlags &= ~(Priveleges)remove;
        }

        /// <summary>
        /// Removes a privelege.
        /// </summary>
        /// <param name="add">The priveleges to remove.</param>
        public void RemovePriveleges(char remove) {
            this.userFlags &= ~(Priveleges)GetModeMask( remove );
        }

        #endregion

        #region Setting Priveleges

        /// <summary>
        /// Sets the priveleges.
        /// </summary>
        /// <param name="set">The priveleges to set.</param>
        public void SetPriveleges(Priveleges set) {
            this.userFlags = set;
        }

        /// <summary>
        /// Sets the Priveleges.
        /// </summary>
        /// <param name="set">The priveleges to set.</param>
        public void SetPriveleges(char set) {
            this.userFlags = (Priveleges)GetModeMask( set );
        }

        /// <summary>
        /// Sets the priveleges.
        /// </summary>
        /// <param name="set">The priveleges to set.</param>
        public void SetPriveleges(ulong set) {
            this.userFlags = (Priveleges)set;
        }

        #endregion

        #endregion

    }

}
