﻿using _COBRA_;
using _UTIL_;
using System.Collections.Generic;
using System.IO;

namespace _BOA_
{
    partial class Harbinger
    {
        static void InitCmd_Run()
        {
            Command.static_domain.AddRoutine(
                "run-boa-script",
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string script_path, out _, strict: true, path_mode: FS_TYPES.FILE))
                        exe.args.Add(script_path);
                },
                routine: ERunScript);

            static IEnumerator<CMD_STATUS> ERunScript(Command.Executor exe)
            {
                while (!exe.line.flags.HasFlag(SIG_FLAGS.TICK))
                    yield return default;

                string script_path = (string)exe.args[0];
                script_path = exe.shell.PathCheck(script_path, PathModes.ForceFull);

                if (!File.Exists(script_path))
                {
                    exe.error = $"file '{script_path}' does not exist";
                    yield break;
                }

                string script_text = File.ReadAllText(script_path);
                BoaReader reader = new(script_text);
                string error = TryParseProgram(reader, stdout => exe.Stdout(stdout), out var contractor);
                if (error != null)
                {
                    exe.error = error;
                    yield break;
                }

                var execution = contractor.EExecute();

                while (true)
                    if (!exe.line.flags.HasFlag(SIG_FLAGS.TICK))
                        yield return default;
                    else if (execution.MoveNext())
                        yield return new CMD_STATUS(progress: execution.Current.progress);
                    else
                        yield break;
            }
        }
    }
}