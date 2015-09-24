﻿using MetaSharp.Native;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaSharp {
    public static class Rewriter {
        public static ImmutableArray<TreeReplacement> GetReplacements(CSharpCompilation compilation, IEnumerable<SyntaxTree> trees) {
            //TODO remove trees argument (need add included files to trees dictionary in generator and rewrite code in them as well)
            return trees
                .Select(tree => {
                    var root = tree.GetRoot();
                    var rewriter = new MetaRewriter(compilation.GetSemanticModel(tree));
                    var newRoot = rewriter.Visit(root);
                    var newTree = tree.WithRootAndOptions(newRoot, tree.Options);
                    return new TreeReplacement(tree, newTree);
                })
                .ToImmutableArray();
        }
    }
    public class MetaRewriter : CSharpSyntaxRewriter {
        //TODO improve performance by skipping non-rewritable nodes
        readonly SemanticModel model;
        public MetaRewriter(SemanticModel model) {
            this.model = model;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocationSyntax) {

            var methodNameSyntax = (invocationSyntax.Expression as MemberAccessExpressionSyntax).Name as GenericNameSyntax;
            if(methodNameSyntax == null)
                return base.VisitInvocationExpression(invocationSyntax);

            var symbol = model.GetSymbolInfo(methodNameSyntax).Symbol as IMethodSymbol;
            if(!symbol.HasAttribute<RewriteGenericArgsToStringArgsAttribute>(model.Compilation))
                return base.VisitInvocationExpression(invocationSyntax);

            var genericTypeNodes = methodNameSyntax.TypeArgumentList.Arguments; //TODO too many generic arguments
            var expression = invocationSyntax.Expression as MemberAccessExpressionSyntax;
            var newNameSyntax = (SimpleNameSyntax)SyntaxFactory.ParseName(methodNameSyntax.Identifier.ValueText);

            var newArguments = ((ArgumentListSyntax)VisitArgumentList(invocationSyntax.ArgumentList)).Arguments.ToArray();
            if(genericTypeNodes.OfType<OmittedTypeArgumentSyntax>().Any())
                return base.VisitInvocationExpression(invocationSyntax);
            var typeArgs = genericTypeNodes
                .Select(x => "\"" + x.ToFullString() + "\"")
                .ConcatStrings(", ");
            var newInvocationSyntax = invocationSyntax.Update(
                expression
                    .WithName(newNameSyntax)
                    .WithExpression((ExpressionSyntax)base.Visit(expression.Expression)),
                SyntaxFactory.ParseArgumentList($"({typeArgs})")
                    .AddArguments(newArguments)
            );

            //TODO type name is alias (using Foo = Bla.Bla.Doo);
            //TODO check syntax errors before rewriting anything
            //TODO rewrite explicit generator type (ClassGenerator g = ...; g.Property ...)

            return newInvocationSyntax;
        }
        public override SyntaxNode VisitArgument(ArgumentSyntax node) {
            var invocationSyntax = node.Parent.Parent as InvocationExpressionSyntax;
            if(invocationSyntax == null)
                return base.VisitArgument(node);
            var methodNameSyntax = (invocationSyntax.Expression as MemberAccessExpressionSyntax).Name as GenericNameSyntax;
            if(methodNameSyntax == null || methodNameSyntax.Identifier.ValueText != "Property")
                return base.VisitArgument(node);

            var lambda = node.Expression as LambdaExpressionSyntax;
            if(lambda != null) {
                //TODO check that parameter name preserved if specified, otherwise order can be broken
                return node.WithExpression(VisitLambdaExpression(lambda));
            }
            return node.WithExpression(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(node.Expression.ToFullString())));
            //return base.VisitArgument(node);
        }

        LiteralExpressionSyntax VisitLambdaExpression(LambdaExpressionSyntax lambda) {
            //TODO check parents and semantic of parents
            //var parents = lambda.GetParents();
            //var argument = lambda.Parent;
            //var symbol = model.GetSymbolInfo(argument.Parent.Parent).Symbol as IMethodSymbol;

            //TODO check lambda expression parameters (1 parameter, type is same as method's generic parameter)
            //TODO custom args order (named parameters)
            //TODO works only for 'Property' methods
            //TODO semantic model checks

            var body = (MemberAccessExpressionSyntax)lambda.Body;
            var property = body.Name.Identifier.Text;
            var literal = SyntaxFactory.Literal(property);
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, literal);
        }
    }
    public struct TreeReplacement {
        public readonly SyntaxTree Old, New;
        public TreeReplacement(SyntaxTree old, SyntaxTree @new) {
            Old = old;
            New = @new;
        }
    }
    public struct SyntaxtReplacement {
        public readonly SyntaxTree Tree;
        public readonly SyntaxNode Old, New;
        public SyntaxtReplacement(SyntaxTree tree, SyntaxNode old, SyntaxNode @new) {
            Tree = tree;
            Old = old;
            New = @new;
        }
    }
}
