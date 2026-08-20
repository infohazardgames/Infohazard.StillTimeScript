#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel;
using Infohazard.StillTimeScript.ViewModel.Annotations;
using UnityEngine;

namespace StillTime.Editor.ScriptEditor {
    public static class ScriptLineFormatter {
        private static readonly StringBuilder StringBuilder = new();

        private static readonly Color CommentColor = new(0.6f, 1.0f, 0.6f);

        private static readonly Color DefinitionColor = new(0.1f, 0.7f, 0.1f);

        private static readonly Color ErrorColor = new(1.0f, 0.1f, 0.1f);

        private static readonly Color KeywordColor = new(0.5f, 0.5f, 1.0f);

        private static readonly Color StringColor = new(0.9f, 0.8f, 0.2f);

        public static string FormatLine(StsDocumentViewModel viewModel, int index) {
            StringBuilder.Clear();
            List<LineAnnotation>? annotations = viewModel.GetAnnotations(index);

            string line = viewModel.ScriptLines[index];
            if (annotations == null) {
                AppendNoParse(line);
            } else {
                annotations.Sort((a, b) => a.Range.Start.CompareTo(b.Range.Start));
                int curIndex = 0;

                foreach (LineAnnotation annotation in annotations) {
                    if (annotation.Range.Start < curIndex) continue;

                    if (annotation.Range.Start > curIndex) {
                        AppendNoParse(line.AsSpan(curIndex, annotation.Range.Start - curIndex));
                    }

                    HandleAnnotation(annotation, line.AsSpan(annotation.Range.Start, annotation.Range.Length));
                    curIndex = annotation.Range.End;
                }

                if (curIndex < line.Length) {
                    AppendNoParse(line.AsSpan(curIndex));
                }
            }

            return StringBuilder.ToString();
        }

        private static void HandleAnnotation(LineAnnotation annotation, ReadOnlySpan<char> text) {
            switch (annotation) {
                case ColorLiteralAnnotation colorLiteralAnnotation:
                    StsColor c = colorLiteralAnnotation.Color;
                    AppendColoredText(text, new Color(c.R, c.G, c.B, c.A));
                    break;
                case CommentAnnotation:
                    AppendColoredText(text, CommentColor);
                    break;
                case DefinitionAnnotation:
                    AppendColoredText(text, DefinitionColor);
                    break;
                case DefinitionReferenceAnnotation:
                    StringBuilder.Append("<u>");
                    AppendColoredText(text, DefinitionColor);
                    StringBuilder.Append("</u>");
                    break;
                case ErrorAnnotation:
                    AppendColoredText(text, ErrorColor);
                    break;
                case KeywordAnnotation:
                    AppendColoredText(text, KeywordColor);
                    break;
                case StringAnnotation:
                    AppendColoredText(text, StringColor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(annotation));
            }
        }

        private static void AppendColoredText(ReadOnlySpan<char> text, Color color) {
            StringBuilder.Append("<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">");
            AppendNoParse(text);
            StringBuilder.Append("</color>");
        }

        private static void AppendNoParse(ReadOnlySpan<char> text) {
            StringBuilder.Append("<noparse>");
            StringBuilder.Append(text);
            StringBuilder.Append("</noparse>");
        }
    }
}
