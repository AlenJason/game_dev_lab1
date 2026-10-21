using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Một lượt chơi đã kết thúc
[Serializable]
public class RunRecord
{
    public int score;
    public int level;
    public int wave;
    public int enemiesKilled;
    public int bossesKilled;
    public string playedAt;
}

// Lưu và đọc lịch sử các lượt chơi bằng PlayerPrefs (dạng JSON)
public static class RunHistory
{
    const string Key = "RunHistory";
    const int MaxRecords = 50;

    [Serializable]
    class RunList { public List<RunRecord> runs = new List<RunRecord>(); }

    public static List<RunRecord> Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) { return new List<RunRecord>(); }
        return JsonUtility.FromJson<RunList>(json).runs;
    }

    // Lưu lượt chơi mới, trả về true nếu đây là điểm cao nhất
    public static bool Save(RunRecord record)
    {
        var runs = Load();
        bool isBest = runs.Count == 0 || record.score > runs.Max(r => r.score);

        record.playedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        runs.Add(record);
        if (runs.Count > MaxRecords) { runs.RemoveAt(0); }

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new RunList { runs = runs }));
        PlayerPrefs.Save();
        return isBest;
    }

    public static int BestScore()
    {
        var runs = Load();
        return runs.Count == 0 ? 0 : runs.Max(r => r.score);
    }
}
