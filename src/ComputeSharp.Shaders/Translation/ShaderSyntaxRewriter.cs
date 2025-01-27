﻿using System.Collections.Generic;
using ComputeSharp.Shaders.Extensions;
using ComputeSharp.Shaders.Translation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComputeSharp.Shaders.Translation
{
    /// <summary>
    /// A custom <see cref="CSharpSyntaxRewriter"/> <see langword="class"/> that processes C# methods to convert to HLSL
    /// </summary>
    internal class ShaderSyntaxRewriter : CSharpSyntaxRewriter
    {
        /// <summary>
        /// The <see cref="Microsoft.CodeAnalysis.SemanticModel"/> instance to use to rewrite the decompiled code
        /// </summary>
        private readonly SemanticModel SemanticModel;

        /// <summary>
        /// Creates a new <see cref="ShaderSyntaxRewriter"/> instance with the specified parameters
        /// </summary>
        /// <param name="semanticModel"></param>
        public ShaderSyntaxRewriter(SemanticModel semanticModel) => SemanticModel = semanticModel;

        private readonly Dictionary<string, ReadableMember> _StaticMembers = new Dictionary<string, ReadableMember>();

        /// <summary>
        /// Gets the mapping of captured static members used by the target code
        /// </summary>
        public IReadOnlyDictionary<string, ReadableMember> StaticMembers => _StaticMembers;

        /// <inheritdoc/>
        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            node = (ParameterSyntax)base.VisitParameter(node);
            node = node.WithAttributeLists(default);
            node = node.ReplaceType(node.Type);

            return node;
        }

        /// <inheritdoc/>
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            node = (CastExpressionSyntax)base.VisitCastExpression(node);

            return node.ReplaceType(node.Type);
        }

        /// <inheritdoc/>
        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            node = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node);

            return node.ReplaceType(node.Declaration.Type);
        }

        /// <inheritdoc/>
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);
            node = node.ReplaceType(node.Type);

            if (node.ArgumentList.Arguments.Count == 0)
            {
                return SyntaxFactory.CastExpression(node.Type, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));
            }

            return SyntaxFactory.InvocationExpression(node.Type, node.ArgumentList);
        }

        /// <inheritdoc/>
        public override SyntaxNode VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            node = (DefaultExpressionSyntax)base.VisitDefaultExpression(node);
            node = node.ReplaceType(node.Type);

            return SyntaxFactory.CastExpression(node.Type, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));
        }

        /// <inheritdoc/>
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            node = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node);
            SyntaxNode syntaxNode = node.ReplaceMember(SemanticModel, out var variable);

            // Register the captured member, if any
            if (variable.HasValue && !_StaticMembers.ContainsKey(variable.Value.Name))
            {
                _StaticMembers.Add(variable.Value.Name, variable.Value.MemberInfo);
            }

            return syntaxNode;
        }
    }
}
