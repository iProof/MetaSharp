﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaSharp.Native;
using System.Linq.Expressions;

namespace MetaSharp {
    public class MetaContext {
        public string Namespace { get; }
        public IEnumerable<string> Usings { get; }
        public MetaContext(string @namespace, IEnumerable<string> usings) {
            Namespace = @namespace;
            Usings = usings;
        }
    }
    public static class MetaContextExtensions {
        //TODO replace all string types with tree string builder
        public static string WrapMembers(this MetaContext metaContext, string members) {
            var usings = metaContext.Usings.ConcatStringsWithNewLines();
            return 
$@"namespace {metaContext.Namespace} {{
{usings}

{members.AddIndent(4)}
}}";
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

    public enum RelativeLocation {
        Project,
        TargetPath,
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class MetaReferenceAttribute : Attribute {
        public MetaReferenceAttribute(string dllName, RelativeLocation relativeLocation = RelativeLocation.Project) {
            DllName = dllName;
            RelativeLocation = relativeLocation;
        }
        public string DllName { get; private set; }
        public RelativeLocation RelativeLocation { get; private set; }
    }

    public static class ClassGenerator {
        public static ClassGenerator<T> Class<T>() {
            throw new NotImplementedException();
        }
        public static ClassGenerator_ Class_(string name)
            => new ClassGenerator_(name);
    }
    public class ClassGenerator<T> {
        public ClassGenerator<T> Property<TProperty>(Expression<Func<T, TProperty>> property) {
            throw new NotImplementedException();
        }
        public string Generate() {
            throw new NotImplementedException();
        }
    }
    public class ClassGenerator_ {
        struct PropertyInfo {
            public readonly string Type, Name;
            public PropertyInfo(string type, string name) {
                Type = type;
                Name = name;
            }
        }
        readonly string name;
        //TODO make immutable
        readonly List<PropertyInfo> properties;
        public ClassGenerator_(string name) {
            this.name = name;
            this.properties = new List<PropertyInfo>();
        }
        public ClassGenerator_ Property_(string propertyType, string propertyName) {
            properties.Add(new PropertyInfo(propertyType, propertyName));
            return this;
        }
        public ClassGenerator_ Property<T>(string propertyName) {
            //TODO use simple name ('int' istead 'Int32')
            return Property_(typeof(T).Name, propertyName);
        }
        public string Generate() {
            var propertiesList = properties
                .Select(x => $"public {x.Type} {x.Name} {{ get; set; }}")
                .ConcatStringsWithNewLines();
            return
$@"public class {name} {{
{propertiesList.AddIndent(4)}
}}";
        }
    }
}
