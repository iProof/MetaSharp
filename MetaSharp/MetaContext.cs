﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaSharp {
    public class MetaContext {
        public string Namespace { get; }
        public IEnumerable<string> Usings { get; }
        public MetaContext(string @namespace, IEnumerable<string> usings) {
            Namespace = @namespace;
            Usings = usings;
        }
    }
    public enum MetaLocationKind {
        IntermediateOutput,
        IntermediateOutputNoIntellisense,
        Designer,
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class MetaLocationAttribute : Attribute {
        public MetaLocationAttribute(MetaLocationKind location = default(MetaLocationKind)) {
            Location = location;
        }
        public MetaLocationKind Location { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class MetaIncludeAttribute : Attribute {
        public MetaIncludeAttribute(string fileName) {
            FileName = fileName;
        }
        public string FileName { get; private set; }
    }
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MetaReferenceAttribute : Attribute {
        public MetaReferenceAttribute(string dllName) {
            DllName = dllName;
        }
        public string DllName { get; private set; }
    }
}
