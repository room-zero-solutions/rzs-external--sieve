using System;
using RzsSieve.Models;

namespace RzsSieve.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SieveAttribute : Attribute, ISievePropertyMetadata
    {
        /// <summary>
        /// Override name used
        /// </summary>
        public string Name { get; set; }

        public string FullName => Name;

        public bool CanSort { get; set; }
        public bool CanFilter { get; set; }
    }
}
