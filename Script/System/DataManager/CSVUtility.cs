// 파일명: CSVUtility.cs (혹은 원하는 유틸리티명)
using System.Collections.Generic;

public static class CSVUtility
{
    // 💡 함수를 public static으로 선언해야 다른 파일에서 접근 가능합니다.

    public static List<string> GetLinesFromCSV(string csvText)
    {
        List<string> lines = new List<string>();
        string currentLine = "";
        bool inQuotes = false;

        // ... (함수 내용 유지) ...
        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }

            // 줄바꿈 문자 확인
            if (c == '\n')
            {
                // 큰따옴표 밖에 있을 때만 줄바꿈을 레코드의 끝으로 인식합니다.
                if (!inQuotes)
                {
                    // 캐리지 리턴(\r)이 포함되어 있다면 제거
                    lines.Add(currentLine.Trim('\r'));
                    currentLine = "";
                    continue;
                }
            }

            currentLine += c;
        }

        // 마지막 줄 추가
        if (!string.IsNullOrEmpty(currentLine.Trim('\r', '\n')))
        {
            lines.Add(currentLine.Trim('\r'));
        }

        return lines;
    }

    public static List<string> SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        // ... (함수 내용 유지) ...
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }
}