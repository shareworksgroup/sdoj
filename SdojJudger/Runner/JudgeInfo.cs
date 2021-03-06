﻿namespace SdojJudger.Runner
{
    public class JudgeInfo
    {
        public string Path { get; set; }

        public string Input { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }
    }

    public class Process2JudgeInfo
    {
        public string Path1 { get; set; }

        public string Path2 { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }
    }
}
