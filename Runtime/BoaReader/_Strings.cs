﻿using UnityEngine;

namespace _BOA_
{
    partial class BoaReader
    {
        public bool TryParseString(out string value)
        {
            int read_old = read_i;

            if (HasNext())
            {
                cpl_start = Mathf.Min(read_old + 1, read_i);
                cpl_end = read_i;

                char sep = default;

                if (TryReadChar_match('\'', lint: lint_theme.quotes))
                    sep = '\'';
                else if (TryReadChar_match('"', lint: lint_theme.quotes))
                    sep = '"';

                if (sep != default)
                {
                    value = string.Empty;
                    int start_i = read_i;
                    LintToThisPosition(lint_theme.quotes, false);

                    while (TryReadChar_out(out char c, skippables: null))
                        switch (c)
                        {
                            case '\\':
                                ++read_i;
                                break;

                            case '\'' or '"' when c == sep:
                                LintToThisPosition(lint_theme.strings, false, read_i - 1);
                                LintToThisPosition(lint_theme.quotes, false);
                                last_arg = value;
                                return true;

                            default:
                                value += c;
                                break;
                        }

                    if (value.TryIndexOf_min(out int err_index, true, ' ', '\t', '\n', '\r'))
                    {
                        value = value[..err_index];
                        read_i = start_i + err_index;
                    }

                    Stderr($"string error: expected closing quote '{sep}'.");
                    return false;
                }
            }

            value = null;
            read_i = read_old;
            return false;
        }
    }
}