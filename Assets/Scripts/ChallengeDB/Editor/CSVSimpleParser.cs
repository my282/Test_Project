using System;
using System.Collections.Generic;
using System.Text;

namespace ChallengeDB.Editor
{
    /// <summary>
    /// シンプルなCSVパーサー（RFC 4180準拠）
    /// </summary>
    public static class CSVSimpleParser
    {
        /// <summary>
        /// CSV文字列をパースして、行のリスト（Dictionary）として返す
        /// </summary>
        /// <param name="csvText">CSV文字列</param>
        /// <returns>各行のDictionary（列名 → 値）のリスト</returns>
        public static List<Dictionary<string, string>> ParseCSV(string csvText)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                throw new ArgumentException("CSV text is empty");
            }

            // BOM除去
            csvText = RemoveBOM(csvText);

            // 改行コードを統一
            csvText = csvText.Replace("\r\n", "\n").Replace("\r", "\n");

            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            List<string> headers = null;
            
            int pos = 0;
            int lineNumber = 0;

            while (pos < csvText.Length)
            {
                lineNumber++;
                List<string> row = ParseRow(csvText, ref pos);

                // 空行をスキップ
                if (row.Count == 0 || (row.Count == 1 && string.IsNullOrEmpty(row[0])))
                {
                    continue;
                }

                // ヘッダー行
                if (headers == null)
                {
                    headers = row;
                    continue;
                }

                // データ行
                if (row.Count != headers.Count)
                {
                    throw new FormatException(
                        $"Line {lineNumber}: Column count mismatch. Expected {headers.Count}, got {row.Count}");
                }

                Dictionary<string, string> rowDict = new Dictionary<string, string>();
                for (int i = 0; i < headers.Count; i++)
                {
                    rowDict[headers[i]] = row[i];
                }
                result.Add(rowDict);
            }

            if (headers == null)
            {
                throw new FormatException("CSV file has no header row");
            }

            return result;
        }

        /// <summary>
        /// 1行分のCSVをパース
        /// </summary>
        private static List<string> ParseRow(string csvText, ref int pos)
        {
            List<string> row = new List<string>();

            while (pos < csvText.Length)
            {
                // フィールドをパース
                string field = ParseField(csvText, ref pos);
                row.Add(field);

                // 次の文字を確認
                if (pos >= csvText.Length)
                {
                    break; // EOF
                }

                char nextChar = csvText[pos];
                if (nextChar == ',')
                {
                    pos++; // カンマをスキップ
                    continue; // 次のフィールドへ
                }
                else if (nextChar == '\n')
                {
                    pos++; // 改行をスキップ
                    break; // 行の終わり
                }
                else
                {
                    throw new FormatException($"Unexpected character at position {pos}: '{nextChar}'");
                }
            }

            return row;
        }

        /// <summary>
        /// 1つのフィールドをパース（ダブルクォート対応）
        /// </summary>
        private static string ParseField(string csvText, ref int pos)
        {
            if (pos >= csvText.Length)
            {
                return string.Empty;
            }

            // ダブルクォートで囲まれたフィールド
            if (csvText[pos] == '"')
            {
                return ParseQuotedField(csvText, ref pos);
            }

            // 通常のフィールド（カンマまたは改行まで）
            StringBuilder sb = new StringBuilder();
            while (pos < csvText.Length)
            {
                char c = csvText[pos];
                if (c == ',' || c == '\n')
                {
                    break;
                }
                sb.Append(c);
                pos++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// ダブルクォートで囲まれたフィールドをパース
        /// </summary>
        private static string ParseQuotedField(string csvText, ref int pos)
        {
            pos++; // 開始のダブルクォートをスキップ

            StringBuilder sb = new StringBuilder();
            while (pos < csvText.Length)
            {
                char c = csvText[pos];

                if (c == '"')
                {
                    pos++;
                    // 次の文字をチェック
                    if (pos < csvText.Length && csvText[pos] == '"')
                    {
                        // エスケープされたダブルクォート ("")
                        sb.Append('"');
                        pos++;
                    }
                    else
                    {
                        // フィールドの終わり
                        break;
                    }
                }
                else
                {
                    sb.Append(c);
                    pos++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// BOM（Byte Order Mark）を除去
        /// </summary>
        private static string RemoveBOM(string text)
        {
            if (text.Length > 0 && text[0] == '\uFEFF')
            {
                return text.Substring(1);
            }
            return text;
        }
    }
}
