using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Project2Q.SDK.Algorithms {

        /// <summary>
        /// Binary Search Algorithm
        /// </summary>
        public static class BinarySearch {

            /// <summary>
            /// Searches Parse arrays by parsename.
            /// </summary>
            /// <param name="parses">The parse array.</param>
            /// <param name="parsename">The parse name.</param>
            /// <returns>Negative if not found. Index if found.</returns>
            public static int ParseBinarySearch(List<Project2Q.SDK.IRCEvents.Parse> parses, string parsename) {

                int left = 0, right = parsename.Length-1;
                int mid;

                while ( left <= right ) {
                    mid = (right + left) / 2;

                    int comp = parses[mid].ParseString.CompareTo( parsename );

                    if ( comp == 0 )
                        return mid;
                    else if ( comp < 0 )
                        left = mid+1; 
                    else if ( comp > 0 )
                        right = mid-1;
                }

                return -1;

            }

            /// <summary>
            /// Searches EventInfo arrays by name.
            /// </summary>
            /// <param name="toSearch">The array to search.</param>
            /// <param name="name">The name to search for.</param>
            /// <returns>Negative if not found. Index if found.</returns>
            public static int EventInfoBinarySearch(EventInfo[] toSearch, string name) {

                int left = 0, right = toSearch.Length-1;
                int mid;

                while ( left <= right ) {
                    mid = (right + left) / 2; //This is what I had originally -_-

                    int comp = toSearch[mid].Name.CompareTo( name );

                    if ( comp == 0 )
                        return mid;
                    else if ( comp < 0 )
                        left = mid+1; 
                    else if ( comp > 0 )
                        right = mid-1;
                }

                return -1;
            }

        }

        /// <summary>
        /// Quick sorting algorithms.
        /// </summary>
    internal static class QuickSort {

        /// <summary>
        /// Sorts EventInfo objects by name.
        /// </summary>
        /// <param name="toSort"></param>
        public static void EventInfoQuickSort(ref EventInfo[] toSort) {
            EventInfoQuickSort( ref toSort, 0, toSort.Length - 1 );
        }

        /// <summary>
        /// Sorts EventInfo objects by name.
        /// </summary>
        /// <param name="toSort">The array to sort.</param>
        /// <param name="start">The starting index.</param>
        /// <param name="end">The ending index.</param>
        public static void EventInfoQuickSort(ref EventInfo[] toSort, int start, int end) {

            if ( start >= end )
                return;

            int pivot = ( end + start ) / 2;

            pivot = Partition( ref toSort, start, end, pivot );
            EventInfoQuickSort( ref toSort, start, pivot - 1 );
            EventInfoQuickSort( ref toSort, pivot + 1, end );

        }

        /// <summary>
        /// Partitions
        /// </summary>
        /// <param name="toSort">The array to sort.</param>
        /// <param name="start">The starting index to sort.</param>
        /// <param name="end">The ending index to sort.</param>
        /// <param name="pivot">The pivot point between start + end.</param>
        /// <returns>The new pivot point.</returns>
        private static int Partition(ref EventInfo[] toSort, int start, int end, int pivot) {

            Swap( (object[])toSort, pivot, end );

            int n = start - 1;

            for ( int i = start; i < end; i++ )
                if ( string.Compare( toSort[i].Name, toSort[end].Name ) <= 0 )
                    Swap( (object[])toSort, ++n, i );

            Swap( (object[])toSort, ++n, end );

            return n;

        }

        /// <summary>
        /// Swaps objects in an array.
        /// </summary>
        /// <param name="list">The list in which to swap.</param>
        /// <param name="a">Index a.</param>
        /// <param name="b">Index b.</param>
        private static void Swap(object[] list, int a, int b) {
            object c = list[a];
            list[a] = list[b];
            list[b] = c;
        }

    }

}
