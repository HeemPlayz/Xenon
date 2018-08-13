#region

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Xenon.Core;
using Xenon.Services;

#endregion

namespace Xenon.Modules
{
    [CheckBotOwner]
    [CommandCategory(CommandCategory.BotOwner)]
    public class BotOwnerModule : CommandBase
    {
        [Command("eval")]
        [Summary("Evaluates some code and returns the result")]
        public async Task EvaluateAsync([Remainder] string code)
        {
            code =
                $"{Regex.Match(code, PublicVariables.CodeBlockRegex, RegexOptions.Compiled | RegexOptions.Multiline).Groups[2]}";
            try
            {
                var script = CSharpScript.Create<object>(code,
                    ScriptOptions.Default
                        .WithImports("System.Math")
                        .AddReferences(Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly(),
                            Assembly.GetExecutingAssembly()), typeof(EvaluateObject));
                script.Compile();
                var result = await script.RunAsync(new EvaluateObject
                {
                    Client = Context.Client,
                    Context = Context,
                    Database = Database,
                    Random = Random,
                    Server = Server
                });
                await ReplyAsync($"{result.ReturnValue}");
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message ?? e.InnerException.Message);
            }
        }
    }
}