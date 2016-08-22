using System;
using System.Collections.Generic;

namespace TfsAdvanced.Data
{
    public enum StructureType
    {
        iteration,
        area
    }

    public class Attributes
    {
        public DateTime finishDate { get; set; }
        public DateTime startDate { get; set; }
    }

    public class CreateIteration
    {
        public Attributes attributes { get; set; }
        public string name { get; set; }
    }

    public class Iteration
    {
        public List<Iteration> children;
        public Attributes attributes { get; set; }
        public bool hasChildren { get; set; }
        public int id { get; set; }
        public string identifier { get; set; }
        public string name { get; set; }
        public Project project { get; set; }
        public StructureType structureType { get; set; }
        public string url { get; set; }
    }

    public class WorkQuery
    {
        public WorkQueryPayLoad payload { get; set; }
    }

    public class WorkQueryPayLoad
    {
        public object[][] rows { get; set; }
    }
}