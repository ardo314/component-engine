using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Engine.Generators;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("EngineGeneratorLoaded.g.cs", "// Engine generator loaded successfully");
        });

        // Find all interfaces decorated with [Generate]
        var services = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Engine.Core.GenerateAttribute",
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (ctx, _) => GetServiceInfo(ctx)
            )
            .Where(static info => info is not null)!;

        context.RegisterSourceOutput(
            services,
            static (ctx, service) =>
            {
                if (service is null)
                    return;

                ctx.AddSource($"{service.ServiceName}.Messages.g.cs", GenerateMessages(service));
                ctx.AddSource($"{service.ServiceName}.Client.g.cs", GenerateClient(service));
                ctx.AddSource($"{service.ServiceName}.Server.g.cs", GenerateServer(service));
            }
        );
    }

    // ────────────────────────────────────────────
    //  Extraction: Interface → ServiceInfo
    // ────────────────────────────────────────────

    private static ServiceInfo? GetServiceInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedType)
            return null;

        if (namedType.TypeKind != TypeKind.Interface)
            return null;

        var ns = namedType.ContainingNamespace.IsGlobalNamespace
            ? ""
            : namedType.ContainingNamespace.ToDisplayString();

        var interfaceName = namedType.Name;

        // Derive service name: strip leading 'I' if followed by uppercase
        var serviceName =
            interfaceName.Length > 1 && interfaceName[0] == 'I' && char.IsUpper(interfaceName[1])
                ? interfaceName.Substring(1)
                : interfaceName;

        var subjectPrefix = ns;

        // Check for attribute property overrides
        var attr = context.Attributes.FirstOrDefault();
        if (attr is not null)
        {
            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "ServiceName" && named.Value.Value is string sn && sn.Length > 0)
                {
                    serviceName = sn;
                }
                else if (
                    named.Key == "SubjectPrefix"
                    && named.Value.Value is string sp
                    && sp.Length > 0
                )
                {
                    subjectPrefix = sp;
                }
            }
        }

        // Collect methods
        var methods = new List<ServiceMethodInfo>();
        foreach (var member in namedType.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            var isRequestReply = false;
            string? replyTypeName = null;

            var returnType = method.ReturnType;
            if (returnType is INamedTypeSymbol { IsGenericType: true } generic)
            {
                var originalDef = generic.OriginalDefinition.ToDisplayString();
                if (
                    originalDef == "System.Threading.Tasks.Task<TResult>"
                    || originalDef == "System.Threading.Tasks.ValueTask<TResult>"
                )
                {
                    isRequestReply = true;
                    replyTypeName = generic
                        .TypeArguments[0]
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }

            // Collect parameters, excluding CancellationToken
            var parameters = new List<ServiceParameterInfo>();
            var hasCancellationToken = false;

            foreach (var param in method.Parameters)
            {
                var paramTypeName = param.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                );

                if (paramTypeName == "global::System.Threading.CancellationToken")
                {
                    hasCancellationToken = true;
                    continue;
                }

                parameters.Add(new ServiceParameterInfo(param.Name, paramTypeName));
            }

            methods.Add(
                new ServiceMethodInfo(
                    method.Name,
                    isRequestReply,
                    replyTypeName,
                    parameters,
                    hasCancellationToken
                )
            );
        }

        if (methods.Count == 0)
            return null;

        return new ServiceInfo(ns, interfaceName, serviceName, subjectPrefix, methods);
    }

    // ────────────────────────────────────────────
    //  Code Generation: Message DTOs
    // ────────────────────────────────────────────

    private static string GenerateMessages(ServiceInfo service)
    {
        var sb = new StringBuilder();
        AppendFileHeader(sb, service.Namespace);

        foreach (var method in service.Methods)
        {
            // Request DTO — one property per parameter
            sb.AppendLine($"public sealed class {service.ServiceName}_{method.Name}_Request");
            sb.AppendLine("{");
            foreach (var param in method.Parameters)
            {
                sb.AppendLine(
                    $"    public {param.Type} {ToPascalCase(param.Name)} {{ get; set; }} = default!;"
                );
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // Reply DTO — only for request-reply methods
            if (method.IsRequestReply && method.ReplyTypeName is not null)
            {
                sb.AppendLine($"public sealed class {service.ServiceName}_{method.Name}_Reply");
                sb.AppendLine("{");
                sb.AppendLine(
                    $"    public {method.ReplyTypeName} Result {{ get; set; }} = default!;"
                );
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // ────────────────────────────────────────────
    //  Code Generation: Client Proxy
    // ────────────────────────────────────────────

    private static string GenerateClient(ServiceInfo service)
    {
        var sb = new StringBuilder();
        AppendFileHeader(sb, service.Namespace);

        sb.AppendLine($"public sealed class {service.ServiceName}Client : {service.InterfaceName}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly global::NATS.Client.Core.INatsConnection _connection;");
        sb.AppendLine();
        sb.AppendLine(
            $"    public {service.ServiceName}Client(global::NATS.Client.Core.INatsConnection connection)"
        );
        sb.AppendLine("    {");
        sb.AppendLine("        _connection = connection;");
        sb.AppendLine("    }");

        foreach (var method in service.Methods)
        {
            sb.AppendLine();

            var subject = $"{service.SubjectPrefix}.{service.ServiceName}.{method.Name}";
            var requestType = $"{service.ServiceName}_{method.Name}_Request";
            var ct = method.HasCancellationToken ? "ct" : "default";

            // Build parameter list for the method signature
            var sigParts = new List<string>();
            foreach (var p in method.Parameters)
                sigParts.Add($"{p.Type} {p.Name}");
            if (method.HasCancellationToken)
                sigParts.Add("global::System.Threading.CancellationToken ct = default");
            var methodParams = string.Join(", ", sigParts);

            if (method.IsRequestReply && method.ReplyTypeName is not null)
            {
                var replyType = $"{service.ServiceName}_{method.Name}_Reply";
                sb.AppendLine(
                    $"    public async global::System.Threading.Tasks.Task<{method.ReplyTypeName}> {method.Name}({methodParams})"
                );
                sb.AppendLine("    {");
                AppendRequestCreation(sb, requestType, method.Parameters);
                sb.AppendLine(
                    $"        var __reply = await _connection.RequestAsync<{requestType}, {replyType}>(\"{subject}\", __request, cancellationToken: {ct}).ConfigureAwait(false);"
                );
                sb.AppendLine("        return __reply.Data!.Result;");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine(
                    $"    public async global::System.Threading.Tasks.Task {method.Name}({methodParams})"
                );
                sb.AppendLine("    {");
                AppendRequestCreation(sb, requestType, method.Parameters);
                sb.AppendLine(
                    $"        await _connection.PublishAsync(\"{subject}\", __request, cancellationToken: {ct}).ConfigureAwait(false);"
                );
                sb.AppendLine("    }");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // ────────────────────────────────────────────
    //  Code Generation: Server Base Class
    // ────────────────────────────────────────────

    private static string GenerateServer(ServiceInfo service)
    {
        var sb = new StringBuilder();
        AppendFileHeader(sb, service.Namespace);

        sb.AppendLine($"public abstract class {service.ServiceName}ServerBase");
        sb.AppendLine("{");

        // Abstract methods — one per interface method
        foreach (var method in service.Methods)
        {
            var sigParts = new List<string>();
            foreach (var p in method.Parameters)
                sigParts.Add($"{p.Type} {p.Name}");
            sigParts.Add("global::System.Threading.CancellationToken cancellationToken");
            var methodParams = string.Join(", ", sigParts);

            if (method.IsRequestReply && method.ReplyTypeName is not null)
            {
                sb.AppendLine(
                    $"    public abstract global::System.Threading.Tasks.Task<{method.ReplyTypeName}> {method.Name}({methodParams});"
                );
            }
            else
            {
                sb.AppendLine(
                    $"    public abstract global::System.Threading.Tasks.Task {method.Name}({methodParams});"
                );
            }
        }

        sb.AppendLine();

        // StartAsync — subscribes to all subjects and dispatches to handlers
        sb.AppendLine(
            "    public async global::System.Threading.Tasks.Task StartAsync(global::NATS.Client.Core.INatsConnection connection, global::System.Threading.CancellationToken cancellationToken = default)"
        );
        sb.AppendLine("    {");
        sb.AppendLine(
            "        var __tasks = new global::System.Collections.Generic.List<global::System.Threading.Tasks.Task>();"
        );
        foreach (var method in service.Methods)
        {
            sb.AppendLine(
                $"        __tasks.Add(Handle{method.Name}Async(connection, cancellationToken));"
            );
        }
        sb.AppendLine(
            "        await global::System.Threading.Tasks.Task.WhenAll(__tasks).ConfigureAwait(false);"
        );
        sb.AppendLine("    }");

        // Private handler methods — one per interface method
        foreach (var method in service.Methods)
        {
            sb.AppendLine();

            var subject = $"{service.SubjectPrefix}.{service.ServiceName}.{method.Name}";
            var requestType = $"{service.ServiceName}_{method.Name}_Request";

            sb.AppendLine(
                $"    private async global::System.Threading.Tasks.Task Handle{method.Name}Async(global::NATS.Client.Core.INatsConnection connection, global::System.Threading.CancellationToken cancellationToken)"
            );
            sb.AppendLine("    {");
            sb.AppendLine(
                $"        await foreach (var __msg in connection.SubscribeAsync<{requestType}>(\"{subject}\", cancellationToken: cancellationToken).ConfigureAwait(false))"
            );
            sb.AppendLine("        {");

            // Build argument list for the abstract method call
            var argParts = new List<string>();
            foreach (var p in method.Parameters)
                argParts.Add($"__msg.Data!.{ToPascalCase(p.Name)}");
            argParts.Add("cancellationToken");
            var args = string.Join(", ", argParts);

            if (method.IsRequestReply && method.ReplyTypeName is not null)
            {
                var replyType = $"{service.ServiceName}_{method.Name}_Reply";
                sb.AppendLine(
                    $"            var __result = await {method.Name}({args}).ConfigureAwait(false);"
                );
                sb.AppendLine(
                    $"            await __msg.ReplyAsync(new {replyType} {{ Result = __result }}).ConfigureAwait(false);"
                );
            }
            else
            {
                sb.AppendLine($"            await {method.Name}({args}).ConfigureAwait(false);");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // ────────────────────────────────────────────
    //  Helpers
    // ────────────────────────────────────────────

    private static void AppendFileHeader(StringBuilder sb, string ns)
    {
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (ns.Length > 0)
        {
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine();
        }
    }

    private static void AppendRequestCreation(
        StringBuilder sb,
        string requestType,
        List<ServiceParameterInfo> parameters
    )
    {
        sb.AppendLine($"        var __request = new {requestType}");
        sb.AppendLine("        {");
        foreach (var param in parameters)
        {
            sb.AppendLine($"            {ToPascalCase(param.Name)} = {param.Name},");
        }
        sb.AppendLine("        };");
    }

    private static string ToPascalCase(string name)
    {
        if (name.Length == 0)
            return name;
        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    // ────────────────────────────────────────────
    //  Model Classes
    // ────────────────────────────────────────────

    private sealed class ServiceInfo
    {
        public string Namespace { get; }
        public string InterfaceName { get; }
        public string ServiceName { get; }
        public string SubjectPrefix { get; }
        public List<ServiceMethodInfo> Methods { get; }

        public ServiceInfo(
            string ns,
            string interfaceName,
            string serviceName,
            string subjectPrefix,
            List<ServiceMethodInfo> methods
        )
        {
            Namespace = ns;
            InterfaceName = interfaceName;
            ServiceName = serviceName;
            SubjectPrefix = subjectPrefix;
            Methods = methods;
        }
    }

    private sealed class ServiceMethodInfo
    {
        public string Name { get; }
        public bool IsRequestReply { get; }
        public string? ReplyTypeName { get; }
        public List<ServiceParameterInfo> Parameters { get; }
        public bool HasCancellationToken { get; }

        public ServiceMethodInfo(
            string name,
            bool isRequestReply,
            string? replyTypeName,
            List<ServiceParameterInfo> parameters,
            bool hasCancellationToken
        )
        {
            Name = name;
            IsRequestReply = isRequestReply;
            ReplyTypeName = replyTypeName;
            Parameters = parameters;
            HasCancellationToken = hasCancellationToken;
        }
    }

    private sealed class ServiceParameterInfo
    {
        public string Name { get; }
        public string Type { get; }

        public ServiceParameterInfo(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
